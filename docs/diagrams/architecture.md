# Arquitectura de la solución

Diagramas en Mermaid (se ven directamente en GitHub y en VS Code con la extensión de Mermaid).

## Proyectos y regla de dependencias

Las flechas indican "referencia a". Domain no referencia nada; Application solo a Domain; el Worker es la raíz de composición. La API se despliega por separado y no referencia a ningún otro proyecto.

```mermaid
flowchart BT
    subgraph Transactional
        TD[Transactional.Data<br/>carga CSV → OLTP]
        TL[Transactional.Load<br/>consola] --> TD
        TA[Transactional.Api<br/>comentarios sociales]
    end

    subgraph Analytical
        D[Analytical.Domain<br/>Result, Staging*, Dim*, Fact*]
        A[Analytical.Application<br/>abstracciones, pipeline, reportes] --> D
        I[Analytical.Infrastructure<br/>CsvHelper, SqlClient, HttpClient] --> A
        I --> D
        W[Analytical.Worker<br/>host, Serilog, Servicio de Windows] --> A
        W --> I
    end

    subgraph Tools
        G[Tools.SurveyGenerator<br/>archivo de 50.000 filas]
    end
```

## Componentes en ejecución

```mermaid
flowchart LR
    CSV[/data/csv/surveys_part1.csv/]
    OLTP[(OpinionesClientesDB)]
    DW[(OpinionesClientesDW<br/>esquema Staging)]

    subgraph API["Transactional.Api (HTTPS + X-Api-Key)"]
        EP[GET /api/v1/social-comments] --> SP[[dbo.usp_GetSocialCommentsPage]]
    end
    SP --> OLTP

    subgraph Worker["Analytical.Worker"]
        EW[ExtractionWorker] --> P[ExtractionPipeline]
        P --> S1[ExtractionSource: Surveys]
        P --> S2[ExtractionSource: WebReviews]
        P --> S3[ExtractionSource: Customers]
        P --> S4[ExtractionSource: Products]
        P --> S5[ExtractionSource: SocialComments]
    end

    S1 -- CsvSurveyExtractor + CsvFileReader --> CSV
    S2 -- vw_ExtractWebReviews --> OLTP
    S3 -- vw_ExtractCustomers --> OLTP
    S4 -- vw_ExtractProducts --> OLTP
    S5 -- SocialCommentsApiClient<br/>IHttpClientFactory + resiliencia --> EP

    S1 & S2 & S3 & S4 & S5 -- SqlBulkStagingRepository<br/>SqlBulkCopy --> DW
```

## Jerarquía de extractores

```mermaid
classDiagram
    class IExtractor {
        <<interface>>
        +Name string
        +SourceKind SourceKind
    }
    class IExtractor~TRecord~ {
        <<interface>>
        +ExtractAsync(ct) IAsyncEnumerable~Result~TRecord~~
    }
    class ExtractorBase~TRecord~ {
        <<abstract>>
        +ExtractAsync(ct) plantilla
        #ExtractCoreAsync(ct)*
    }
    class FileExtractorBase~TRow,TRecord~ {
        <<abstract>>
        #Map(TRow)*
    }
    class DatabaseExtractorBase~TRecord~ {
        <<abstract>>
        #Query*
        #Map(DbDataReader)*
    }
    class ApiExtractorBase~TDto,TRecord~ {
        <<abstract>>
        #GetPageAsync(page, ct)*
        #KeyOf(TDto)*
        #Map(TDto)*
    }
    IExtractor <|-- IExtractor~TRecord~
    IExtractor~TRecord~ <|.. ExtractorBase~TRecord~
    ExtractorBase~TRecord~ <|-- FileExtractorBase~TRow,TRecord~
    ExtractorBase~TRecord~ <|-- DatabaseExtractorBase~TRecord~
    ExtractorBase~TRecord~ <|-- ApiExtractorBase~TDto,TRecord~
    FileExtractorBase~TRow,TRecord~ <|-- CsvSurveyExtractor
    DatabaseExtractorBase~TRecord~ <|-- WebReviewDatabaseExtractor
    DatabaseExtractorBase~TRecord~ <|-- CustomerDatabaseExtractor
    DatabaseExtractorBase~TRecord~ <|-- ProductDatabaseExtractor
    ApiExtractorBase~TDto,TRecord~ <|-- SocialCommentApiExtractor
```
