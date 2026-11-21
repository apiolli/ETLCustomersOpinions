/* ============================================================================
   Sistema de Análisis de Opiniones de Clientes
   Script 03 - Programabilidad de carga de OpinionesClientesDW
   ----------------------------------------------------------------------------
   Objetos que usará la fase de carga (ADO.NET llamando procedimientos):
     1. Tipos de datos definidos por el usuario (UDDT)
     2. Función reutilizable para la clave de fecha
     3. Procedimientos de carga de dimensiones (registros nuevos o modificados)
     4. Miembros por defecto (clasificación, canales, evento "Ninguno", cliente anónimo)
     5. Carga de FactOpinion con table-valued parameter (TVP)
     6. Carga de FactOpinionPalabra vía bulk insert (SqlBulkCopy) a staging + procedimiento
   Requiere: 01_CreateDataWarehouse.sql y 02_CreateStaging.sql.
   Script idempotente: puede ejecutarse varias veces.
   ============================================================================ */

use OpinionesClientesDW;
go

/* ============================================================================
   1. TIPOS DE DATOS DEFINIDOS POR EL USUARIO (UDDT)
      Un solo lugar para la definición de campos que se repiten (DRY).
   ============================================================================ */
if type_id(N'dbo.udtIdOrigen') is null
    create type dbo.udtIdOrigen from varchar(20) not null;
go

if type_id(N'dbo.udtComentario') is null
    create type dbo.udtComentario from varchar(500) not null;
go

if type_id(N'dbo.udtNombre') is null
    create type dbo.udtNombre from varchar(100) not null;
go

/* ============================================================================
   2. FUNCIÓN: CLAVE DE FECHA DEL DATA WAREHOUSE (yyyyMMdd como entero)
      Se usa al generar DimTiempo. En los joins de carga se compara por
      DimTiempo.Fecha (índice único) para no evaluar la función fila por fila.
   ============================================================================ */
create or alter function dbo.fn_GetDateKey (@Fecha date)
returns int
with schemabinding
as
begin
    return year(@Fecha) * 10000 + month(@Fecha) * 100 + day(@Fecha);
end
go

/* ============================================================================
   3. DIMENSIONES
   ============================================================================ */

-- 3.1 DimTiempo: genera todas las fechas del rango que aún no existen
create or alter procedure dbo.usp_LoadDimTiempo
    @FechaInicio date,
    @FechaFin    date,
    @Insertados  int output
as
begin
    set nocount on;

    if @FechaInicio is null or @FechaFin is null or @FechaFin < @FechaInicio
        throw 50201, 'Rango de fechas inválido para DimTiempo.', 1;

    with Numeros as (
        select top (datediff(day, @FechaInicio, @FechaFin) + 1)
               row_number() over (order by (select null)) - 1 as n
        from sys.all_objects a
        cross join sys.all_objects b
    ),
    Fechas as (
        select dateadd(day, n, @FechaInicio) as Fecha
        from Numeros
    )
    insert into dbo.DimTiempo (TiempoKey, Fecha, Anio, Semestre, Trimestre, Mes, NombreMes, AnioMes,
                               SemanaAnio, Dia, DiaSemana, NombreDia, EsFinDeSemana)
    select  dbo.fn_GetDateKey(f.Fecha),
            f.Fecha,
            year(f.Fecha),
            case when month(f.Fecha) <= 6 then 1 else 2 end,
            datepart(quarter, f.Fecha),
            month(f.Fecha),
            choose(month(f.Fecha), 'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio',
                   'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'),
            convert(char(7), f.Fecha, 126),                       -- yyyy-MM
            datepart(iso_week, f.Fecha),
            day(f.Fecha),
            d.DiaSemana,                                          -- 1 = lunes ... 7 = domingo
            choose(d.DiaSemana, 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado', 'Domingo'),
            case when d.DiaSemana in (6, 7) then 1 else 0 end
    from Fechas f
    -- día de la semana independiente de SET DATEFIRST
    cross apply (select ((datepart(weekday, f.Fecha) + @@datefirst - 2) % 7) + 1 as DiaSemana) d
    where not exists (select 1 from dbo.DimTiempo t where t.Fecha = f.Fecha);

    set @Insertados = @@rowcount;
end
go

-- 3.2 DimCliente: inserta si no existe; actualiza solo si cambió (SCD tipo 1)
create or alter procedure dbo.usp_LoadDimCliente
    @IdClienteOrigen int,
    @Nombre          varchar(100),
    @Email           varchar(150) = null,
    @EsInferido      bit          = 0,
    @Pais            varchar(60)  = 'Desconocido',
    @RangoEdad       varchar(20)  = 'Desconocido',
    @TipoCliente     varchar(30)  = 'Desconocido',
    @Accion          varchar(12)  output            -- Insertado / Actualizado / SinCambios
as
begin
    set nocount on;

    if @IdClienteOrigen is null
        throw 50211, 'IdClienteOrigen es obligatorio; el cliente anónimo lo crea usp_SeedDimensionMembers.', 1;

    if exists (select 1 from dbo.DimCliente where IdClienteOrigen = @IdClienteOrigen)
    begin
        update dbo.DimCliente
           set Nombre      = @Nombre,
               Email       = @Email,
               EsInferido  = @EsInferido,
               Pais        = @Pais,
               RangoEdad   = @RangoEdad,
               TipoCliente = @TipoCliente
         where IdClienteOrigen = @IdClienteOrigen
           and (   Nombre      <> @Nombre
                or isnull(Email, '') <> isnull(@Email, '')
                or EsInferido  <> @EsInferido
                or Pais        <> @Pais
                or RangoEdad   <> @RangoEdad
                or TipoCliente <> @TipoCliente);

        set @Accion = case when @@rowcount > 0 then 'Actualizado' else 'SinCambios' end;
        return;
    end

    insert into dbo.DimCliente (IdClienteOrigen, Nombre, Email, Pais, RangoEdad, TipoCliente, EsInferido)
    values (@IdClienteOrigen, @Nombre, @Email, @Pais, @RangoEdad, @TipoCliente, @EsInferido);

    set @Accion = 'Insertado';
end
go

-- 3.3 DimProducto: inserta si no existe; actualiza solo si cambió (SCD tipo 1)
create or alter procedure dbo.usp_LoadDimProducto
    @IdProductoOrigen int,
    @Nombre           varchar(100),
    @Categoria        varchar(50),
    @EsInferido       bit = 0,
    @Accion           varchar(12) output
as
begin
    set nocount on;

    if exists (select 1 from dbo.DimProducto where IdProductoOrigen = @IdProductoOrigen)
    begin
        update dbo.DimProducto
           set Nombre     = @Nombre,
               Categoria  = @Categoria,
               EsInferido = @EsInferido
         where IdProductoOrigen = @IdProductoOrigen
           and (Nombre <> @Nombre or Categoria <> @Categoria or EsInferido <> @EsInferido);

        set @Accion = case when @@rowcount > 0 then 'Actualizado' else 'SinCambios' end;
        return;
    end

    insert into dbo.DimProducto (IdProductoOrigen, Nombre, Categoria, EsInferido)
    values (@IdProductoOrigen, @Nombre, @Categoria, @EsInferido);

    set @Accion = 'Insertado';
end
go

-- 3.4 DimCanal: canal + plataforma (llave natural única)
create or alter procedure dbo.usp_LoadDimCanal
    @Canal      varchar(30),
    @Plataforma varchar(30),
    @TipoOrigen varchar(30)
as
begin
    set nocount on;

    if not exists (select 1 from dbo.DimCanal where Canal = @Canal and Plataforma = @Plataforma)
        insert into dbo.DimCanal (Canal, Plataforma, TipoOrigen)
        values (@Canal, @Plataforma, @TipoOrigen);
end
go

-- 3.5 DimEvento: eventos de negocio (lanzamientos, campañas, temporadas); llave natural = Nombre
create or alter procedure dbo.usp_LoadDimEvento
    @Nombre      varchar(100),
    @TipoEvento  varchar(30),
    @FechaInicio date = null,
    @FechaFin    date = null
as
begin
    set nocount on;

    if exists (select 1 from dbo.DimEvento where Nombre = @Nombre)
        update dbo.DimEvento
           set TipoEvento  = @TipoEvento,
               FechaInicio = @FechaInicio,
               FechaFin    = @FechaFin
         where Nombre = @Nombre;
    else
        insert into dbo.DimEvento (Nombre, TipoEvento, FechaInicio, FechaFin)
        values (@Nombre, @TipoEvento, @FechaInicio, @FechaFin);
end
go

-- 3.6 DimPalabraClave: palabra individual
create or alter procedure dbo.usp_LoadDimPalabraClave
    @Palabra    varchar(50),
    @PalabraKey int output
as
begin
    set nocount on;

    set @PalabraKey = null;
    select @PalabraKey = PalabraKey from dbo.DimPalabraClave where Palabra = @Palabra;

    if @PalabraKey is null
    begin
        insert into dbo.DimPalabraClave (Palabra) values (@Palabra);
        set @PalabraKey = scope_identity();
    end
end
go

/* ============================================================================
   4. MIEMBROS POR DEFECTO
      FactOpinion exige ClienteKey, ClasificacionKey y EventoKey (not null).
   ============================================================================ */
create or alter procedure dbo.usp_SeedDimensionMembers
as
begin
    set nocount on;

    -- Clasificación de sentimiento (clave fija, no identity)
    if not exists (select 1 from dbo.DimClasificacion where ClasificacionKey = 1)
        insert into dbo.DimClasificacion (ClasificacionKey, Nombre, ValorSentimiento) values (1, 'Positiva', 1);
    if not exists (select 1 from dbo.DimClasificacion where ClasificacionKey = 2)
        insert into dbo.DimClasificacion (ClasificacionKey, Nombre, ValorSentimiento) values (2, 'Neutra', 0);
    if not exists (select 1 from dbo.DimClasificacion where ClasificacionKey = 3)
        insert into dbo.DimClasificacion (ClasificacionKey, Nombre, ValorSentimiento) values (3, 'Negativa', -1);
    if not exists (select 1 from dbo.DimClasificacion where ClasificacionKey = 4)
        insert into dbo.DimClasificacion (ClasificacionKey, Nombre, ValorSentimiento) values (4, 'Sin clasificar', null);

    -- Canales y su tecnología de origen
    exec dbo.usp_LoadDimCanal @Canal = 'Encuesta Interna', @Plataforma = 'EncuestaInterna', @TipoOrigen = 'CSV';
    exec dbo.usp_LoadDimCanal @Canal = 'Web',              @Plataforma = 'Sitio Web',       @TipoOrigen = 'Base de datos';
    exec dbo.usp_LoadDimCanal @Canal = 'Red Social',       @Plataforma = 'Facebook',        @TipoOrigen = 'API';
    exec dbo.usp_LoadDimCanal @Canal = 'Red Social',       @Plataforma = 'Instagram',       @TipoOrigen = 'API';
    exec dbo.usp_LoadDimCanal @Canal = 'Red Social',       @Plataforma = 'Twitter',         @TipoOrigen = 'API';

    -- Evento por defecto para opiniones fuera de cualquier evento
    if not exists (select 1 from dbo.DimEvento where TipoEvento = 'Ninguno')
        insert into dbo.DimEvento (Nombre, TipoEvento, FechaInicio, FechaFin)
        values ('Ninguno', 'Ninguno', null, null);

    -- Cliente anónimo para comentarios sociales sin cliente
    if not exists (select 1 from dbo.DimCliente where IdClienteOrigen is null and Nombre = 'Anónimo')
        insert into dbo.DimCliente (IdClienteOrigen, Nombre, Email, EsInferido)
        values (null, 'Anónimo', null, 0);
end
go

/* ============================================================================
   5. FACTOPINION CON TABLE-VALUED PARAMETER (TVP)
      El ETL envía todas las opiniones transformadas en un solo viaje.
      Las llaves de las dimensiones se resuelven aquí con joins y los
      duplicados se descartan con NOT EXISTS (sin if en C#).
   ============================================================================ */
if type_id(N'dbo.FactOpinionTableType') is null
    create type dbo.FactOpinionTableType as table (
        IdOpinionOrigen  dbo.udtIdOrigen,
        Fecha            date          not null,
        IdProductoOrigen int           not null,
        IdClienteOrigen  int           null,        -- null = cliente anónimo
        Canal            varchar(30)   not null,    -- Encuesta Interna / Web / Red Social
        Plataforma       varchar(30)   not null,    -- EncuestaInterna / Sitio Web / Facebook / Instagram / Twitter
        Clasificacion    varchar(20)   not null,    -- Positiva / Neutra / Negativa / Sin clasificar
        Comentario       dbo.udtComentario,
        Puntaje          tinyint       null,        -- 1-5; null en redes sociales
        primary key (Canal, Plataforma, IdOpinionOrigen)
    );
go

create or alter procedure dbo.usp_LoadFactOpinion
    @Opiniones  dbo.FactOpinionTableType readonly,
    @Insertados int output,
    @Omitidos   int output        -- duplicadas o con dimensiones sin resolver
as
begin
    set nocount on;

    declare @ClienteAnonimoKey int =
        (select top (1) ClienteKey from dbo.DimCliente
          where IdClienteOrigen is null and Nombre = 'Anónimo' order by ClienteKey);

    declare @EventoNingunoKey int =
        (select top (1) EventoKey from dbo.DimEvento
          where TipoEvento = 'Ninguno' order by EventoKey);

    declare @SinClasificarKey int =
        (select ClasificacionKey from dbo.DimClasificacion where Nombre = 'Sin clasificar');

    if @ClienteAnonimoKey is null or @EventoNingunoKey is null or @SinClasificarKey is null
        throw 50251, 'Faltan los miembros por defecto. Ejecute dbo.usp_SeedDimensionMembers.', 1;

    insert into dbo.FactOpinion (TiempoKey, ProductoKey, ClienteKey, CanalKey, ClasificacionKey,
                                 EventoKey, IdOpinionOrigen, Comentario, Cantidad, Puntaje)
    select  t.TiempoKey,
            p.ProductoKey,
            coalesce(c.ClienteKey, @ClienteAnonimoKey),
            ca.CanalKey,
            coalesce(cl.ClasificacionKey, @SinClasificarKey),
            coalesce(ev.EventoKey, @EventoNingunoKey),
            o.IdOpinionOrigen,
            o.Comentario,
            1,
            o.Puntaje
    from @Opiniones o
    join dbo.DimTiempo   t  on t.Fecha = o.Fecha
    join dbo.DimProducto p  on p.IdProductoOrigen = o.IdProductoOrigen
    join dbo.DimCanal    ca on ca.Canal = o.Canal and ca.Plataforma = o.Plataforma
    left join dbo.DimCliente       c  on c.IdClienteOrigen = o.IdClienteOrigen
    left join dbo.DimClasificacion cl on cl.Nombre = o.Clasificacion
    outer apply (
        select top (1) e.EventoKey
        from dbo.DimEvento e
        where e.TipoEvento <> 'Ninguno'
          and o.Fecha between e.FechaInicio and e.FechaFin
        order by e.FechaInicio desc
    ) ev
    where (o.IdClienteOrigen is null or c.ClienteKey is not null)       -- cliente conocido o anónimo
      and not exists (select 1 from dbo.FactOpinion f
                       where f.CanalKey = ca.CanalKey
                         and f.IdOpinionOrigen = o.IdOpinionOrigen);

    set @Insertados = @@rowcount;
    set @Omitidos   = (select count(*) from @Opiniones) - @Insertados;
end
go

/* ============================================================================
   6. FACTOPINIONPALABRA VÍA BULK INSERT
      El ETL escribe las palabras clave con SqlBulkCopy en Staging.OpinionKeywords
      y luego llama a usp_LoadFactOpinionPalabra, que resuelve las llaves en bloque.
   ============================================================================ */
if schema_id(N'Staging') is null
    exec (N'create schema Staging');
go

if object_id(N'Staging.OpinionKeywords', N'U') is null
create table Staging.OpinionKeywords (
    Canal           varchar(30) not null,
    Plataforma      varchar(30) not null,
    IdOpinionOrigen varchar(20) not null,
    Palabra         varchar(50) not null,
    Frecuencia      tinyint     not null
);
go

create or alter procedure dbo.usp_LoadFactOpinionPalabra
    @PalabrasNuevas int output,
    @Insertados     int output
as
begin
    set nocount on;

    -- 1. Palabras que aún no existen en la dimensión
    insert into dbo.DimPalabraClave (Palabra)
    select distinct k.Palabra
    from Staging.OpinionKeywords k
    where not exists (select 1 from dbo.DimPalabraClave d where d.Palabra = k.Palabra);

    set @PalabrasNuevas = @@rowcount;

    -- 2. Relación opinión-palabra (solo para opiniones ya cargadas en FactOpinion)
    insert into dbo.FactOpinionPalabra (OpinionKey, PalabraKey, Frecuencia)
    select  f.OpinionKey,
            d.PalabraKey,
            cast(case when sum(k.Frecuencia) > 255 then 255 else sum(k.Frecuencia) end as tinyint)
    from Staging.OpinionKeywords k
    join dbo.DimCanal        ca on ca.Canal = k.Canal and ca.Plataforma = k.Plataforma
    join dbo.FactOpinion     f  on f.CanalKey = ca.CanalKey and f.IdOpinionOrigen = k.IdOpinionOrigen
    join dbo.DimPalabraClave d  on d.Palabra = k.Palabra
    where not exists (select 1 from dbo.FactOpinionPalabra fp
                       where fp.OpinionKey = f.OpinionKey
                         and fp.PalabraKey = d.PalabraKey)
    group by f.OpinionKey, d.PalabraKey;

    set @Insertados = @@rowcount;

    -- 3. El staging de palabras se vacía después de cada carga
    truncate table Staging.OpinionKeywords;
end
go

/* ============================================================================
   7. EJEMPLOS DE USO (para pruebas manuales en SSMS)
   ============================================================================ */
-- exec dbo.usp_SeedDimensionMembers;
--
-- declare @n int;
-- exec dbo.usp_LoadDimTiempo @FechaInicio = '2024-01-01', @FechaFin = '2026-12-31', @Insertados = @n output;
-- select @n as DiasInsertados;
--
-- declare @accion varchar(12);
-- exec dbo.usp_LoadDimProducto @IdProductoOrigen = 1, @Nombre = 'Producto_1', @Categoria = 'Juguetes',
--      @EsInferido = 0, @Accion = @accion output;
-- exec dbo.usp_LoadDimCliente @IdClienteOrigen = 1, @Nombre = 'Cliente_1', @Email = 'cliente1@mail.com',
--      @EsInferido = 0, @Accion = @accion output;
-- select @accion as Accion;
--
-- declare @o dbo.FactOpinionTableType, @ins int, @omi int;
-- insert into @o values ('1', '2025-07-15', 1, 1, 'Encuesta Interna', 'EncuestaInterna', 'Neutra',
--                        'El producto está bien, pero esperaba un poco más de calidad.', 3);
-- exec dbo.usp_LoadFactOpinion @Opiniones = @o, @Insertados = @ins output, @Omitidos = @omi output;
-- select @ins as Insertados, @omi as Omitidos;
go

print 'Programabilidad de carga de OpinionesClientesDW creada correctamente.';
go
