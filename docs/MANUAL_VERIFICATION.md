# Verificación manual (máquina con SQL Server)

Lista paso a paso para comprobar la solución completa. Los pasos se ejecutan desde la raíz del repositorio en PowerShell. Marca cada casilla y toma las capturas indicadas con 📸.

## 0. Requisitos

- [ ] SDK de .NET 8 instalado (`dotnet --list-sdks` muestra una versión `8.0.x`). Con solo el SDK de .NET 10 los proyectos compilan, pero no se ejecutan sin el runtime de .NET 8.
- [ ] SQL Server 2019+ local y SSMS.
- [ ] Certificado de desarrollo HTTPS de confianza: `dotnet dev-certs https --trust`.
- [ ] `dotnet build CustomersOpinions.sln` termina con **0 warnings y 0 errores**. 📸

## 1. Crear las bases de datos

En SSMS ejecutar, en este orden:

1. [ ] `database/transactional/01_CreateDatabase.sql`
2. [ ] `database/transactional/02_Programmability.sql`
3. [ ] `database/transactional/04_ExtractionObjects.sql`
4. [ ] `database/analytical/01_CreateDataWarehouse.sql`
5. [ ] `database/analytical/02_CreateStaging.sql`
6. [ ] `database/analytical/03_LoadProcedures.sql`
7. [ ] `exec OpinionesClientesDW.dbo.usp_SeedDimensionMembers;`

Comprobar que existen las vistas `dbo.vw_ExtractWebReviews`, `dbo.vw_ExtractCustomers`, `dbo.vw_ExtractProducts`, el procedimiento `dbo.usp_GetSocialCommentsPage` y las 5 tablas del esquema `Staging`. 📸

## 2. Cargar la base transaccional

- [ ] Revisar la cadena de conexión en `src/Transactional/CustomersOpinions.Transactional.Load/App.config` (servidor de tu máquina).
- [ ] `dotnet run --project src/Transactional/CustomersOpinions.Transactional.Load`
- [ ] Resultado esperado: 1700 procesados / 1700 insertados / 0 rechazados; 465 clientes y 302 productos inferidos. 📸
- [ ] (Opcional) `dotnet run --project src/Transactional/CustomersOpinions.Transactional.Load -- dirty`: muestra los rechazos de los CSV sucios y no cambia la base. 📸

## 3. Configurar los secretos

Generar una clave para la API:

```powershell
$key = [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
```

API:

```powershell
dotnet user-secrets set "ConnectionStrings:TransactionalDb" "Server=.;Database=OpinionesClientesDB;Integrated Security=True;TrustServerCertificate=True" --project src/Transactional/CustomersOpinions.Transactional.Api
dotnet user-secrets set "Security:ApiKey" $key --project src/Transactional/CustomersOpinions.Transactional.Api
```

Worker:

```powershell
dotnet user-secrets set "ConnectionStrings:TransactionalDb" "Server=.;Database=OpinionesClientesDB;Integrated Security=True;TrustServerCertificate=True" --project src/Analytical/CustomersOpinions.Analytical.Worker
dotnet user-secrets set "ConnectionStrings:AnalyticalDb" "Server=.;Database=OpinionesClientesDW;Integrated Security=True;TrustServerCertificate=True" --project src/Analytical/CustomersOpinions.Analytical.Worker
dotnet user-secrets set "Sources:SocialComments:ApiKey" $key --project src/Analytical/CustomersOpinions.Analytical.Worker
```

Cambia `Server=.` si tu instancia tiene otro nombre (por ejemplo `.\SQLEXPRESS`).

- [ ] Evidencia de seguridad: antes de configurar los secretos, `dotnet run --project src/Analytical/CustomersOpinions.Analytical.Worker` falla de inmediato y lista los comandos `dotnet user-secrets set ...` que faltan, sin stack trace. 📸

## 4. Iniciar la API

Terminal 1:

```powershell
dotnet run --project src/Transactional/CustomersOpinions.Transactional.Api --launch-profile https
```

- [ ] `https://localhost:7181/health` responde `{"status":"Healthy"}`. 📸
- [ ] Swagger en `https://localhost:7181/swagger`. 📸
- [ ] Con `src/Transactional/CustomersOpinions.Transactional.Api/CustomersOpinions.Transactional.Api.http` (pegar la clave en `@apiKey`):
  - [ ] 200 con clave: `totalCount` = 200. 📸
  - [ ] 400 con `pageNumber=0`. 📸
  - [ ] 401 sin clave y con clave incorrecta. 📸

## 5. Ejecutar la extracción

Terminal 2:

```powershell
dotnet run --project src/Analytical/CustomersOpinions.Analytical.Worker
```

- [ ] El reporte muestra las 5 fuentes en `OK` y el proceso termina con código 0 (`$LASTEXITCODE`). 📸
- [ ] Revisar los conteos en SSMS:

```sql
select 'Surveys' t, count(*) from OpinionesClientesDW.Staging.Surveys union all
select 'WebReviews', count(*) from OpinionesClientesDW.Staging.WebReviews union all
select 'SocialComments', count(*) from OpinionesClientesDW.Staging.SocialComments union all
select 'Customers', count(*) from OpinionesClientesDW.Staging.Customers union all
select 'Products', count(*) from OpinionesClientesDW.Staging.Products;
```

| Tabla staging | Filas esperadas |
|---|---|
| Surveys | 500 |
| WebReviews | 200 |
| SocialComments | 200 (88 con `CustomerId` null) |
| Customers | 965 |
| Products | 502 |

📸 del resultado y de un `select top 10 *` de cada tabla (se ven `SourceFile` / `SourceObject` / `SourceEndpoint` y `ExtractedAt`).

- [ ] Re-ejecutar el Worker: los conteos no cambian (refresco completo, no se duplican filas).
- [ ] Revisar el log del día en `src/Analytical/CustomersOpinions.Analytical.Worker/bin/Debug/net8.0/logs/extraction-yyyyMMdd.log`. 📸

## 6. Aislamiento de fuentes

- [ ] Detener la API (Ctrl+C en la terminal 1) y ejecutar el Worker otra vez: `SocialComments` sale `FAILED` con un motivo legible, las otras 4 fuentes salen `OK`, el código de salida es 1 y `Staging.SocialComments` **conserva** las 200 filas de la ejecución anterior. 📸
- [ ] Desactivar una fuente solo por configuración: `dotnet run --project src/Analytical/CustomersOpinions.Analytical.Worker -- --Sources:Products:Enabled=false` → `Products` sale `SKIPPED`. 📸

## 7. Prueba de rendimiento (50.000 encuestas en menos de 5 minutos)

```powershell
dotnet run --project tools/CustomersOpinions.Tools.SurveyGenerator -- --count 50000 --output data/performance/surveys_50k.csv
dotnet run --project src/Analytical/CustomersOpinions.Analytical.Worker -- --Sources:Surveys:FilePath="$PWD\data\performance\surveys_50k.csv" --Sources:WebReviews:Enabled=false --Sources:Customers:Enabled=false --Sources:Products:Enabled=false --Sources:SocialComments:Enabled=false
```

- [ ] El reporte muestra `Surveys (CSV)` con 50.000 leídas y 50.000 escritas; anotar el tiempo y las filas por segundo (lo esperado es unos pocos segundos). 📸
- [ ] `select count(*) from OpinionesClientesDW.Staging.Surveys;` devuelve 50000. 📸
- [ ] Volver a ejecutar el paso 5 para dejar staging con los datos originales.

## 8. (Opcional) Servicio de Windows

Ver `src/Analytical/README.md` §13, "Instalar como Servicio de Windows". 📸 de `sc.exe query "CustomersOpinions Extraction"`.

## Lista de capturas para la entrega

1. Compilación sin warnings.
2. Objetos creados en SSMS.
3. Carga transaccional (y la simulación con datos sucios).
4. Worker sin secretos (mensaje de configuración).
5. `/health`, Swagger y las respuestas 200 / 400 / 401.
6. Reporte del Worker con las 5 fuentes OK y los conteos de staging.
7. Log de Serilog.
8. Fuente API caída (aislamiento) y fuente deshabilitada.
9. Prueba de rendimiento de 50.000 filas.
