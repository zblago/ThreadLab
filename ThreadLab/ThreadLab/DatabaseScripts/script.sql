CREATE DATABASE ThreadLab;
GO

USE ThreadLab;
GO

USE [ThreadLab]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 2.9.2026. 23:42:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ThreadIterations](
	[ThreadIterationId] [int] IDENTITY(1,1) NOT NULL,
	[ThreadJobId] [int] NOT NULL,
	[ManagedThreadId] [int] NOT NULL,
	[IsBackground] [bit] NOT NULL,
	[StartNumber] [bigint] NOT NULL,
	[EndNumber] [bigint] NOT NULL,
	[DateTimeStarted] [datetime2](7) NOT NULL,
	[DateTimeFinished] [datetime2](7) NULL,
 CONSTRAINT [PK_ThreadIterations] PRIMARY KEY CLUSTERED 
(
	[ThreadIterationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ThreadJobs](
	[ThreadJobId] [int] IDENTITY(1,1) NOT NULL,
	[ManagedThreadId] [int] NOT NULL,
	[IsBackground] [bit] NOT NULL,
	[NumberOfThreads] [int] NOT NULL,
	[NumberOfStepsPerThread] [int] NOT NULL,
	[DateTimeStarted] [datetime2](7) NOT NULL,
	[DateTimeFinished] [datetime2](7) NULL,
 CONSTRAINT [PK_ThreadJobs] PRIMARY KEY CLUSTERED 
(
	[ThreadJobId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ThreadIterations]  WITH CHECK ADD  CONSTRAINT [FK_ThreadIterations_ThreadJobs_ThreadJobId] FOREIGN KEY([ThreadJobId])
REFERENCES [dbo].[ThreadJobs] ([ThreadJobId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ThreadIterations] CHECK CONSTRAINT [FK_ThreadIterations_ThreadJobs_ThreadJobId]
GO
