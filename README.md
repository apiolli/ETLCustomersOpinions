# CustomersOpinions: proceso de carga (ETL) para OpinionesClientesDB

Proceso en C# (.NET 10) que lee los 6 CSV del proyecto, valida y transforma la información e inserta en la base transaccional `OpinionesClientesDB` (SQL Server). Respeta las claves primarias y foráneas definidas en `01_CrearBaseDatos.sql`.

- Estudiante: Cesar Augusto Diaz Morillo | 2025-1962
- Materia: Electiva 1, ITLA. Docente: Francis Ramirez

---

## 1. Decisiones de diseño

| # | Decisión | Resultado |
|---|----------|-----------|
| 1 | Clientes y productos citados por las encuestas que no existen en los catálogos | **Crear registros provisionales** con `EsInferido = 1`: `Cliente_8537` sin email y `Producto_366` en la categoría `Sin categoría`. Configurable con `CrearProvisionales` en `App.config`; en `false` se rechazan por integridad referencial. |
| 2 | Acceso a datos | **ADO.NET** (`Microsoft.Data.SqlClient`) llamando a **procedimientos almacenados** |
| 3 | Arquitectura | Dos proyectos, siguiendo el ejemplo del profesor: `CustomersOpinions.Data` y `CustomersOpinions.Load` |
| 4 | Conexión | `Server=Cesar-Diaz;Database=OpinionesClientesDB;Integrated Security=True;TrustServerCertificate=True` |
| 5 | Evidencia de rechazados | Carpeta de CSV con errores sembrados a propósito (los originales no se tocan). Se ejecuta en **modo simulación**: todo se revierte al final y la base no cambia. |
| 6 | `IdFuente` de las opiniones | Queda en `NULL`: la columna `Fuente` de los CSV (EncuestaInterna, Instagram...) no indica a qué lote `F###` pertenece cada opinión. |
| 7 | `IdClasificacion` de comentarios sociales y reseñas | Queda en `NULL`, como indica el modelo ("se completa al analizar el sentimiento"). |

El script 01 crea el esquema `stg`, pero esta práctica no lo usa: carga directo desde los CSV a las tablas `dbo`.

---

## 2. Hallazgos del perfilado de datos

- 0 duplicados y 0 vacíos (salvo 88 comentarios sociales sin cliente). Las fechas siempre vienen como `yyyy-MM-dd`.
- Puntaje y rating siempre entre 1 y 5.
- Encuestas: Negativa = 1 o 2, Neutra = 3, Positiva = 4 o 5, sin excepciones. Se usa como validación de coherencia.
- Las encuestas citan 465 clientes y 302 productos que no están en los catálogos. Social y web solo usan C001-C040 y P001-P020, que sí existen.
- Social y web usan prefijos (`C019`, `P003`); en la base son enteros.
- Categorías: Electrónica, Hogar, Juguetes, Ropa y Deportes. Tipos de fuente: Web, CSV y Red Social. Redes: Facebook, Instagram y Twitter.

---

## 3. Modelo de datos (`01_CrearBaseDatos.sql`)

12 tablas en `dbo`:

| Grupo | Tablas |
|-------|--------|
| Catálogos | `Categoria`, `TipoFuente`, `RedSocial`, `Clasificacion` |
| Maestros | `Cliente`, `Producto` (ambos con `EsInferido`), `FuenteDatos` |
| Opiniones | `Encuesta`, `ComentarioSocial`, `ResenaWeb` |
| Ventas | `Venta`, `DetalleVenta`: quedan vacías porque no hay CSV de origen |

El script ya trae datos semilla: las 3 clasificaciones, los 4 tipos de fuente, las 3 redes sociales y la categoría `Sin categoría`.

---

## 4. Estructura de la solución

```
CustomersOpinions.sln
├── CustomersOpinions.Data/            (Class Library, .NET 10)
│   ├── Csvs/                          una clase por CSV (CsvHelper; [Name("Categoría")], etc.)
│   │   ├── ClienteCsv.cs, ProductoCsv.cs, FuenteDatoCsv.cs
│   │   └── EncuestaCsv.cs, ComentarioSocialCsv.cs, ResenaCsv.cs
│   ├── Models/                        Cliente, Producto, FuenteDatos, Encuesta, ComentarioSocial, ResenaWeb, Catalogo y Tabla (enums)
│   ├── Daos/                          ADO.NET + stored procedures
│   │   ├── Interfaces/                IInsertarDao<T>, IClienteDao, IProductoDao, ICatalogoDao, IExistentesDao, ITransaccionArchivo
│   │   ├── DbSession.cs               conexión + transacción por archivo (savepoints en simulación)
│   │   ├── DaoBase.cs                 ejecución común de los SP de inserción y lectura
│   │   ├── SqlParameterExtensions.cs  agregar parámetros (null → DBNull)
│   │   ├── CatalogoDao.cs             un método para los 4 catálogos
│   │   ├── ExistentesDao.cs           ids y claves de contenido ya cargados (para los HashSet)
│   │   ├── ClienteDao.cs, ProductoDao.cs, FuenteDatosDao.cs
│   │   └── EncuestaDao.cs, ComentarioSocialDao.cs, ResenaWebDao.cs
│   └── Services/
│       ├── Lectura/                   ILectorCsv + CsvReaderService (cabeceras y columnas por fila)
│       ├── Validacion/                Validador (reglas reutilizables) y Patron (formatos C###, P###...)
│       ├── Conversiones.cs            quitar prefijos C/P, fechas, nulos
│       ├── ClaveOpinion.cs            clave de contenido cliente|producto|fecha|comentario
│       ├── Estado/                    CatalogoCache, ReferenciasCarga (clientes/productos/emails), Provisionales
│       ├── Cargadores/                CargadorArchivo<TCsv, TModelo> (flujo común y duplicados) + un cargador por CSV
│       ├── LoadService.cs             orquestador: transacción por archivo y rollback
│       ├── CargaFactory.cs            raíz de composición: crea y conecta las clases concretas
│       ├── ResultadoCarga.cs          procesados / insertados / rechazados + motivos
│       ├── RechazosWriter.cs          exporta rechazados_yyyyMMdd_HHmmss.csv
│       └── Exceptions/                ValidationException, ReferentialIntegrityException, EstructuraCsvException
├── CustomersOpinions.Load/            (Console App, .NET 10)
│   ├── App.config                     cadena de conexión, carpetas, CrearProvisionales
│   ├── ConfiguracionCarga.cs          lee App.config y los argumentos
│   ├── ResumenConsola.cs              imprime el resumen en consola
│   ├── Program.cs                     punto de entrada
│   ├── CsvFiles/                      los 6 CSV originales (se copian al directorio de salida)
│   └── CsvFiles_Sucios/               CSV con errores sembrados para la evidencia
└── Scripts/
    ├── 01_CrearBaseDatos.sql          modelo existente
    ├── 02_Programabilidad.sql         procedimientos almacenados y vistas
    └── 03_ConsultasValidacion.sql     consultas de validación
```

Paquetes NuGet: `CsvHelper`, `Microsoft.Data.SqlClient`, `System.Configuration.ConfigurationManager`.

### Principios de diseño aplicados

| Principio | Cómo se aplica |
|-----------|----------------|
| **S**: responsabilidad única | Cada clase hace una cosa. `LoadService` solo maneja transacciones y rollback; cada `XxxCargador` sabe validar y cargar su CSV; `Provisionales` decide qué hacer con clientes y productos faltantes; `ConfiguracionCarga` lee la configuración y `ResumenConsola` imprime. |
| **O**: abierto/cerrado | Para cargar un CSV nuevo se crea otro `CargadorArchivo<T>` y se agrega a la lista de `CargaFactory`. No se modifica `LoadService` ni los otros cargadores. |
| **L**: sustitución de Liskov | Todo `ICargadorArchivo` se procesa igual. `ValidationException` y `ReferentialIntegrityException` se tratan como `RegistroRechazadoException`. |
| **I**: segregación de interfaces | Interfaces pequeñas: `LoadService` solo ve `ITransaccionArchivo` (no la conexión); un cargador que solo inserta recibe `IInsertarDao<T>`; `ICatalogos` e `IReferencias` están separados. |
| **D**: inversión de dependencias | Cargadores, estado y orquestador dependen de interfaces. Solo `CargaFactory` conoce las clases concretas. |
| **DRY** | El flujo leer → validar → duplicado → cargar → contar está una sola vez, en `CargadorArchivo<T>`. `DaoBase` concentra la ejecución de los SP. Las reglas (`Validador`), los formatos (`Patron`) y las conversiones se reutilizan. Hay un solo método para los 4 catálogos. |
| **KISS** | Sin contenedor de inyección de dependencias, sin repositorio genérico ni capas extra. El SQL se queda con un procedimiento por catálogo en lugar de SQL dinámico. |

---

## 5. Flujo del proceso

El orden de carga lo imponen las claves foráneas y se define en `CargaFactory`. Cada archivo se procesa en su propia transacción: si ocurre un error de estructura o de base de datos, se hace rollback del archivo y se reporta.

1. **Clientes** (`clients.csv`).
2. **Productos** (`products.csv`): la categoría se obtiene o se crea en `Categoria`.
3. **FuenteDatos** (`fuente_datos.csv`): el tipo se obtiene o se crea en `TipoFuente`.
4. **Encuestas** (`surveys_part1.csv`): usa la clasificación de `Clasificacion` y crea los clientes y productos provisionales que falten.
5. **ComentariosSociales** (`social_comments.csv`): la red se obtiene o se crea en `RedSocial`.
6. **ReseñasWeb** (`web_reviews.csv`).

Para cada registro: leer → validar → transformar → revisar duplicados → revisar integridad → insertar → contar. Un registro rechazado no detiene el proceso.

### Transformaciones

- `C019` → 19 y `P003` → 3 (quitar prefijo, convertir a entero).
- `Trim` de textos; cadena vacía → `NULL` (cliente anónimo en comentarios sociales). Email en minúsculas.
- Fechas parseadas con formato exacto `yyyy-MM-dd`.
- Clientes y productos provisionales para las encuestas (si `CrearProvisionales=true`).

### Validaciones

| Validación | Ejemplos |
|------------|----------|
| Estructura | Cabeceras esperadas por archivo; si falta una columna → `EstructuraCsvException` y se revierte el archivo. Fila con más o menos columnas → se rechaza la fila |
| Campos obligatorios | Nombre de cliente y de producto, categoría, tipo de fuente, texto del comentario, fecha, red social, fuente de la encuesta |
| Tipos de datos | Id entero positivo, fecha válida, puntaje y rating numéricos |
| Formato | Email válido; ids con patrón `C###`, `P###`, `F###`, `T####`, `W####`, con un máximo de 9 dígitos (no desborda `INT` ni `VARCHAR(10)`) |
| Rangos | Puntaje y rating entre 1 y 5; textos dentro del largo de su columna (comentario 500, nombre 100, email 150); fecha entre 2000-01-01 y hoy |
| Coherencia | Clasificación de la encuesta consistente con el puntaje |
| Duplicados | Id repetido en el archivo (comparado ya normalizado: `007` = `7`); Id que ya existe en la base; email repetido, sin distinguir mayúsculas, en el archivo o en la base; opinión repetida con otro Id (mismo cliente, producto, fecha y comentario), en el archivo o en la base |
| Integridad referencial | Social y web: el cliente y el producto deben existir. Encuestas: se crean provisionales |
| Errores | Excepciones propias. Los errores de la base de datos que afectan a una sola fila (FK, CHECK, UNIQUE) rechazan solo esa fila |

---

### Rendimiento

- **Sin consultas por fila para detectar duplicados.** Al empezar cada archivo se leen una sola vez los IDs (y, en las opiniones, las claves de contenido) que ya están en la base. Se guardan en `HashSet` con búsqueda O(1). Lo mismo pasa con los clientes, emails y productos para la integridad referencial (`ReferenciasCarga`). En una re-ejecución, los 1700 registros se rechazan en memoria sin llamar a ningún procedimiento.
- **Catálogos en caché** (`CatalogoCache`): un `Dictionary` por catálogo, que no distingue mayúsculas. Cada categoría, tipo de fuente, red social o clasificación se consulta a la base una sola vez.
- **Lectura en streaming:** `CsvReaderService` devuelve las filas una a una (`yield return`) en lugar de cargar el archivo completo en memoria.
- **Regex compiladas una sola vez** (`RegexOptions.Compiled`, campos `static readonly`).
- **Una conexión para toda la corrida** y una transacción por archivo. No se abre una conexión por registro.
- El procedimiento de inserción vuelve a revisar la PK con `IF EXISTS`, que usa el índice de la clave primaria. Es solo una red de seguridad.

Posible mejora futura, si los archivos crecen a cientos de miles de filas: insertar por lotes con `SqlBulkCopy` o parámetros con valores de tabla (TVP) en lugar de un procedimiento por fila. Con 1700 filas no hace falta y complicaría el diseño (KISS).

---

## 6. Procedimientos almacenados y vistas (`02_Programabilidad.sql`)

Procedimientos idempotentes; los de inserción devuelven `@Insertado` = 1 si insertaron el registro o 0 si ya existía:

- `usp_ObtenerOCrearCategoria`, `usp_ObtenerOCrearTipoFuente`, `usp_ObtenerOCrearRedSocial`, `usp_ObtenerOCrearClasificacion`
- `usp_InsertarCliente`, `usp_InsertarProducto`, `usp_InsertarFuenteDatos`
- `usp_ObtenerIdsExistentes`, `usp_ObtenerClavesOpiniones` (lo que ya está en la base, para detectar duplicados en memoria)
- `usp_InsertarEncuesta`, `usp_InsertarComentarioSocial`, `usp_InsertarResenaWeb`
- `usp_ObtenerClientesExistentes`, `usp_ObtenerProductosExistentes` (estado inicial para validar duplicados e integridad)

Vistas:

- `vw_OpinionesUnificadas` (ya está en el script 01): encuestas, comentarios sociales y reseñas web en una sola salida.
- `vw_ResumenPorProducto`: cantidad de opiniones por origen y puntaje promedio por producto.
- `vw_OpinionesPorClasificacion`: conteo y puntaje promedio de encuestas por clasificación.

---

## 7. Resumen de salida

Al finalizar, `Program.cs` muestra:

- una tabla por archivo con **procesados / insertados / rechazados** y el total general;
- los clientes y productos provisionales creados;
- los motivos de rechazo (los primeros 60 en pantalla).

La lista completa de rechazados se guarda en `Rechazados/rechazados_yyyyMMdd_HHmmss.csv`, dentro del directorio de salida.

Resultado esperado con los CSV originales sobre una base recién creada:

| Archivo | Procesados | Insertados | Rechazados |
|---------|-----------:|-----------:|-----------:|
| clients.csv | 500 | 500 | 0 |
| products.csv | 200 | 200 | 0 |
| fuente_datos.csv | 100 | 100 | 0 |
| surveys_part1.csv | 500 | 500 | 0 |
| social_comments.csv | 200 | 200 | 0 |
| web_reviews.csv | 200 | 200 | 0 |
| **Total** | **1700** | **1700** | **0** |

Provisionales: 465 clientes y 302 productos. Así, `Cliente` queda con 965 filas (500 + 465) y `Producto` con 502 (200 + 302).

---

## 8. Consultas de validación (`03_ConsultasValidacion.sql`)

1. Conteo de filas por tabla contra lo esperado, separando reales y provisionales (`EsInferido`).
2. Huérfanos de clave foránea en Encuesta, ComentarioSocial, ResenaWeb y Producto (debe dar 0).
3. Clientes y productos provisionales: muestra y verificación (sin email; en `Sin categoría`).
4. Duplicados de email y de opiniones por contenido (debe dar 0).
5. Puntajes y ratings fuera de 1-5, clasificación incoherente y fechas futuras (debe dar 0).
6. Comentarios sociales anónimos (`IdCliente IS NULL`, esperado 88), por red social.
7. Vistas `vw_ResumenPorProducto`, `vw_OpinionesPorClasificacion` y conteo por origen.
8. `SELECT TOP 20 *` de las 12 tablas para la evidencia.

---

## 9. Cómo ejecutar

1. En SSMS ejecutar, en orden: `01_CrearBaseDatos.sql` y `02_Programabilidad.sql`.
2. Abrir `CustomersOpinions.sln` en Visual Studio 2026 (o VS Code con el SDK de .NET 10) y verificar la cadena de conexión en `CustomersOpinions.Load/App.config`.
3. Ejecutar `CustomersOpinions.Load`:
   - Carga normal con los CSV originales: `dotnet run --project CustomersOpinions.Load`
   - Prueba de validaciones con datos sucios (simulación, no guarda nada): `dotnet run --project CustomersOpinions.Load -- sucios`
     En Visual Studio: Propiedades del proyecto → Depurar → Argumentos de la línea de comandos: `sucios`.
4. Ejecutar `03_ConsultasValidacion.sql` y capturar los resultados.

La carga es re-ejecutable: si se corre otra vez, los registros ya existentes se reportan como rechazados por duplicado y no se vuelven a insertar.

---

## 10. Evidencias a entregar

- Captura de la ejecución con el resumen (procesados, insertados, rechazados).
- Captura de las 12 tablas pobladas (sección 8 del script 03).
- Captura de la ejecución con datos sucios mostrando los rechazos y sus motivos.
- Captura de las consultas de validación.

---

## 11. CSV sucios de prueba (`CsvFiles_Sucios/`)

Son copias reducidas con IDs nuevos (9xxx, `T9xxx`, `W9xxx`) para que los errores sembrados no se confundan con "ya existe en la base".

| Archivo | Errores sembrados |
|---------|-------------------|
| clients.csv | Id repetido, nombre vacío, email repetido, email con formato inválido, id no numérico (`ABC`), email que ya existe en la base (`cliente1@mail.com`, solo si ya se cargaron los originales) |
| products.csv | Nombre vacío, categoría vacía, id negativo; categoría nueva `Mascotas` (válida, se crea en el catálogo) |
| fuente_datos.csv | Falta la columna `FechaCarga` → error de estructura, se revierte el archivo completo |
| surveys_part1.csv | Puntaje 0, Positiva con puntaje 1, fecha `2025-13-45`, comentario vacío, clasificación `Mixta`, id repetido, fecha futura, fecha `1899-12-31`, fuente vacía, opinión repetida con otro Id (90009 = 90001); cliente 9999 / producto 9998 crean provisionales |
| social_comments.csv | Id `CXYZ`, comentario vacío, red social vacía, cliente inexistente `C8888`, comentario de más de 500 caracteres, id repetido, id `C99999999999` (desbordaría INT); un comentario anónimo válido |
| web_reviews.csv | Rating 9, rating vacío, rating `cinco`, producto inexistente `P7777`, reseña sin cliente, fecha `01/05/2025`, fila con una columna de más, reseña repetida con otro Id (W9009 = W9001) |

Resultado esperado de la simulación:

| Archivo | Procesados | Válidos | Rechazados |
|---------|-----------:|--------:|-----------:|
| clients.csv | 8 | 2 (3 si la base está vacía) | 6 (5) |
| products.csv | 5 | 2 | 3 |
| fuente_datos.csv | 0 | 0 | 0, revertido por estructura |
| surveys_part1.csv | 12 | 2 | 10 |
| social_comments.csv | 9 | 2 | 7 |
| web_reviews.csv | 9 | 1 | 8 |
