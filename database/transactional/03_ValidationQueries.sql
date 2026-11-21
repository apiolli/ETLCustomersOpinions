use OpinionesClientesDB;
go

/* ----------------------------------------------------------------------------
   1. Conteo de filas por tabla contra lo esperado.
      Cliente y Producto se separan en reales (CSV) y provisionales (EsInferido = 1),
      porque las encuestas citan 465 clientes y 302 productos que no están en los catálogos.
   ---------------------------------------------------------------------------- */
select Tabla, Filas, Esperado,
       case when Filas = Esperado then 'OK' else 'REVISAR' end as Estado
from (
    select 'Cliente (clients.csv)'       as Tabla, (select count(*) from dbo.Cliente  where EsInferido = 0) as Filas, 500 as Esperado union all
    select 'Cliente (provisionales)',             (select count(*) from dbo.Cliente  where EsInferido = 1), 465 union all
    select 'Producto (products.csv)',             (select count(*) from dbo.Producto where EsInferido = 0), 200 union all
    select 'Producto (provisionales)',            (select count(*) from dbo.Producto where EsInferido = 1), 302 union all
    select 'FuenteDatos (fuente_datos.csv)',      (select count(*) from dbo.FuenteDatos),      100 union all
    select 'Encuesta (surveys_part1.csv)',        (select count(*) from dbo.Encuesta),         500 union all
    select 'ComentarioSocial (social_comments)',  (select count(*) from dbo.ComentarioSocial), 200 union all
    select 'ResenaWeb (web_reviews.csv)',         (select count(*) from dbo.ResenaWeb),        200 union all
    select 'Categoria (5 + Sin categoría)',       (select count(*) from dbo.Categoria),          6 union all
    select 'TipoFuente',                          (select count(*) from dbo.TipoFuente),         4 union all
    select 'RedSocial',                           (select count(*) from dbo.RedSocial),          3 union all
    select 'Clasificacion',                       (select count(*) from dbo.Clasificacion),      3
) t;
go

/* ----------------------------------------------------------------------------
   2. Huérfanos de clave foránea (todas deben dar 0)
   ---------------------------------------------------------------------------- */
select 'Encuesta sin cliente'  as Revision, count(*) as Huerfanos
from dbo.Encuesta e left join dbo.Cliente c on c.IdCliente = e.IdCliente where c.IdCliente is null
union all
select 'Encuesta sin producto', count(*)
from dbo.Encuesta e left join dbo.Producto p on p.IdProducto = e.IdProducto where p.IdProducto is null
union all
select 'ComentarioSocial con cliente inexistente', count(*)
from dbo.ComentarioSocial s left join dbo.Cliente c on c.IdCliente = s.IdCliente
where s.IdCliente is not null and c.IdCliente is null
union all
select 'ComentarioSocial sin producto', count(*)
from dbo.ComentarioSocial s left join dbo.Producto p on p.IdProducto = s.IdProducto where p.IdProducto is null
union all
select 'ResenaWeb sin cliente', count(*)
from dbo.ResenaWeb w left join dbo.Cliente c on c.IdCliente = w.IdCliente where c.IdCliente is null
union all
select 'ResenaWeb sin producto', count(*)
from dbo.ResenaWeb w left join dbo.Producto p on p.IdProducto = w.IdProducto where p.IdProducto is null
union all
select 'Producto sin categoría', count(*)
from dbo.Producto p left join dbo.Categoria c on c.IdCategoria = p.IdCategoria where c.IdCategoria is null;
go

/* ----------------------------------------------------------------------------
   3. Registros provisionales creados por las encuestas
   ---------------------------------------------------------------------------- */
select top 20 IdCliente, Nombre, Email, EsInferido
from dbo.Cliente where EsInferido = 1 order by IdCliente;

select top 20 p.IdProducto, p.Nombre, c.Nombre as Categoria, p.EsInferido
from dbo.Producto p join dbo.Categoria c on c.IdCategoria = p.IdCategoria
where p.EsInferido = 1 order by p.IdProducto;

-- Ningún provisional debe tener email, y todos deben estar en 'Sin categoría' (ambas deben dar 0)
select 'Clientes provisionales con email' as Revision, count(*) as Cantidad
from dbo.Cliente where EsInferido = 1 and Email is not null
union all
select 'Productos provisionales con categoría real', count(*)
from dbo.Producto p join dbo.Categoria c on c.IdCategoria = p.IdCategoria
where p.EsInferido = 1 and c.Nombre <> 'Sin categoría';
go

/* ----------------------------------------------------------------------------
   4. Duplicados (deben dar 0 filas)
   ---------------------------------------------------------------------------- */
-- Email repetido
select Email, count(*) as Veces
from dbo.Cliente where Email is not null
group by Email having count(*) > 1;

-- Opiniones repetidas por contenido (mismo cliente, producto, fecha y comentario)
select 'Encuesta' as Origen, IdCliente, IdProducto, Fecha, Comentario, count(*) as Veces
from dbo.Encuesta group by IdCliente, IdProducto, Fecha, Comentario having count(*) > 1
union all
select 'Red Social', IdCliente, IdProducto, Fecha, Comentario, count(*)
from dbo.ComentarioSocial group by IdCliente, IdProducto, Fecha, Comentario having count(*) > 1
union all
select 'Reseña Web', IdCliente, IdProducto, Fecha, Comentario, count(*)
from dbo.ResenaWeb group by IdCliente, IdProducto, Fecha, Comentario having count(*) > 1;
go

/* ----------------------------------------------------------------------------
   5. Rangos y coherencia (todas deben dar 0)
   ---------------------------------------------------------------------------- */
select 'Puntaje de encuesta fuera de 1-5' as Revision, count(*) as Cantidad
from dbo.Encuesta where PuntajeSatisfaccion not between 1 and 5
union all
select 'Rating web fuera de 1-5', count(*)
from dbo.ResenaWeb where Rating not between 1 and 5
union all
select 'Clasificación incoherente con el puntaje', count(*)
from dbo.Encuesta e join dbo.Clasificacion c on c.IdClasificacion = e.IdClasificacion
where not (   (c.Nombre = 'Negativa' and e.PuntajeSatisfaccion between 1 and 2)
           or (c.Nombre = 'Neutra'   and e.PuntajeSatisfaccion = 3)
           or (c.Nombre = 'Positiva' and e.PuntajeSatisfaccion between 4 and 5))
union all
select 'Opiniones con fecha futura', count(*)
from dbo.vw_OpinionesUnificadas where Fecha > cast(getdate() as date);
go

/* ----------------------------------------------------------------------------
   6. Comentarios sociales anónimos (esperado: 88)
   ---------------------------------------------------------------------------- */
select count(*) as ComentariosAnonimos, 88 as Esperado
from dbo.ComentarioSocial where IdCliente is null;

select r.Nombre as RedSocial,
       count(*) as Total,
       sum(case when s.IdCliente is null then 1 else 0 end) as Anonimos
from dbo.ComentarioSocial s join dbo.RedSocial r on r.IdRedSocial = s.IdRedSocial
group by r.Nombre;
go

/* ----------------------------------------------------------------------------
   7. Vistas de análisis
   ---------------------------------------------------------------------------- */
select top 20 * from dbo.vw_ResumenPorProducto
where EsInferido = 0
order by TotalOpiniones desc, IdProducto;

select * from dbo.vw_OpinionesPorClasificacion;

select Origen, count(*) as Opiniones
from dbo.vw_OpinionesUnificadas
group by Origen;
go

/* ----------------------------------------------------------------------------
   8. Evidencia: muestra de cada tabla poblada
      (Venta y DetalleVenta quedan vacías: no hay CSV de origen)
   ---------------------------------------------------------------------------- */
select top 20 * from dbo.Categoria;
select top 20 * from dbo.Producto order by IdProducto;
select top 20 * from dbo.Cliente order by IdCliente;
select top 20 * from dbo.TipoFuente;
select top 20 * from dbo.FuenteDatos order by IdFuente;
select top 20 * from dbo.RedSocial;
select top 20 * from dbo.Clasificacion;
select top 20 * from dbo.Encuesta order by IdOpinion;
select top 20 * from dbo.ComentarioSocial order by IdComment;
select top 20 * from dbo.ResenaWeb order by IdReview;
select top 20 * from dbo.Venta;
select top 20 * from dbo.DetalleVenta;
go
