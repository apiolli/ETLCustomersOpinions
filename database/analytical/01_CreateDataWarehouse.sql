create database OpinionesClientesDW
    collate Modern_Spanish_CI_AS;
GO

use OpinionesClientesDW;
GO

-- Dimensiones
create table DimTiempo (
    TiempoKey int not null,
    Fecha date not null,
    Anio smallint not null,
    Semestre tinyint not null,
    Trimestre tinyint not null,
    Mes tinyint not null,
    NombreMes varchar(15) not null,
    AnioMes char(7) not null,         
    SemanaAnio tinyint not null,
    Dia tinyint not null,
    DiaSemana tinyint not null,    
    NombreDia varchar(10) not null,
    EsFinDeSemana bit not null,
    constraint PK_DimTiempo primary key (TiempoKey),
    constraint UQ_DimTiempo_Fecha unique (Fecha)
);
GO

-- Productos
create table DimProducto (
    ProductoKey int identity(1,1) not null,
    IdProductoOrigen int not null, 
    Nombre varchar(100) not null,
    Categoria varchar(50) not null,
    EsInferido  bit not null constraint DF_DimProducto_EsInferido default (0),
    constraint PK_DimProducto primary key (ProductoKey),
    constraint UQ_DimProducto_IdOrigen unique (IdProductoOrigen)
);
GO

-- Clientes
create table DimCliente (
    ClienteKey int identity(1,1) not null,
    IdClienteOrigen int null, 
    Nombre varchar(100) not null,
    Email varchar(150) null,
    Pais varchar(60) not null 
    constraint DF_DimCliente_Pais default ('Desconocido'),
    RangoEdad varchar(20) not null 
        constraint DF_DimCliente_RangoEdad default ('Desconocido'),
    TipoCliente varchar(30) not null 
        constraint DF_DimCliente_Tipo default ('Desconocido'),
    EsInferido bit not null 
        constraint DF_DimCliente_EsInferido default (0),
    constraint PK_DimCliente primary key (ClienteKey)
);
GO

create unique index UX_DimCliente_IdOrigen 
    on dbo.DimCliente (IdClienteOrigen) 
    where IdClienteOrigen is not null;
GO

-- Canal por donde llegó la opinion y su tecnologia de origen
create table DimCanal (
    CanalKey int identity(1,1) not null,
    Canal varchar(30) not null,   
    Plataforma varchar(30) not null,      
    TipoOrigen varchar(30) not null,      
    constraint PK_DimCanal primary key (CanalKey),
    constraint UQ_DimCanal_Canal_Plataforma unique (Canal, Plataforma)
);
GO

-- Sentimiento de la opinion
create table DimClasificacion (
    ClasificacionKey int not null,
    Nombre varchar(20) not null, -- Positiva / Neutra / Negativa / Sin clasificar
    ValorSentimiento smallint null,         
    constraint PK_DimClasificacion primary key (ClasificacionKey),
    constraint UQ_DimClasificacion_Nombre  unique (Nombre)
);
GO

-- Eventos de negocio
create table DimEvento (
    EventoKey int identity(1,1) not null,
    Nombre varchar(100) not null,
    TipoEvento varchar(30) not null, -- Lanzamiento / Campaña / Temporada / Ninguno
    FechaInicio date null,
    FechaFin date null,
    constraint PK_DimEvento primary key (EventoKey),
    constraint CK_DimEvento_Fechas 
        check (FechaFin is null or FechaInicio is null or FechaFin >= FechaInicio)
);
GO

-- Tabla de hechos
create table FactOpinion (
    OpinionKey bigint identity(1,1) not null,
    TiempoKey int not null,
    ProductoKey int not null,
    ClienteKey int not null,
    CanalKey int not null,
    ClasificacionKey int not null,
    EventoKey int not null,
    IdOpinionOrigen varchar(20) not null,       
    Comentario varchar(500) not null,
    Cantidad tinyint not null 
    constraint DF_FactOpinion_Cantidad default (1),
    Puntaje tinyint null, -- 1-5 encuesta o rating, null en redes sociales
    EsSatisfecho as (case when Puntaje >= 4 then 1 when Puntaje is null 
        then null else 0 end) persisted,
    FechaCarga datetime2(0) not null 
        constraint DF_FactOpinion_FechaCarga 
            default (sysdatetime()),
        constraint PK_FactOpinion primary key (OpinionKey),
        constraint FK_FactOpinion_Tiempo 
            foreign key (TiempoKey)        
            references DimTiempo (TiempoKey),
        constraint FK_FactOpinion_Producto      
            foreign key (ProductoKey)      
            references DimProducto (ProductoKey),
        constraint FK_FactOpinion_Cliente       
            foreign key (ClienteKey)     
            references DimCliente (ClienteKey),
        constraint FK_FactOpinion_Canal        
            foreign key (CanalKey)        
            references DimCanal (CanalKey),
        constraint FK_FactOpinion_Clasificacion 
            foreign key (ClasificacionKey) 
            references DimClasificacion (ClasificacionKey),
        constraint FK_FactOpinion_Evento        
            foreign key (EventoKey)       
            references DimEvento (EventoKey),
        constraint UQ_FactOpinion_Origen        
            unique (CanalKey, IdOpinionOrigen),  
        constraint CK_FactOpinion_Puntaje       
            check (Puntaje between 1 and 5)
);
GO

create table DimPalabraClave (
    PalabraKey int identity(1,1) not null,
    Palabra varchar(50) not null,
    constraint PK_DimPalabraClave 
        primary key (PalabraKey),
    constraint UQ_DimPalabraClave_Palabra  
        unique (Palabra)
);
GO

create table FactOpinionPalabra (
    OpinionKey bigint not null,
    PalabraKey int not null,
    Frecuencia tinyint not null 
    constraint DF_FactOpinionPalabra_Frecuencia 
        default (1),
    constraint PK_FactOpinionPalabra 
        primary key (OpinionKey, PalabraKey),
    constraint FK_FactOpinionPalabra_Opinion 
        foreign key (OpinionKey) references dbo.FactOpinion (OpinionKey) on delete cascade,
    constraint FK_FactOpinionPalabra_Palabra 
        foreign key (PalabraKey) references dbo.DimPalabraClave (PalabraKey)
);
GO

-- Indices
create index IX_FactOpinion_Tiempo 
    on FactOpinion (TiempoKey)
    include (Puntaje, ClasificacionKey);

create index IX_FactOpinion_Producto      
    on FactOpinion (ProductoKey)     
    include (Puntaje, ClasificacionKey);

create index IX_FactOpinion_Cliente      
    on FactOpinion (ClienteKey);

create index IX_FactOpinion_Canal        
    on FactOpinion (CanalKey)        
    include (ClasificacionKey);

create index IX_FactOpinion_Clasificacion 
    on FactOpinion (ClasificacionKey);

create index IX_FactOpinion_Evento        
    on FactOpinion (EventoKey);

create index IX_FactOpinionPalabra_Palabra
    on FactOpinionPalabra (PalabraKey);
GO
