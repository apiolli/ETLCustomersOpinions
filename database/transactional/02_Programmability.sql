use OpinionesClientesDB;
go

/* 1. CATÁLOGOS */
create or alter procedure dbo.usp_ObtenerOCrearCategoria
    @Nombre varchar(50),
    @Id     int output
as
begin
    set nocount on;
    set @Id = null;
    select @Id = IdCategoria from dbo.Categoria where Nombre = @Nombre;
    if @Id is null
    begin
        insert into dbo.Categoria (Nombre) values (@Nombre);
        set @Id = scope_identity();
    end
end
go

create or alter procedure dbo.usp_ObtenerOCrearTipoFuente
    @Nombre varchar(50),
    @Id     int output
as
begin
    set nocount on;
    set @Id = null;
    select @Id = IdTipoFuente from dbo.TipoFuente where Nombre = @Nombre;
    if @Id is null
    begin
        insert into dbo.TipoFuente (Nombre) values (@Nombre);
        set @Id = scope_identity();
    end
end
go

create or alter procedure dbo.usp_ObtenerOCrearRedSocial
    @Nombre varchar(50),
    @Id     int output
as
begin
    set nocount on;
    set @Id = null;
    select @Id = IdRedSocial from dbo.RedSocial where Nombre = @Nombre;
    if @Id is null
    begin
        insert into dbo.RedSocial (Nombre) values (@Nombre);
        set @Id = scope_identity();
    end
end
go

create or alter procedure dbo.usp_ObtenerOCrearClasificacion
    @Nombre varchar(20),
    @Id     int output
as
begin
    set nocount on;
    set @Id = null;
    select @Id = IdClasificacion from dbo.Clasificacion where Nombre = @Nombre;
    if @Id is null
    begin
        insert into dbo.Clasificacion (Nombre) values (@Nombre);
        set @Id = scope_identity();
    end
end
go

/* ============================================================================
   2. CLIENTES, PRODUCTOS Y FUENTES DE DATOS
   ============================================================================ */
create or alter procedure dbo.usp_InsertarCliente
    @IdCliente  int,
    @Nombre     varchar(100),
    @Email      varchar(150) = null,
    @EsInferido bit = 0,
    @Insertado  bit output
as
begin
    set nocount on;
    if exists (select 1 from dbo.Cliente where IdCliente = @IdCliente)
    begin
        set @Insertado = 0;
        return;
    end

    insert into dbo.Cliente (IdCliente, Nombre, Email, EsInferido)
    values (@IdCliente, @Nombre, @Email, @EsInferido);
    set @Insertado = 1;
end
go

create or alter procedure dbo.usp_InsertarProducto
    @IdProducto  int,
    @Nombre      varchar(100),
    @IdCategoria int,
    @EsInferido  bit = 0,
    @Insertado   bit output
as
begin
    set nocount on;
    if exists (select 1 from dbo.Producto where IdProducto = @IdProducto)
    begin
        set @Insertado = 0;
        return;
    end

    insert into dbo.Producto (IdProducto, Nombre, IdCategoria, EsInferido)
    values (@IdProducto, @Nombre, @IdCategoria, @EsInferido);
    set @Insertado = 1;
end
go

create or alter procedure dbo.usp_InsertarFuenteDatos
    @IdFuente     varchar(10),
    @IdTipoFuente int,
    @FechaCarga   date,
    @Insertado    bit output
as
begin
    set nocount on;
    if exists (select 1 from dbo.FuenteDatos where IdFuente = @IdFuente)
    begin
        set @Insertado = 0;
        return;
    end

    insert into dbo.FuenteDatos (IdFuente, IdTipoFuente, FechaCarga)
    values (@IdFuente, @IdTipoFuente, @FechaCarga);
    set @Insertado = 1;
end
go

-- Estado actual para validar duplicados e integridad referencial desde la aplicación
create or alter procedure dbo.usp_ObtenerClientesExistentes
as
begin
    set nocount on;
    select IdCliente, Email from dbo.Cliente;
end
go

create or alter procedure dbo.usp_ObtenerProductosExistentes
as
begin
    set nocount on;
    select IdProducto from dbo.Producto;
end
go

-- Claves primarias existentes (como texto) para detectar duplicados en memoria
create or alter procedure dbo.usp_ObtenerIdsExistentes
    @Tabla varchar(30)
as
begin
    set nocount on;
    if      @Tabla = 'Cliente'          select cast(IdCliente  as varchar(10)) as Id from dbo.Cliente;
    else if @Tabla = 'Producto'         select cast(IdProducto as varchar(10)) as Id from dbo.Producto;
    else if @Tabla = 'FuenteDatos'      select IdFuente                        as Id from dbo.FuenteDatos;
    else if @Tabla = 'Encuesta'         select cast(IdOpinion  as varchar(10)) as Id from dbo.Encuesta;
    else if @Tabla = 'ComentarioSocial' select IdComment                       as Id from dbo.ComentarioSocial;
    else if @Tabla = 'ResenaWeb'        select IdReview                        as Id from dbo.ResenaWeb;
    else throw 50001, 'Tabla no soportada en usp_ObtenerIdsExistentes.', 1;
end
go

-- Claves de contenido de las opiniones: IdCliente|IdProducto|yyyy-MM-dd|Comentario
-- (mismo formato que ClaveOpinion en C#; IdCliente null = vacío)
create or alter procedure dbo.usp_ObtenerClavesOpiniones
    @Tabla varchar(30)
as
begin
    set nocount on;
    if @Tabla = 'Encuesta'
        select concat(IdCliente, '|', IdProducto, '|', convert(char(10), Fecha, 23), '|', Comentario) as Clave
        from dbo.Encuesta;
    else if @Tabla = 'ComentarioSocial'
        select concat(IdCliente, '|', IdProducto, '|', convert(char(10), Fecha, 23), '|', Comentario) as Clave
        from dbo.ComentarioSocial;
    else if @Tabla = 'ResenaWeb'
        select concat(IdCliente, '|', IdProducto, '|', convert(char(10), Fecha, 23), '|', Comentario) as Clave
        from dbo.ResenaWeb;
    else throw 50002, 'Tabla no soportada en usp_ObtenerClavesOpiniones.', 1;
end
go

/* ============================================================================
   3. OPINIONES
   ============================================================================ */
create or alter procedure dbo.usp_InsertarEncuesta
    @IdOpinion           int,
    @IdCliente           int,
    @IdProducto          int,
    @IdFuente            varchar(10) = null,
    @Fecha               date,
    @Comentario          varchar(500),
    @IdClasificacion     int,
    @PuntajeSatisfaccion tinyint,
    @Insertado           bit output
as
begin
    set nocount on;
    if exists (select 1 from dbo.Encuesta where IdOpinion = @IdOpinion)
    begin
        set @Insertado = 0;
        return;
    end

    insert into dbo.Encuesta (IdOpinion, IdCliente, IdProducto, IdFuente, Fecha, Comentario,
                              IdClasificacion, PuntajeSatisfaccion)
    values (@IdOpinion, @IdCliente, @IdProducto, @IdFuente, @Fecha, @Comentario,
            @IdClasificacion, @PuntajeSatisfaccion);
    set @Insertado = 1;
end
go

create or alter procedure dbo.usp_InsertarComentarioSocial
    @IdComment       varchar(10),
    @IdCliente       int = null,
    @IdProducto      int,
    @IdRedSocial     int,
    @IdFuente        varchar(10) = null,
    @Fecha           date,
    @Comentario      varchar(500),
    @IdClasificacion int = null,
    @Insertado       bit output
as
begin
    set nocount on;
    if exists (select 1 from dbo.ComentarioSocial where IdComment = @IdComment)
    begin
        set @Insertado = 0;
        return;
    end

    insert into dbo.ComentarioSocial (IdComment, IdCliente, IdProducto, IdRedSocial, IdFuente, Fecha,
                                      Comentario, IdClasificacion)
    values (@IdComment, @IdCliente, @IdProducto, @IdRedSocial, @IdFuente, @Fecha,
            @Comentario, @IdClasificacion);
    set @Insertado = 1;
end
go

create or alter procedure dbo.usp_InsertarResenaWeb
    @IdReview        varchar(10),
    @IdCliente       int,
    @IdProducto      int,
    @IdFuente        varchar(10) = null,
    @Fecha           date,
    @Comentario      varchar(500),
    @Rating          tinyint,
    @IdClasificacion int = null,
    @Insertado       bit output
as
begin
    set nocount on;
    if exists (select 1 from dbo.ResenaWeb where IdReview = @IdReview)
    begin
        set @Insertado = 0;
        return;
    end

    insert into dbo.ResenaWeb (IdReview, IdCliente, IdProducto, IdFuente, Fecha, Comentario,
                               Rating, IdClasificacion)
    values (@IdReview, @IdCliente, @IdProducto, @IdFuente, @Fecha, @Comentario,
            @Rating, @IdClasificacion);
    set @Insertado = 1;
end
go

/* ============================================================================
   4. VISTAS DE ANÁLISIS (vw_OpinionesUnificadas ya existe en el script 01)
   ============================================================================ */

-- Cantidad de opiniones por origen y puntaje promedio (encuestas + rating web) por producto
create or alter view dbo.vw_ResumenPorProducto
as
select  p.IdProducto,
        p.Nombre                                                as Producto,
        c.Nombre                                                as Categoria,
        p.EsInferido,
        count(o.IdRegistro)                                     as TotalOpiniones,
        sum(case when o.Origen = 'Encuesta'   then 1 else 0 end) as Encuestas,
        sum(case when o.Origen = 'Red Social' then 1 else 0 end) as ComentariosSociales,
        sum(case when o.Origen = 'Reseña Web' then 1 else 0 end) as ResenasWeb,
        cast(avg(cast(o.Puntaje as decimal(5,2))) as decimal(4,2)) as PuntajePromedio
from dbo.Producto p
join dbo.Categoria c                    on c.IdCategoria = p.IdCategoria
left join dbo.vw_OpinionesUnificadas o  on o.IdProducto  = p.IdProducto
group by p.IdProducto, p.Nombre, c.Nombre, p.EsInferido;
go

-- Encuestas por clasificación de sentimiento
create or alter view dbo.vw_OpinionesPorClasificacion
as
select  cl.Nombre                                                       as Clasificacion,
        count(e.IdOpinion)                                              as TotalEncuestas,
        min(e.PuntajeSatisfaccion)                                      as PuntajeMinimo,
        max(e.PuntajeSatisfaccion)                                      as PuntajeMaximo,
        cast(avg(cast(e.PuntajeSatisfaccion as decimal(5,2))) as decimal(4,2)) as PuntajePromedio
from dbo.Clasificacion cl
left join dbo.Encuesta e on e.IdClasificacion = cl.IdClasificacion
group by cl.Nombre;
go

print 'Procedimientos y vistas de CustomersOpinions creados correctamente.';
go

