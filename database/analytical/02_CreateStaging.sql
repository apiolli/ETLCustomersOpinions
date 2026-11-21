/* ============================================================================
   Sistema de Análisis de Opiniones de Clientes
   Script 02 - Esquema y tablas staging de OpinionesClientesDW
   ----------------------------------------------------------------------------
   Reciben los datos tal como los entrega cada fuente (texto crudo + metadatos).
   Son tablas heap sin llaves ni restricciones: la validación y la deduplicación
   pertenecen a la fase de Transformación, y los heaps aprovechan al máximo la
   inserción masiva (SqlBulkCopy con TABLOCK).
   Script idempotente: puede ejecutarse varias veces.
   ============================================================================ */

use OpinionesClientesDW;
go

if schema_id(N'Staging') is null
    exec (N'create schema Staging');
go

-- Encuestas internas (CSV: surveys_part1.csv)
if object_id(N'Staging.Surveys', N'U') is null
create table Staging.Surveys (
    SurveyId          varchar(50)  null,
    CustomerId        varchar(50)  null,
    ProductId         varchar(50)  null,
    SurveyDate        varchar(50)  null,
    Comment           varchar(max) null,
    Classification    varchar(50)  null,
    SatisfactionScore varchar(20)  null,
    Source            varchar(100) null,
    SourceFile        varchar(260) not null,   -- metadato: nombre del archivo
    ExtractedAt       datetime2(0) not null    -- metadato: fecha de extracción (UTC)
);
go

-- Reseñas web (BD relacional: dbo.vw_ExtractWebReviews)
if object_id(N'Staging.WebReviews', N'U') is null
create table Staging.WebReviews (
    ReviewId     varchar(50)  null,
    CustomerId   varchar(50)  null,
    ProductId    varchar(50)  null,
    ReviewDate   varchar(50)  null,
    Comment      varchar(max) null,
    Rating       varchar(20)  null,
    SourceObject varchar(128) not null,        -- metadato: vista de origen
    ExtractedAt  datetime2(0) not null
);
go

-- Comentarios en redes sociales (API REST)
if object_id(N'Staging.SocialComments', N'U') is null
create table Staging.SocialComments (
    CommentId      varchar(50)  null,
    CustomerId     varchar(50)  null,          -- null = comentario anónimo
    ProductId      varchar(50)  null,
    Platform       varchar(50)  null,
    CommentDate    varchar(50)  null,
    Comment        varchar(max) null,
    SourceEndpoint varchar(500) not null,      -- metadato: URL del endpoint (sin credenciales)
    ExtractedAt    datetime2(0) not null
);
go

-- Clientes (BD relacional: dbo.vw_ExtractCustomers) -> DimCliente
if object_id(N'Staging.Customers', N'U') is null
create table Staging.Customers (
    CustomerId   varchar(50)  null,
    Name         varchar(200) null,
    Email        varchar(200) null,
    IsInferred   varchar(5)   null,
    SourceObject varchar(128) not null,
    ExtractedAt  datetime2(0) not null
);
go

-- Productos (BD relacional: dbo.vw_ExtractProducts) -> DimProducto
if object_id(N'Staging.Products', N'U') is null
create table Staging.Products (
    ProductId    varchar(50)  null,
    Name         varchar(200) null,
    Category     varchar(100) null,
    IsInferred   varchar(5)   null,
    SourceObject varchar(128) not null,
    ExtractedAt  datetime2(0) not null
);
go

print 'Esquema Staging de OpinionesClientesDW creado correctamente.';
go
