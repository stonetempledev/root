﻿USE [master]
GO
/****** Object:  Database [molinafy]    Script Date: 05/08/2019 23:16:30 ******/
CREATE DATABASE [molinafy] ON  PRIMARY 
( NAME = N'molinafy', FILENAME = N'c:\Program Files\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA\molinafy.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'molinafy_log', FILENAME = N'c:\Program Files\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA\molinafy_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [molinafy] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [molinafy].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [molinafy] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [molinafy] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [molinafy] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [molinafy] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [molinafy] SET ARITHABORT OFF 
GO
ALTER DATABASE [molinafy] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [molinafy] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [molinafy] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [molinafy] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [molinafy] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [molinafy] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [molinafy] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [molinafy] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [molinafy] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [molinafy] SET  DISABLE_BROKER 
GO
ALTER DATABASE [molinafy] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [molinafy] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [molinafy] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [molinafy] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [molinafy] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [molinafy] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [molinafy] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [molinafy] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [molinafy] SET  MULTI_USER 
GO
ALTER DATABASE [molinafy] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [molinafy] SET DB_CHAINING OFF 
GO
USE [molinafy]
GO
/****** Object:  User [molina]    Script Date: 05/08/2019 23:16:30 ******/
CREATE USER [molina] FOR LOGIN [molina] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  Table [dbo].[client_ips]    Script Date: 05/08/2019 23:16:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[client_ips](
	[id_client] [int] IDENTITY(1,1) NOT NULL,
	[ip_address] [varchar](30) NOT NULL,
	[dt_ins] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_client] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[client_logged]    Script Date: 05/08/2019 23:16:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[client_logged](
	[id_client] [int] NOT NULL,
	[id_user] [int] NOT NULL,
	[for_days] [int] NULL,
	[dt_ins] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[users]    Script Date: 05/08/2019 23:16:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[users](
	[id_user] [int] IDENTITY(1,1) NOT NULL,
	[user_name] [varchar](30) NOT NULL,
	[password] [varchar](50) NULL,
	[dt_ins] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_user] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_client_ips]    Script Date: 05/08/2019 23:16:30 ******/
CREATE NONCLUSTERED INDEX [idx_client_ips] ON [dbo].[client_ips]
(
	[ip_address] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_client_logged]    Script Date: 05/08/2019 23:16:30 ******/
CREATE UNIQUE NONCLUSTERED INDEX [idx_client_logged] ON [dbo].[client_logged]
(
	[id_client] ASC,
	[id_user] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_users]    Script Date: 05/08/2019 23:16:30 ******/
CREATE UNIQUE NONCLUSTERED INDEX [idx_users] ON [dbo].[users]
(
	[user_name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[client_ips] ADD  DEFAULT (getdate()) FOR [dt_ins]
GO
ALTER TABLE [dbo].[client_logged] ADD  DEFAULT (getdate()) FOR [dt_ins]
GO
ALTER TABLE [dbo].[users] ADD  DEFAULT (getdate()) FOR [dt_ins]
GO
USE [master]
GO
ALTER DATABASE [molinafy] SET  READ_WRITE 
GO
