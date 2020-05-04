USE [toyn]
GO
/****** Object:  Index [idx_objects_contents_3]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_contents]') AND name = N'idx_objects_contents_3')
  DROP INDEX [idx_objects_contents_3] ON [dbo].[objects_contents]
GO
/****** Object:  Index [idx_objects_contents_2]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_contents]') AND name = N'idx_objects_contents_2')
 DROP INDEX [idx_objects_contents_2] ON [dbo].[objects_contents]
GO
/****** Object:  Index [idx_objects_contents]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_contents]') AND name = N'idx_objects_contents')
 DROP INDEX [idx_objects_contents] ON [dbo].[objects_contents]
GO
/****** Object:  Index [idx_objects_attrs_varchar]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_varchar]') AND name = N'idx_objects_attrs_varchar')
 DROP INDEX [idx_objects_attrs_varchar] ON [dbo].[objects_attrs_varchar]
GO
/****** Object:  Index [idx_objects_attrs_text]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_text]') AND name = N'idx_objects_attrs_text')
 DROP INDEX [idx_objects_attrs_text] ON [dbo].[objects_attrs_text]
GO
/****** Object:  Index [idx_objects_real]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_real]') AND name = N'idx_objects_attrs_real')
 DROP INDEX [idx_objects_real] ON [dbo].[objects_attrs_real]
GO
/****** Object:  Index [idx_objects_attrs_ref]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_ref]') AND name = N'idx_objects_attrs_ref')
 DROP INDEX [idx_objects_attrs_ref] ON [dbo].[objects_attrs_link]
GO
/****** Object:  Index [idx_objects_attrs_integer]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_integer]') AND name = N'idx_objects_attrs_int')
 DROP INDEX [idx_objects_attrs_integer] ON [dbo].[objects_attrs_integer]
GO
/****** Object:  Index [idx_objects_object]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_object]') AND name = N'idx_objects_attrs_object')
 DROP INDEX [idx_objects_object] ON [dbo].[objects_attrs_object]
GO
/****** Object:  Index [idx_objects_flag]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_flag]') AND name = N'idx_objects_attrs_flag')
 DROP INDEX [idx_objects_flag] ON [dbo].[objects_attrs_flag]
GO
/****** Object:  Index [idx_objects_datetime]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_datetime]') AND name = N'idx_objects_attrs_datetime_varchar')
 DROP INDEX [idx_objects_datetime] ON [dbo].[objects_attrs_datetime]
GO
/****** Object:  Index [idx_objects]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_contents]') AND name = N'idx_objects_contents_3')
 DROP INDEX [idx_objects] ON [dbo].[objects]
GO
/****** Object:  Index [idx_objects_attrs]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_contents]') AND name = N'idx_objects_contents_3')
 DROP INDEX [idx_objects_attrs] ON [dbo].[objects_attrs]
GO
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects]') AND name = N'idx_zz_aruba')
 DROP INDEX [idx_zz_aruba] ON [dbo].[objects]
GO
/****** Object:  View [dbo].[vw_objects_attrs]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_objects_attrs]'))
 DROP VIEW [dbo].[vw_objects_attrs]
GO
/****** Object:  View [dbo].[vw_objects_c]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_objects_c]'))
 DROP VIEW [dbo].[vw_objects_c]
GO
/****** Object:  View [dbo].[vw_objects_h]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_objects_h]'))
 DROP VIEW [dbo].[vw_objects_h]
GO
/****** Object:  View [dbo].[vw_objects_p]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_objects_p]'))
 DROP VIEW [dbo].[vw_objects_p]
GO
/****** Object:  Table [dbo].[objects_types]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_types]') AND type in (N'U'))
 DROP TABLE [dbo].[objects_types]
GO
/****** Object:  Table [dbo].[objects_contents]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_contents]') AND type in (N'U'))
 DROP TABLE [dbo].[objects_contents]
GO
/****** Object:  Table [dbo].[objects_attrs_varchar]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_varchar]') AND type in (N'U'))
 DROP TABLE [dbo].[objects_attrs_varchar]
GO
/****** Object:  Table [dbo].[objects_attrs_text]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_text]') AND type in (N'U'))
 DROP TABLE [dbo].[objects_attrs_text]
GO
/****** Object:  Table [dbo].[objects_attrs_real]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_real]') AND type in (N'U'))
 DROP TABLE [dbo].[objects_attrs_real]
GO
/****** Object:  Table [dbo].[objects_attrs_link]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_link]') AND type in (N'U'))
 DROP TABLE [dbo].[objects_attrs_link]
GO
/****** Object:  Table [dbo].[objects_attrs_integer]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_integer]') AND type in (N'U'))
 DROP TABLE [dbo].[objects_attrs_integer]
GO
/****** Object:  Table [dbo].[objects_attrs_object]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_object]') AND type in (N'U'))
 DROP TABLE [dbo].[objects_attrs_object]
GO
/****** Object:  Table [dbo].[objects_attrs_flag]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_flag]') AND type in (N'U'))
 DROP TABLE [dbo].[objects_attrs_flag]
GO
/****** Object:  Table [dbo].[objects_attrs_datetime]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_datetime]') AND type in (N'U'))
 DROP TABLE [dbo].[objects_attrs_datetime]
GO
/****** Object:  Table [dbo].[objects]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects]') AND type in (N'U'))
 DROP TABLE [dbo].[objects]
GO
/****** Object:  Table [dbo].[objects_attrs]    Script Date: 01/05/2020 14:03:59 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs]') AND type in (N'U'))
 DROP TABLE [dbo].[objects_attrs]
GO
/****** Object:  Table [dbo].[objects_attrs]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[objects_attrs](
	[attribute_id] [int] NOT NULL IDENTITY(1, 1) PRIMARY KEY,
	[object_type] [varchar](50) NOT NULL,
	[attribute_code] [varchar](50) NOT NULL,
	[attribute_type] [varchar](20) NOT NULL,
	[attribute_des] [varchar](150) NOT NULL,
	[attribute_object_type] [varchar](50) NULL,
	[user_id] [int] NOT NULL
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[objects]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[objects](
	[object_id] [bigint] IDENTITY(1,1) NOT NULL,
	[object_type] [varchar](25) NOT NULL,
	[object_code] [varchar](250) NULL,
	[object_title] [varchar](250) NULL,
	[dt_ins] [datetime] NOT NULL DEFAULT getdate(),
	[dt_upd] [datetime] NULL,
	[user_id] [int] NOT NULL,
	[deleted] [int] NULL,
	[dt_del] [datetime] NULL,
	[dt_stored] [datetime] NULL,
	[zz_aruba_id] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[object_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[objects_attrs_datetime]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_datetime]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[objects_attrs_datetime](
	[object_id] [bigint] NOT NULL,
	[attribute_id] [int] NOT NULL,
	[value] [datetime] NULL, 
PRIMARY KEY CLUSTERED 
(
	[object_id] ASC, [attribute_id] ASC
)
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[objects_attrs_flag]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_flag]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[objects_attrs_flag](
	[object_id] [bigint] NOT NULL,
	[attribute_id] [int] NOT NULL,
	[value] [bit] NULL, 
PRIMARY KEY CLUSTERED 
(
	[object_id] ASC, [attribute_id] ASC
)
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[objects_attrs_integer]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_integer]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[objects_attrs_integer](
	[object_id] [bigint] NOT NULL,
	[attribute_id] [int] NOT NULL,
	[value] [int] NULL, 
PRIMARY KEY CLUSTERED 
(
	[object_id] ASC, [attribute_id] ASC
)
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[objects_attrs_object]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_object]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[objects_attrs_object](
	[object_id] [bigint] NOT NULL,
	[attribute_id] [int] NOT NULL,
	[value] [bigint] NULL, 
PRIMARY KEY CLUSTERED 
(
	[object_id] ASC, [attribute_id] ASC
)
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[objects_attrs_link]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_link]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[objects_attrs_link](
	[object_id] [bigint] NOT NULL,
	[attribute_id] [int] NOT NULL,
	[value] [varchar](250) NULL, 
PRIMARY KEY CLUSTERED 
(
	[object_id] ASC, [attribute_id] ASC
)
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[objects_attrs_real]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_real]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[objects_attrs_real](
	[object_id] [bigint] NOT NULL,
	[attribute_id] [int] NOT NULL,
	[value] [float] NULL, 
PRIMARY KEY CLUSTERED 
(
	[object_id] ASC, [attribute_id] ASC
)
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[objects_attrs_text]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_text]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[objects_attrs_text](
	[object_id] [bigint] NOT NULL,
	[attribute_id] [int] NOT NULL,
	[value] [varchar](max) NULL, 
PRIMARY KEY CLUSTERED 
(
	[object_id] ASC, [attribute_id] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[objects_attrs_varchar]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs_varchar]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[objects_attrs_varchar](
	[object_id] [bigint] NOT NULL,
	[attribute_id] [int] NOT NULL,
	[value] [varchar](250) NULL, 
PRIMARY KEY CLUSTERED 
(
	[object_id] ASC, [attribute_id] ASC
)
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[objects_contents]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_contents]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[objects_contents](
	[objects_contents_id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[object_id] [bigint] NULL,
	[child_object_id] [bigint] NULL,
	[order] [int] NULL
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[objects_types]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[objects_types]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[objects_types](
	[object_type] [varchar](25) NOT NULL,
	[object_des] [varchar](250) NOT NULL,
	[user_id] [int] NOT NULL
PRIMARY KEY CLUSTERED 
(
	[object_type] ASC, [user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  View [dbo].[vw_objects_p]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_objects_p]'))
EXEC dbo.sp_executesql @statement = N'




CREATE VIEW [dbo].[vw_objects_p]
as
 select pe.object_id as parent_id, pe.objects_contents_id, pe.[order]
   , (case when e.object_type in (''list'', ''attivita'') then 1 else 0 end) as in_list, e.*
  from [objects] e
  left join objects_contents pe on pe.child_object_id = e.object_id
' 
GO
/****** Object:  View [dbo].[vw_objects_h]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_objects_h]'))
EXEC dbo.sp_executesql @statement = N'






CREATE VIEW [dbo].[vw_objects_h]
as
 with h_objects (ref_object_id, objects_contents_id, parent_id, object_id, object_code, object_type, object_title, [order], livello, deleted, [user_id], in_list
   , dt_ins, dt_upd, dt_del, dt_stored)
  as
  (select p.parent_id as ref_object_id, p.objects_contents_id, p.parent_id, p.object_id, p.object_code, p.object_type, p.object_title, p.[order], 0 as livello, p.deleted, p.[user_id]
      , (case when p.object_type in (''list'', ''attivita'') then 1 else 0 end) as in_list, p.dt_ins, p.dt_upd, p.dt_del, p.dt_stored
     from vw_objects_p p
      --where p.parent_id is null
    union all select he.ref_object_id, e.objects_contents_id, e.parent_id, e.object_id, e.object_code, e.object_type, e.object_title, e.[order], livello + 1, e.deleted, e.[user_id]
	  , (case when he.in_list = 1 or e.object_type in (''list'', ''attivita'') then 1 else 0 end) as in_list, e.dt_ins, e.dt_upd, e.dt_del, e.dt_stored
     from vw_objects_p e
     inner join h_objects he on he.object_id = e.parent_id)
  select he.ref_object_id, he.objects_contents_id, he.parent_id, he.object_id, he.object_code, he.object_type, he.object_title, he.[order], he.livello, he.deleted, he.[user_id], he.in_list
     , he.dt_ins, he.dt_upd, he.dt_del, he.dt_stored
   from h_objects he 
' 
GO
/****** Object:  View [dbo].[vw_objects_c]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_objects_c]'))
EXEC dbo.sp_executesql @statement = N'









CREATE VIEW [dbo].[vw_objects_c]
as
 with h_objects (ref_object_id, parent_id, object_id, objects_contents_id, object_code, object_type
   , object_title, [order], livello, deleted, [user_id], dt_ins, dt_del, dt_stored)
  as
  (select r.object_id as ref_object_id, r.parent_id, r.object_id, r.objects_contents_id, r.object_code, r.object_type
      , r.object_title, r.[order], 0 as livello, r.deleted, r.[user_id], r.dt_ins, r.dt_del, r.dt_stored
     from vw_objects_h r
    union all select he.ref_object_id, e.parent_id, e.object_id, e.objects_contents_id, e.object_code, e.object_type
	  , e.object_title, e.[order], livello - 1, e.deleted, e.[user_id], e.dt_ins, e.dt_del, e.dt_stored
     from vw_objects_p e
     inner join h_objects he on he.parent_id = e.object_id and isnull(he.object_id, 0) <> isnull(he.parent_id, 0))
  select distinct he.ref_object_id, he.parent_id, he.object_id, he.objects_contents_id, he.object_code, he.object_type
    , he.object_title, he.[order], he.livello, he.deleted, he.[user_id], he.dt_ins, he.dt_del, he.dt_stored
   from h_objects he 
 
' 
GO
/****** Object:  View [dbo].[vw_objects_attrs]    Script Date: 01/05/2020 14:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_objects_attrs]'))
EXEC dbo.sp_executesql @statement = N'




CREATE view [dbo].[vw_objects_attrs]
as
 select t.*
 from (
  select e.object_id, e.object_code, e.object_type, e.object_title, e.deleted, e.[user_id]
	, e.dt_ins, e.dt_upd, e.dt_stored, a.attribute_code, a.attribute_type
    , adt.value as val_datetime, ao.value as val_object, ai.value as val_integer, al.value as val_link
    , ar.value as val_real, atxt.value as val_text, av.value as val_varchar, af.value as val_flag
   from [objects] e
   left join [objects_attrs] a on a.object_type = e.object_type and a.user_id = e.user_id 
   left join objects_attrs_datetime adt on adt.object_id = e.object_id and adt.attribute_id = a.attribute_id
   left join objects_attrs_integer ai on ai.object_id = e.object_id and ai.attribute_id = a.attribute_id
   left join objects_attrs_object ao on ao.object_id = e.object_id and ao.attribute_id = a.attribute_id
   left join objects_attrs_link al on al.object_id = e.object_id and al.attribute_id = a.attribute_id
   left join objects_attrs_real ar on ar.object_id = e.object_id and ar.attribute_id = a.attribute_id
   left join objects_attrs_text atxt on atxt.object_id = e.object_id and atxt.attribute_id = a.attribute_id
   left join objects_attrs_varchar av on av.object_id = e.object_id and av.attribute_id = a.attribute_id
   left join objects_attrs_flag af on af.object_id = e.object_id and af.attribute_id = a.attribute_id
   ) t where t.val_datetime is not null or t.val_integer is not null or t.val_link is not null
    or t.val_real is not null or t.val_text is not null or t.val_varchar is not null or t.val_flag is not null
' 
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_attrs]') AND name = N'idx_objects_attrs')
CREATE UNIQUE NONCLUSTERED INDEX [idx_objects_attrs] ON [dbo].[objects_attrs]
(
	[object_type] ASC,
	[attribute_code] ASC,
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_objects]    Script Date: 01/05/2020 14:03:59 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects]') AND name = N'idx_objects')
CREATE UNIQUE NONCLUSTERED INDEX [idx_objects] ON [dbo].[objects]
(
	[object_type] ASC,
	[object_code] ASC,
	[user_id] ASC
)
WHERE ([object_code] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_objects_contents]    Script Date: 01/05/2020 14:03:59 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_contents]') AND name = N'idx_objects_contents')
CREATE UNIQUE NONCLUSTERED INDEX [idx_objects_contents] ON [dbo].[objects_contents]
(
	[object_id] ASC,
	[child_object_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_objects_contents_3]    Script Date: 01/05/2020 14:03:59 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects_contents]') AND name = N'idx_objects_contents_3')
CREATE NONCLUSTERED INDEX [idx_objects_contents_3] ON [dbo].[objects_contents]
(
	[object_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

-- TEMPORANEO PER L'IMPORTAZIONE!
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[objects]') AND name = N'idx_zz_aruba')
CREATE UNIQUE INDEX [idx_zz_aruba] ON [dbo].[objects]
(
	[object_type] ASC,
	[zz_aruba_id] ASC,
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


---- CONFIG
/*
select * from objects
create unique index idx_zz_aruba on [objects] (object_type, zz_aruba_id);
select * from objects_types
select * from objects_attrs order by attribute_id
select * from users
*/

-- tipo evento
insert into objects_types 
 select 'tipo_evento' as object_type, 'Tipo Evento' as object_des, u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi');
insert into objects_attrs (object_type, attribute_code, attribute_type, attribute_des, attribute_object_type, user_id)
 select 'tipo_evento', 'notes', 'text', 'note particolari', null, u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi');

-- tipo ricevuta
insert into objects_types 
 select 'tipo_ricevuta', 'Tipo Ricevuta', u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi');

insert into objects_attrs (object_type, attribute_code, attribute_type, attribute_des, attribute_object_type, user_id)
 select 'tipo_ricevuta', 'notes', 'text', 'note particolari', null, u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi')
 union select 'tipo_ricevuta', 'entrata', 'flag', 'si tratta di un entrata', null, u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi');
 
 
-- evento


--  delete from objects_attrs where object_type = 'evento'
insert into objects_types 
 select 'evento', 'Evento', u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi');

insert into objects_attrs (object_type, attribute_code, attribute_type, attribute_des, attribute_object_type, user_id)
 select 'evento', 'tipo_evento', 'object', 'tipo evento', 'tipo_evento', u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi')
 union select 'evento', 'dal', 'datetime', 'dal', null, u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi')
 union select 'evento', 'al', 'datetime', 'al', null, u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi')
 union select 'evento', 'notes', 'text', 'note particolari', null, u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi');

-- ricevuta
insert into objects_types 
 select 'ricevuta', 'Ricevuta', u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi')
insert into objects_attrs (object_type, attribute_code, attribute_type, attribute_des, attribute_object_type, user_id)
 select 'ricevuta', 'evento', 'object', 'evento', 'evento', u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi')
 union select 'ricevuta', 'tipo_ricevuta', 'object', 'tipo ricevuta', 'tipo_ricevuta', u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi')
 union select 'ricevuta', 'notes', 'varchar', 'note particolari', null, u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi')
 union select 'ricevuta', 'qta', 'integer', 'q.ta pezzi', null, u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi')
 union select 'ricevuta', 'data', 'datetime', 'data ricevuta', null, u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi')
 union select 'ricevuta', 'prezzo', 'real', 'prezzo al dettaglio', null, u.user_id
  from users u
  where u.nome in ('papino', 'famigliola', 'gi');


--------- CONFIG

/*

select * from objects_types
select * from objects_attrs order by object_type, attribute_code, user_id
*/

--select * from users
/*
delete from elements where user_id = 6
delete ea from elements_attrs_datetime ea where not exists (select top 1 1 from elements where element_id = ea.element_id)
delete ea from elements_attrs_flag ea where not exists (select top 1 1 from elements where element_id = ea.element_id)
delete ea from elements_attrs_integer ea where not exists (select top 1 1 from elements where element_id = ea.element_id)
delete ea from elements_attrs_link ea where not exists (select top 1 1 from elements where element_id = ea.element_id)
delete ea from elements_attrs_real ea where not exists (select top 1 1 from elements where element_id = ea.element_id)
delete ea from elements_attrs_text ea where not exists (select top 1 1 from elements where element_id = ea.element_id)
delete ea from elements_attrs_varchar ea where not exists (select top 1 1 from elements where element_id = ea.element_id)
*/