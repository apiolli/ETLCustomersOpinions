# AGENTS.md — Plan de trabajo para el agente de programación

Vas a implementar la **fase de Extracción** del ETL de CustomersOpinions y a reestructurar el repositorio. Antes de escribir código, lee completos estos archivos:

1. `README.md` (raíz) — estructura objetivo del repositorio.
2. `src/Analytical/README.md` — **especificación técnica completa**. Es la fuente de verdad.
3. El código existente en `CustomersOpinions.Data/` y `CustomersOpinions.Load/` (práctica anterior) y su `README.md`.
4. Los scripts SQL de `database/`.

---

## Reglas generales

- **Idiomas:**
  - **Código en inglés:** identificadores, namespaces, carpetas, comentarios, mensajes de log, salida de consola y mensajes de commit.
  - **Documentación en español:** todos los README y los `.md` de `docs/`.
  - **No se traducen ni se renombran:** los nombres de las bases de datos; los nombres existentes de tablas, columnas y procedimientos almacenados; los encabezados de los CSV; y el contenido de los scripts ya entregados (`database/transactional/01–03` y `database/analytical/01`).
- **Framework: `net8.0`** en todos los proyectos (restricción del SRS).
- **Alcance del código C#: solo Extracción.** No implementar transformación ni componentes C# que carguen dimensiones o hechos (ver `src/Analytical/README.md` §1.2). Los procedimientos almacenados de carga, el TVP y los UDDT **ya existen** en `database/analytical/03_LoadProcedures.sql`; no se reescriben ni se llaman desde C# en esta práctica.
- **Sin proyectos de pruebas.** El estudiante no los necesita; no crear proyectos de tests.
- **Sin Entity Framework** en la parte Analytical ni en la API. Usar ADO.NET (`Microsoft.Data.SqlClient`).
- **Sin secretos en el repositorio.** Los `appsettings.json` llevan valores vacíos. Usar User Secrets (`<UserSecretsId>` en los `.csproj` del Worker y de la Api).
- **Scripts SQL entregados, no modificar:** todos los scripts de `database/` vienen listos (ver tabla siguiente). No cambiar su contenido ni crear objetos de base de datos adicionales. Si encuentras un error de sintaxis evidente, no lo corrijas en silencio: repórtalo en la lista final.
- **Estilo de código:**
  - async de punta a punta (`async`/`await`, `CancellationToken`), nullable habilitado y cero warnings de compilación;
  - clases pequeñas y cohesivas, `sealed` en las clases que no se heredan;
  - comentarios XML en las abstracciones públicas.
- **Al mover archivos,** usar `git mv` para conservar el historial.
- **Ramas y commits:**
  - trabajar en la rama `feature/extraction`;
  - hacer un commit al final de cada fase con un mensaje claro en inglés (por ejemplo, `refactor(transactional): translate load project to English`);
  - no hacer push ni abrir un PR salvo que se pida.
- **Verificación sin SQL Server:** probablemente no tengas SQL Server disponible. Verifica con `dotnet build`. Nunca afirmes que algo se ejecutó contra una base de datos si no fue así; en su lugar, deja los pasos de verificación manual.

### Scripts SQL incluidos

Todos los scripts vienen en `database/` con su nombre final:

| Script | Base | Contenido | Origen |
|---|---|---|---|
| `transactional/01_CreateDatabase.sql` | OpinionesClientesDB | modelo transaccional | práctica anterior |
| `transactional/02_Programmability.sql` | OpinionesClientesDB | procedimientos de la carga CSV → OLTP | práctica anterior |
| `transactional/03_ValidationQueries.sql` | OpinionesClientesDB | consultas de validación | práctica anterior |
| `transactional/04_ExtractionObjects.sql` | OpinionesClientesDB | vistas de extracción + `usp_GetSocialCommentsPage` (API) | **nuevo** |
| `analytical/01_CreateDataWarehouse.sql` | OpinionesClientesDW | modelo estrella | práctica anterior |
| `analytical/02_CreateStaging.sql` | OpinionesClientesDW | esquema y tablas `Staging` | **nuevo** |
| `analytical/03_LoadProcedures.sql` | OpinionesClientesDW | UDDT, función de fecha, procedimientos de dimensiones, miembros por defecto, TVP de `FactOpinion`, bulk de `FactOpinionPalabra` | **nuevo** (se usa en la próxima práctica) |

Si los scripts de prácticas anteriores también existen sueltos en otra ubicación del repositorio con sus nombres originales (`01-crearBD.sql`, `02-programabilidad.sql`, `03-consultasValidacion.sql`, `ModeladoBDAnalitica.sql`), elimina esos duplicados con `git rm`.

---

## Fase 1 — Reestructurar el repositorio y traducir la parte Transactional

Objetivo: mismo comportamiento que hoy, con la nueva estructura, en inglés y en .NET 8.

1. Crear `Directory.Build.props` en la raíz:
   ```xml
   <Project>
     <PropertyGroup>
       <TargetFramework>net8.0</TargetFramework>
       <Nullable>enable</Nullable>
       <ImplicitUsings>enable</ImplicitUsings>
       <LangVersion>latest</LangVersion>
       <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
     </PropertyGroup>
   </Project>
   ```
   Quitar de cada `.csproj` las propiedades que quedan duplicadas.
2. Mover y renombrar:
   - **Proyectos:**
     - `CustomersOpinions.Data/` → `src/Transactional/CustomersOpinions.Transactional.Data/`
     - `CustomersOpinions.Load/` → `src/Transactional/CustomersOpinions.Transactional.Load/`
   - **CSV:**
     - `CustomersOpinions.Load/CsvFiles/*.csv` → `data/csv/`
     - `CsvFiles_Sucios/*.csv` → `data/csv-dirty/`
     - Referenciarlos desde el `.csproj` de Load como elementos vinculados que se copian a la salida: `<None Include="..\..\..\data\csv\*.csv" Link="CsvFiles\%(Filename)%(Extension)" CopyToOutputDirectory="PreserveNewest" />`, y lo mismo para los sucios en `CsvFilesDirty\`.
   - **README viejo de la raíz:** pasa a `src/Transactional/README.md` y **se mantiene en español**. Hay que actualizar las rutas, los nombres de proyectos, clases y carpetas (ahora en inglés), los nombres de los scripts SQL y el argumento `sucios` → `dirty`. Se conservan el encabezado del estudiante y docente y todo el contenido (decisiones, validaciones y resultados esperados).
   - **README nuevo y `AGENTS.md`:** el `README.md` de raíz y el `AGENTS.md` que vienen con este plan reemplazan al README viejo de la raíz.
3. Cambiar ambos proyectos a `net8.0` y ajustar las versiones de los paquetes: `System.Configuration.ConfigurationManager` 8.x, `Microsoft.Data.SqlClient` 5.2.x y `CsvHelper` 33.x.
4. Renombrar los namespaces a `CustomersOpinions.Transactional.Data.*` y `CustomersOpinions.Transactional.Load`. Traducir al inglés todos los identificadores, carpetas, comentarios, mensajes de excepción, textos de consola y motivos de rechazo. Glosario sugerido:

   | Español | Inglés |
   |---|---|
   | Csvs / ClienteCsv / EncuestaCsv / ResenaCsv … | CsvRecords / CustomerCsv / SurveyCsv / WebReviewCsv … |
   | Daos | DataAccess (las clases conservan el sufijo `Dao`: `CustomerDao`, `IInsertDao<T>`, `DbSession`, `DaoBase`) |
   | Services/Cargadores, CargadorArchivo, ICargadorArchivo | Services/Loaders, FileLoader, IFileLoader |
   | Services/Lectura, ILectorCsv, FilaCsv | Services/Reading, ICsvReader, CsvRow |
   | Services/Validacion, Validador, Patron | Services/Validation, Validator, Pattern |
   | Services/Estado, IEstadoCarga, CatalogoCache, ICatalogos, ReferenciasCarga, IReferencias, Provisionales, IProvisionales | Services/State, ILoadState, CatalogCache, ICatalogs, LoadReferences, IReferences, InferredRecords, IInferredRecords |
   | ResultadoCarga, ResumenCarga, Rechazo, RechazosWriter | LoadResult, LoadSummary, Rejection, RejectionWriter |
   | Conversiones, ClaveOpinion | Conversions, OpinionKey |
   | CargaFactory, ConfiguracionCarga, ResumenConsola | LoadFactory, LoadConfiguration, ConsoleSummary |
   | Cliente, Producto, FuenteDatos, Encuesta, ComentarioSocial, ResenaWeb, Catalogo, Tabla | Customer, Product, DataSource, Survey, SocialComment, WebReview, Catalog, Table |
   | Simulacion / argumento `sucios` | DryRun / argumento `dirty` |
   | Excepciones: ValidationException, ReferentialIntegrityException, EstructuraCsvException, RegistroRechazadoException | ValidationException, ReferentialIntegrityException, CsvStructureException, RecordRejectedException |

   **Lo que se conserva tal cual:**
   - los mapeos de encabezados CSV (`[Name("Categoría")]`, `[Name("Clasificación")]`, …);
   - los nombres y parámetros de los procedimientos almacenados;
   - el `App.config`, aunque se traducen sus claves: `CsvFolder`, `DirtyCsvFolder`, `RejectedFolder`, `CreateInferredRecords`.

   La salida de rechazados pasa a ser `Rejected/rejected_yyyyMMdd_HHmmss.csv`.
5. Crear la solución única `CustomersOpinions.sln` en la raíz y borrar la anterior. Carpetas de solución:
   - `Transactional`, `Analytical` y `Tools`;
   - `Solution Items`, con el README de la raíz, `AGENTS.md`, `Directory.Build.props` y `.gitignore`.
6. Actualizar `.gitignore`: `bin/`, `obj/`, `.vs/`, `*.user`, `**/logs/`, `**/Rejected/`, `data/performance/`.

**Terminado cuando:**
- `dotnet build CustomersOpinions.sln` compila con cero warnings;
- no quedan identificadores en español en el código C#, salvo los atributos de encabezado CSV y los nombres de objetos SQL;
- el comportamiento descrito en `src/Transactional/README.md` no cambia.

---

## Fase 2 — Revisar los scripts SQL

Los scripts ya están escritos. Esta fase es solo de lectura:
1. Leer `database/transactional/04_ExtractionObjects.sql` y `database/analytical/02_CreateStaging.sql`. Los nombres de vistas, columnas, parámetros y tablas staging que usen la API y los extractores deben coincidir **exactamente** con estos scripts.
2. Leer `database/analytical/03_LoadProcedures.sql` solo como contexto. No se usa en esta práctica.

**Terminado cuando:** confirmaste que el código que vas a escribir en las fases 3 a 5 coincide con los nombres de los scripts. Cualquier discrepancia con `src/Analytical/README.md` se resuelve a favor del script y se reporta.

---

## Fase 3 — `src/Transactional/CustomersOpinions.Transactional.Api`

Web API de ASP.NET Core (.NET 8) que publica los comentarios sociales. El contrato está en `src/Analytical/README.md` §6.4.

- **Estructura:**
  - `Controllers/` o `Endpoints/` (minimal API es válido);
  - `Contracts/`: `SocialCommentResponse` y `PagedResponse<T>`;
  - `Data/`: `ISocialCommentRepository` y `SqlSocialCommentRepository`. Usa ADO.NET: `SqlCommand` con `CommandType.StoredProcedure` que llama a `dbo.usp_GetSocialCommentsPage`, parámetro de salida `@TotalCount` y lector asíncrono;
  - `Security/ApiKeyMiddleware.cs`;
  - `Configuration/PagingOptions.cs`.
- **No referencia** ningún proyecto Analytical ni Transactional.Data; se despliega de forma independiente.
- **`/health`** verifica la base con un `SELECT 1` corto. Junto con `/swagger` (solo en Development), no exige la API key.
- **`appsettings.json`:**
  - `ConnectionStrings:TransactionalDb` = "" y `Security:ApiKey` = "";
  - `Paging: { DefaultPageSize: 500, MaxPageSize: 1000 }`;
  - si falta un secreto, falla al iniciar mostrando el comando `dotnet user-secrets set ...` que lo configura.
- **`launchSettings.json`:** perfil `https` en `https://localhost:7181;http://localhost:5181`.
- Agregar un archivo `.http` con peticiones de ejemplo (con y sin API key, paginación inválida, `/health`).

**Terminado cuando:** compila y el archivo `.http` cubre los casos 200, 400 y 401.

---

## Fase 4 — Domain y Application de Analytical

Implementar `src/Analytical/README.md` §4 y §5.1–5.5: abstracciones, `ExtractionSource<T>`, `ExtractionPipeline`, reportes y `AddApplication()`. Application no debe referenciar ningún paquete de infraestructura.

**Terminado cuando:** los proyectos compilan y respetan la regla de dependencias (Domain sin referencias; Application solo referencia a Domain).

---

## Fase 5 — Infrastructure de Analytical

Implementar §5.2–5.4, §6, §8 (opciones) y §10:
- **Clases base:** `ExtractorBase<T>`, más `FileExtractorBase`, `DatabaseExtractorBase` y `ApiExtractorBase`.
- **Extractores:** `CsvSurveyExtractor`, `WebReviewDatabaseExtractor`, `CustomerDatabaseExtractor`, `ProductDatabaseExtractor` y `SocialCommentApiExtractor`.
- **Lectura de archivos:** `CsvFileReader<T>` y `FileReaderResolver`.
- **Persistencia:**
  - `IDbConnectionFactory` / `SqlConnectionFactory`, con las conexiones nombradas `TransactionalDb` y `AnalyticalDb`;
  - `SqlBulkStagingRepository<T>`, `StagingTableMap<T>` y `StagingTableMaps`.
- **Cliente de la API:** `SocialCommentsApiClient` con `IHttpClientFactory` y `AddStandardResilienceHandler()`.
- **`AddInfrastructure(IConfiguration)`:**
  - enlaza las opciones con validación al iniciar;
  - registra cada fuente como `IExtractionSource` solo mediante inyección de dependencias, de modo que agregar una fuente no obligue a tocar el pipeline.

**Terminado cuando:** compila sin warnings.

---

## Fase 6 — Worker de Analytical

Implementar §7 (host, Serilog, Windows Service, modos de ejecución, códigos de salida y reporte en consola) y el `appsettings.json` de §8.

**Terminado cuando:**
- `dotnet run --project src/Analytical/CustomersOpinions.Analytical.Worker` **sin secretos configurados** falla de inmediato con instrucciones claras, no con un stack trace;
- **con secretos pero sin base de datos,** reporta cada fuente como `Failed` con un motivo legible y el proceso termina normalmente con código 1.

---

## Fase 7 — Generador de encuestas

`tools/CustomersOpinions.Tools.SurveyGenerator` (consola, .NET 8): argumentos `--count` (por defecto 50000) y `--output`. El comportamiento está en §12. No es un proyecto de pruebas: es una herramienta para producir el archivo de la prueba de rendimiento.

**Terminado cuando:** genera un CSV con el mismo encabezado que `surveys_part1.csv` y el número de filas pedido.

---

## Fase 8 — Documentación (en español)

1. Mantener `src/Analytical/README.md` alineado con el código final, sobre todo §3.3 (árbol de carpetas), §8 (configuración) y §11 (patrones). Actualizar lo que haya cambiado durante la implementación.
2. Crear `docs/diagrams/architecture.md` y `docs/diagrams/etl-flow.md` con diagramas Mermaid: arquitectura, flujo de extracción y diagrama de secuencia de la ejecución de una fuente. Si es posible, también un archivo `architecture.drawio`.
3. `README.md` de la raíz: confirmar que los comandos del inicio rápido funcionan tal como están escritos.
4. Crear `docs/MANUAL_VERIFICATION.md`: lista paso a paso para que el estudiante verifique en una máquina con SQL Server:
   - crear las bases y cargar la OLTP;
   - configurar los secretos;
   - iniciar la API y ejecutar el Worker, con los conteos esperados;
   - correr la prueba de rendimiento con el archivo de 50.000 filas;
   - las capturas que debe tomar como evidencia.

---

## Lista final (repórtala al terminar)

- [ ] `dotnet build CustomersOpinions.sln` — 0 warnings, 0 errores.
- [ ] No hay secretos versionados (`git grep -i "password\|apikey\|Server="` solo muestra valores vacíos o documentación).
- [ ] Los proyectos Analytical respetan la regla de dependencias.
- [ ] No hay paquetes de Entity Framework ni proyectos de pruebas en la solución.
- [ ] No se agregó código C# de transformación ni de carga, y no se modificó ningún script de `database/`.
- [ ] El comportamiento de la parte Transactional no cambió (mismos conteos esperados en su README).
- [ ] Errores de sintaxis detectados en los scripts SQL (si los hay), sin corregirlos.
- [ ] Lista de lo que no se pudo verificar sin SQL Server.
- [ ] Cualquier desviación de la especificación, con su motivo.
