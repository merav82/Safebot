USE [master]
GO
/****** Object:  Database [MTurkDB]    Script Date: 22-Feb-19 11:35:35 ******/
CREATE DATABASE [MTurkDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'MTurkDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS\MSSQL\DATA\MTurkDB.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'MTurkDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS\MSSQL\DATA\MTurkDB_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [MTurkDB] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [MTurkDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [MTurkDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [MTurkDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [MTurkDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [MTurkDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [MTurkDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [MTurkDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [MTurkDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [MTurkDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [MTurkDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [MTurkDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [MTurkDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [MTurkDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [MTurkDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [MTurkDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [MTurkDB] SET  DISABLE_BROKER 
GO
ALTER DATABASE [MTurkDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [MTurkDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [MTurkDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [MTurkDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [MTurkDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [MTurkDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [MTurkDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [MTurkDB] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [MTurkDB] SET  MULTI_USER 
GO
ALTER DATABASE [MTurkDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [MTurkDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [MTurkDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [MTurkDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [MTurkDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [MTurkDB] SET QUERY_STORE = OFF
GO
USE [MTurkDB]
GO
/****** Object:  User [amosygal]    Script Date: 22-Feb-19 11:35:41 ******/
CREATE USER [amosygal] FOR LOGIN [amosygal] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  Table [dbo].[BonusGrant]    Script Date: 22-Feb-19 11:35:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BonusGrant](
	[workerId] [nchar](20) NOT NULL,
	[expNum] [int] NULL,
	[bonusCompleted] [int] NULL,
	[reward] [real] NOT NULL,
	[assignmentId] [nchar](40) NULL,
	[status] [nchar](10) NULL,
	[gameName] [nchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EcChat]    Script Date: 22-Feb-19 11:35:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EcChat](
	[gameId] [int] NOT NULL,
	[time] [datetime] NOT NULL,
	[isAgentTalk] [bit] NOT NULL,
	[sentence] [text] NULL,
	[runningId] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_EcChats] PRIMARY KEY CLUSTERED 
(
	[runningId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EcGames]    Script Date: 22-Feb-19 11:35:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EcGames](
	[expNum] [int] NULL,
	[workerId] [nchar](20) NOT NULL,
	[assignmentId] [nchar](40) NOT NULL,
	[gameId] [int] IDENTITY(1,1) NOT NULL,
	[isComplete] [bit] NOT NULL,
	[insertTime] [datetime] NOT NULL,
	[isLearningMode] [bit] NOT NULL,
	[completeTime] [datetime] NULL,
	[bonus] [int] NULL,
	[tasksCompleted] [int] NOT NULL,
	[currentTask] [nchar](500) NULL,
	[finished] [bit] NOT NULL,
 CONSTRAINT [PK_EcGames] PRIMARY KEY CLUSTERED 
(
	[gameId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EcQuest]    Script Date: 22-Feb-19 11:35:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EcQuest](
	[workerId] [nchar](20) NOT NULL,
	[assignmentId] [nchar](40) NOT NULL,
	[program] [int] NOT NULL,
	[smart] [int] NOT NULL,
	[understood] [int] NOT NULL,
	[mobile] [int] NOT NULL,
	[comments] [text] NOT NULL,
	[tech] [int] NOT NULL,
 CONSTRAINT [PK_EcQuests] PRIMARY KEY CLUSTERED 
(
	[workerId] ASC,
	[assignmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EcTasksCompleted]    Script Date: 22-Feb-19 11:35:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EcTasksCompleted](
	[gameId] [int] NOT NULL,
	[taskName] [nchar](20) NOT NULL,
	[completedTime] [datetime] NOT NULL,
 CONSTRAINT [PK_EcTasksCompleteds] PRIMARY KEY CLUSTERED 
(
	[gameId] ASC,
	[taskName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EcWorkers]    Script Date: 22-Feb-19 11:35:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EcWorkers](
	[workerId] [nchar](20) NOT NULL,
	[age] [int] NOT NULL,
	[country] [nchar](20) NOT NULL,
	[education] [nchar](20) NOT NULL,
	[gender] [nchar](10) NOT NULL,
	[culture] [nchar](10) NULL,
 CONSTRAINT [PK_EcWorkers] PRIMARY KEY CLUSTERED 
(
	[workerId] ASC,
	[age] ASC,
	[country] ASC,
	[education] ASC,
	[gender] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GnBonus]    Script Date: 22-Feb-19 11:35:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GnBonus](
	[workerId] [nchar](20) NOT NULL,
	[assignmentId] [nchar](40) NOT NULL,
	[userBonusNumber] [int] NOT NULL,
	[gameName] [nchar](10) NOT NULL,
	[bonusAmount] [float] NOT NULL,
	[tryingToGrant] [bit] NOT NULL,
	[grantedSuccefully] [bit] NOT NULL,
	[createdTime] [datetime] NOT NULL,
 CONSTRAINT [PK_GnBonus] PRIMARY KEY CLUSTERED 
(
	[workerId] ASC,
	[assignmentId] ASC,
	[userBonusNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GnExceptions]    Script Date: 22-Feb-19 11:35:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GnExceptions](
	[message] [text] NOT NULL,
	[timeOccur] [datetime] NOT NULL,
	[logLevel] [nchar](10) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[EcChat] ADD  CONSTRAINT [DF_EcChat_time]  DEFAULT (getdate()) FOR [time]
GO
ALTER TABLE [dbo].[EcGames] ADD  CONSTRAINT [DF_EcGames_expNum]  DEFAULT ((0)) FOR [expNum]
GO
ALTER TABLE [dbo].[EcGames] ADD  CONSTRAINT [DF_EcGames_isComplete]  DEFAULT ((0)) FOR [isComplete]
GO
ALTER TABLE [dbo].[EcGames] ADD  CONSTRAINT [DF_EcGames_insertTime]  DEFAULT (getdate()) FOR [insertTime]
GO
ALTER TABLE [dbo].[EcGames] ADD  CONSTRAINT [DF_EcGames_tasksCompleted]  DEFAULT ((0)) FOR [tasksCompleted]
GO
ALTER TABLE [dbo].[EcGames] ADD  CONSTRAINT [DF_EcGames_finished]  DEFAULT ((0)) FOR [finished]
GO
ALTER TABLE [dbo].[EcTasksCompleted] ADD  CONSTRAINT [DF_EcTasksCompleted_completedTime]  DEFAULT (getdate()) FOR [completedTime]
GO
ALTER TABLE [dbo].[GnExceptions] ADD  CONSTRAINT [DF_GnExceptions_timeOccur]  DEFAULT (getdate()) FOR [timeOccur]
GO
/****** Object:  StoredProcedure [dbo].[EcIsWorkerAllowedSP]    Script Date: 22-Feb-19 11:35:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EcIsWorkerAllowedSP] 
	-- Add the parameters for the stored procedure here
	@workerId nchar(20),
	@assignmentId nchar(40),
	@isAllowed int out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	set @isAllowed = 0;
	declare @doneInPast as int;
	select @doneInPast = count(*) from EcGames where workerId=@workerId and finished=1;
	if (@doneInPast < 1)
		set @isAllowed = 1;

	Return @@Error

END
GO
/****** Object:  StoredProcedure [dbo].[EcNewWorkerSP]    Script Date: 22-Feb-19 11:35:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EcNewWorkerSP] 
	-- Add the parameters for the stored procedure here
	@workerId nchar(20), 
    @age int, 
    @country nchar(20), 
    @education nchar(20), 
    @gender nchar(10),
	@culture int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @doneInPast as int;
	select @doneInPast = count(*) from EcWorkers where workerId=@workerId;
	if (@doneInPast < 1)
		insert into EcWorkers (workerId, age, country, education, gender, culture) 
			values (@workerId, @age, @country, @education, @gender, 'USA-AC');
	Return @@Error

END
GO
/****** Object:  StoredProcedure [dbo].[GnGrantingBonusSP]    Script Date: 22-Feb-19 11:35:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GnGrantingBonusSP] 
	-- Add the parameters for the stored procedure here
	@workerId nchar(20),
	@assignmentId nchar(40),
	@gameName nchar(10),
	@bonusAmount real,
	@requestToGrant bit,
	@bonusGrantedSuccessfully bit,
	@grantBonus bit out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	if (@requestTogrant = 0)
	begin
		set @grantBonus = 0;
		if (@bonusGrantedSuccessfully = 1)
		begin
			update BonusGrants set status='granted' where workerId=@workerId and assignmentId=@assignmentId and (gameName = null or gameName = @gameName);
			if (@@RowCount = 0)
				insert into BonusGrants (workerId, assignmentId, gameName, reward, status) values (@workerId, @assignmentId, @gameName, @bonusAmount, 'granted');
			return @@Error;
		end
		update BonusGrants set status='failed' where workerId=@workerId and assignmentId=@assignmentId and (gameName = null or gameName = @gameName) and status='inProg';
		return @@Error;
	end
	declare @alreadyIn as int;
	select @alreadyIn = count(*) from BonusGrants where workerId=@workerId and assignmentId=@assignmentId and (gameName = null or gameName = @gameName) and (status='inProg' or status='granted');
	if (@alreadyIn > 0)
	begin
		set @grantBonus = 0;
		return @@Error;
	end
	insert into BonusGrants (workerId, assignmentId, gameName, reward, status) values (@workerId, @assignmentId, @gameName, @bonusAmount, 'inProg');
	set @grantBonus = 1;
	Return @@Error

END
GO
USE [master]
GO
ALTER DATABASE [MTurkDB] SET  READ_WRITE 
GO
