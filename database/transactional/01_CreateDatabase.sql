/* ============================================================================
   Sistema de Análisis de Opiniones de Clientes
   Script 01 - Creación de la base de datos, esquemas y tablas
   Motor: Microsoft SQL Server 2019+ (T-SQL)
   ----------------------------------------------------------------------------
   Esquemas:
     dbo  -> modelo relacional final (catálogos, ventas y opiniones)
     stg  -> tablas de staging que reciben los CSV tal como vienen
   ============================================================================ */

use master;
go

if db_id(N'OpinionesClientesDB') is not null
begin
    alter database OpinionesClientesDB set single_user with rollback immediate;
    drop database OpinionesClientesDB;
end
go

create database OpinionesClientesDB
    collate Modern_Spanish_CI_AS;
go

use OpinionesClientesDB;
go

create schema stg;
go

/* ============================================================================
   1. CATÁLOGOS
   ============================================================================ */

-- Categorías de producto (Electrónica, Hogar, Juguetes, Ropa, Deportes, ...)
create table dbo.Categoria (
    IdCategoria     int identity(1,1) not null,
    Nombre          varchar(50)       not null,
    constraint PK_Categoria        primary key (IdCategoria),
    constraint UQ_Categoria_Nombre unique (Nombre)
);
go

-- Productos (products.csv)
create table dbo.Producto (
    IdProducto      int               not null,
    Nombre          varchar(100)      not null,
    IdCategoria     int               not null,
    EsInferido      bit               not null constraint DF_Producto_EsInferido default (0),
    FechaRegistro   datetime2(0)      not null constraint DF_Producto_FechaRegistro default (sysdatetime()),
    constraint PK_Producto primary key (IdProducto),
    constraint FK_Producto_Categoria foreign key (IdCategoria)
        references dbo.Categoria (IdCategoria)
);
go

-- Clientes (clients.csv)
create table dbo.Cliente (
    IdCliente       int               not null,
    Nombre          varchar(100)      not null,
    Email           varchar(150)      null,
    EsInferido      bit               not null constraint DF_Cliente_EsInferido default (0),
    FechaRegistro   datetime2(0)      not null constraint DF_Cliente_FechaRegistro default (sysdatetime()),
    constraint PK_Cliente primary key (IdCliente),
    constraint CK_Cliente_Email check (Email is null or Email like '%_@_%._%')
);
go
-- Email único cuando existe (los clientes inferidos no tienen email)
create unique index UX_Cliente_Email on dbo.Cliente (Email) where Email is not null;
go

-- Tipos de fuente (Web, CSV, Red Social, Encuesta Interna)
create table dbo.TipoFuente (
    IdTipoFuente    int identity(1,1) not null,
    Nombre          varchar(50)       not null,
    constraint PK_TipoFuente        primary key (IdTipoFuente),
    constraint UQ_TipoFuente_Nombre unique (Nombre)
);
go

-- Fuentes / lotes de carga de datos (fuente_datos.csv)
create table dbo.FuenteDatos (
    IdFuente        varchar(10)       not null,
    IdTipoFuente    int               not null,
    FechaCarga      date              not null,
    constraint PK_FuenteDatos primary key (IdFuente),
    constraint FK_FuenteDatos_TipoFuente foreign key (IdTipoFuente)
        references dbo.TipoFuente (IdTipoFuente)
);
go

-- Plataformas de redes sociales (Facebook, Instagram, Twitter)
create table dbo.RedSocial (
    IdRedSocial     int identity(1,1) not null,
    Nombre          varchar(50)       not null,
    constraint PK_RedSocial        primary key (IdRedSocial),
    constraint UQ_RedSocial_Nombre unique (Nombre)
);
go

-- Clasificación de sentimiento (Positiva, Neutra, Negativa)
create table dbo.Clasificacion (
    IdClasificacion int identity(1,1) not null,
    Nombre          varchar(20)       not null,
    constraint PK_Clasificacion        primary key (IdClasificacion),
    constraint UQ_Clasificacion_Nombre unique (Nombre)
);
go

/* ============================================================================
   2. VENTAS  (entidad requerida por el proyecto; no hay CSV de origen todavía)
   ============================================================================ */

create table dbo.Venta (
    IdVenta         int identity(1,1) not null,
    IdCliente       int               not null,
    FechaVenta      date              not null,
    Total           decimal(12,2)     not null constraint DF_Venta_Total default (0),
    constraint PK_Venta primary key (IdVenta),
    constraint FK_Venta_Cliente foreign key (IdCliente)
        references dbo.Cliente (IdCliente),
    constraint CK_Venta_Total check (Total >= 0)
);
go

create table dbo.DetalleVenta (
    IdVenta         int               not null,
    IdProducto      int               not null,
    Cantidad        int               not null,
    PrecioUnitario  decimal(10,2)     not null,
    Subtotal        as (Cantidad * PrecioUnitario) persisted,
    constraint PK_DetalleVenta primary key (IdVenta, IdProducto),
    constraint FK_DetalleVenta_Venta foreign key (IdVenta)
        references dbo.Venta (IdVenta) on delete cascade,
    constraint FK_DetalleVenta_Producto foreign key (IdProducto)
        references dbo.Producto (IdProducto),
    constraint CK_DetalleVenta_Cantidad check (Cantidad > 0),
    constraint CK_DetalleVenta_Precio   check (PrecioUnitario >= 0)
);
go

/* ============================================================================
   3. OPINIONES (tres orígenes)
   ============================================================================ */

-- Encuestas internas (surveys_part1.csv)
create table dbo.Encuesta (
    IdOpinion           int           not null,
    IdCliente           int           not null,
    IdProducto          int           not null,
    IdFuente            varchar(10)   null,
    Fecha               date          not null,
    Comentario          varchar(500)  not null,
    IdClasificacion     int           not null,
    PuntajeSatisfaccion tinyint       not null,
    constraint PK_Encuesta primary key (IdOpinion),
    constraint FK_Encuesta_Cliente       foreign key (IdCliente)       references dbo.Cliente (IdCliente),
    constraint FK_Encuesta_Producto      foreign key (IdProducto)      references dbo.Producto (IdProducto),
    constraint FK_Encuesta_FuenteDatos   foreign key (IdFuente)        references dbo.FuenteDatos (IdFuente),
    constraint FK_Encuesta_Clasificacion foreign key (IdClasificacion) references dbo.Clasificacion (IdClasificacion),
    constraint CK_Encuesta_Puntaje check (PuntajeSatisfaccion between 1 and 5)
);
go

-- Comentarios en redes sociales (social_comments.csv)
create table dbo.ComentarioSocial (
    IdComment           varchar(10)   not null,
    IdCliente           int           null,          -- comentarios anónimos permitidos
    IdProducto          int           not null,
    IdRedSocial         int           not null,
    IdFuente            varchar(10)   null,
    Fecha               date          not null,
    Comentario          varchar(500)  not null,
    IdClasificacion     int           null,          -- se completa al analizar el sentimiento
    constraint PK_ComentarioSocial primary key (IdComment),
    constraint FK_ComentarioSocial_Cliente       foreign key (IdCliente)       references dbo.Cliente (IdCliente),
    constraint FK_ComentarioSocial_Producto      foreign key (IdProducto)      references dbo.Producto (IdProducto),
    constraint FK_ComentarioSocial_RedSocial     foreign key (IdRedSocial)     references dbo.RedSocial (IdRedSocial),
    constraint FK_ComentarioSocial_FuenteDatos   foreign key (IdFuente)        references dbo.FuenteDatos (IdFuente),
    constraint FK_ComentarioSocial_Clasificacion foreign key (IdClasificacion) references dbo.Clasificacion (IdClasificacion)
);
go

-- Reseñas web (web_reviews.csv)
create table dbo.ResenaWeb (
    IdReview            varchar(10)   not null,
    IdCliente           int           not null,
    IdProducto          int           not null,
    IdFuente            varchar(10)   null,
    Fecha               date          not null,
    Comentario          varchar(500)  not null,
    Rating              tinyint       not null,
    IdClasificacion     int           null,
    constraint PK_ResenaWeb primary key (IdReview),
    constraint FK_ResenaWeb_Cliente       foreign key (IdCliente)       references dbo.Cliente (IdCliente),
    constraint FK_ResenaWeb_Producto      foreign key (IdProducto)      references dbo.Producto (IdProducto),
    constraint FK_ResenaWeb_FuenteDatos   foreign key (IdFuente)        references dbo.FuenteDatos (IdFuente),
    constraint FK_ResenaWeb_Clasificacion foreign key (IdClasificacion) references dbo.Clasificacion (IdClasificacion),
    constraint CK_ResenaWeb_Rating check (Rating between 1 and 5)
);
go

/* ============================================================================
   4. ÍNDICES PARA CONSULTAS DE ANÁLISIS (FK + fecha)
   ============================================================================ */
create index IX_Producto_Categoria          on dbo.Producto (IdCategoria);
create index IX_FuenteDatos_Tipo            on dbo.FuenteDatos (IdTipoFuente);
create index IX_Venta_Cliente               on dbo.Venta (IdCliente, FechaVenta);
create index IX_DetalleVenta_Producto       on dbo.DetalleVenta (IdProducto);
create index IX_Encuesta_Producto_Fecha     on dbo.Encuesta (IdProducto, Fecha);
create index IX_Encuesta_Cliente            on dbo.Encuesta (IdCliente);
create index IX_ComentarioSocial_Prod_Fecha on dbo.ComentarioSocial (IdProducto, Fecha);
create index IX_ComentarioSocial_Cliente    on dbo.ComentarioSocial (IdCliente);
create index IX_ResenaWeb_Producto_Fecha    on dbo.ResenaWeb (IdProducto, Fecha);
create index IX_ResenaWeb_Cliente           on dbo.ResenaWeb (IdCliente);
go

/* ============================================================================
   5. VISTA UNIFICADA DE OPINIONES (base para el análisis)
   ============================================================================ */
create view dbo.vw_OpinionesUnificadas
as
select  'Encuesta'            as Origen,
        cast(e.IdOpinion as varchar(10)) as IdRegistro,
        e.IdCliente, e.IdProducto, e.Fecha, e.Comentario,
        e.PuntajeSatisfaccion as Puntaje,
        c.Nombre              as Clasificacion,
        'Encuesta Interna'    as Canal
from dbo.Encuesta e
join dbo.Clasificacion c on c.IdClasificacion = e.IdClasificacion
union all
select  'Red Social', s.IdComment, s.IdCliente, s.IdProducto, s.Fecha, s.Comentario,
        null, cl.Nombre, r.Nombre
from dbo.ComentarioSocial s
join dbo.RedSocial r          on r.IdRedSocial = s.IdRedSocial
left join dbo.Clasificacion cl on cl.IdClasificacion = s.IdClasificacion
union all
select  'Reseña Web', w.IdReview, w.IdCliente, w.IdProducto, w.Fecha, w.Comentario,
        w.Rating, cl.Nombre, 'Web'
from dbo.ResenaWeb w
left join dbo.Clasificacion cl on cl.IdClasificacion = w.IdClasificacion;
go

/* ============================================================================
   6. STAGING (recibe los CSV sin transformar; todo como texto)
   ============================================================================ */
create table stg.Clients        (IdCliente varchar(20), Nombre varchar(100), Email varchar(150));
create table stg.Products       (IdProducto varchar(20), Nombre varchar(100), Categoria varchar(50));
create table stg.FuenteDatos    (IdFuente varchar(10), TipoFuente varchar(50), FechaCarga varchar(20));
create table stg.Surveys        (IdOpinion varchar(20), IdCliente varchar(20), IdProducto varchar(20),
                                 Fecha varchar(20), Comentario varchar(500), Clasificacion varchar(20),
                                 PuntajeSatisfaccion varchar(5), Fuente varchar(50));
create table stg.SocialComments (IdComment varchar(10), IdCliente varchar(10), IdProducto varchar(10),
                                 Fuente varchar(50), Fecha varchar(20), Comentario varchar(500));
create table stg.WebReviews     (IdReview varchar(10), IdCliente varchar(10), IdProducto varchar(10),
                                 Fecha varchar(20), Comentario varchar(500), Rating varchar(5));
go

/* ============================================================================
   7. DATOS SEMILLA DE CATÁLOGOS FIJOS
   ============================================================================ */
insert into dbo.Clasificacion (Nombre) values ('Positiva'), ('Neutra'), ('Negativa');
insert into dbo.TipoFuente    (Nombre) values ('Web'), ('CSV'), ('Red Social'), ('Encuesta Interna');
insert into dbo.RedSocial     (Nombre) values ('Facebook'), ('Instagram'), ('Twitter');
insert into dbo.Categoria     (Nombre) values ('Sin categoría');
go

print 'Base de datos OpinionesClientesDB creada correctamente.';
go
