/* ============================================================================
   Sistema de Análisis de Opiniones de Clientes
   Script 04 - Objetos de extracción sobre OpinionesClientesDB
   ----------------------------------------------------------------------------
   Vistas de las que el ETL extrae la información hacia la base analítica y
   procedimiento paginado que usa la API REST de comentarios sociales.
   Solo se exponen las columnas que necesita el data warehouse.
   Script idempotente: puede ejecutarse varias veces.
   ============================================================================ */

use OpinionesClientesDB;
go

/* ----------------------------------------------------------------------------
   1. Reseñas web -> extractor de base de datos (FactOpinion, canal Web)
   ---------------------------------------------------------------------------- */
create or alter view dbo.vw_ExtractWebReviews
as
select  w.IdReview,
        w.IdCliente,
        w.IdProducto,
        w.Fecha,
        w.Comentario,
        w.Rating
from dbo.ResenaWeb w;
go

/* ----------------------------------------------------------------------------
   2. Clientes -> DimCliente
   ---------------------------------------------------------------------------- */
create or alter view dbo.vw_ExtractCustomers
as
select  c.IdCliente,
        c.Nombre,
        c.Email,
        c.EsInferido
from dbo.Cliente c;
go

/* ----------------------------------------------------------------------------
   3. Productos -> DimProducto (categoría desnormalizada, como en el modelo estrella)
   ---------------------------------------------------------------------------- */
create or alter view dbo.vw_ExtractProducts
as
select  p.IdProducto,
        p.Nombre,
        cat.Nombre as Categoria,
        p.EsInferido
from dbo.Producto p
join dbo.Categoria cat on cat.IdCategoria = p.IdCategoria;
go

/* ----------------------------------------------------------------------------
   4. Comentarios sociales paginados -> API REST (FactOpinion, canal Red Social)
   ---------------------------------------------------------------------------- */
create or alter procedure dbo.usp_GetSocialCommentsPage
    @PageNumber int,
    @PageSize   int,
    @TotalCount int output
as
begin
    set nocount on;

    if @PageNumber is null or @PageNumber < 1
        throw 50101, 'PageNumber debe ser mayor o igual a 1.', 1;

    if @PageSize is null or @PageSize < 1
        throw 50102, 'PageSize debe ser mayor o igual a 1.', 1;

    select @TotalCount = count(*) from dbo.ComentarioSocial;

    select  s.IdComment,
            s.IdCliente,             -- null = comentario anónimo
            s.IdProducto,
            r.Nombre as RedSocial,
            s.Fecha,
            s.Comentario
    from dbo.ComentarioSocial s
    join dbo.RedSocial r on r.IdRedSocial = s.IdRedSocial
    order by s.IdComment
    offset (@PageNumber - 1) * @PageSize rows
    fetch next @PageSize rows only;
end
go

/* ----------------------------------------------------------------------------
   5. (Opcional) Rol de mínimo privilegio para el ETL y la API
      Descomentar si se usa un login SQL en lugar de seguridad integrada.
   ---------------------------------------------------------------------------- */
-- if database_principal_id(N'etl_reader') is null
--     create role etl_reader;
-- grant select  on dbo.vw_ExtractWebReviews      to etl_reader;
-- grant select  on dbo.vw_ExtractCustomers       to etl_reader;
-- grant select  on dbo.vw_ExtractProducts        to etl_reader;
-- grant execute on dbo.usp_GetSocialCommentsPage to etl_reader;
-- -- create login etl_user with password = '<contraseña segura>';
-- -- create user etl_user for login etl_user;
-- -- alter role etl_reader add member etl_user;
-- go

print 'Objetos de extracción de OpinionesClientesDB creados correctamente.';
go
