/****** Object:  Table [dbo].[Animals]    Script Date: 6/11/2019 12:13:01 AM ******/ 
SET ANSI_NULLS ON 

SET QUOTED_IDENTIFIER ON 

CREATE TABLE [dbo].[Animals]( 
[Id] [int] IDENTITY(1,1) NOT NULL, 
[DateOfFlight] [nvarchar](15) NOT NULL, 
[LocationOfFlight] [nvarchar](50) NOT NULL, 
[Season] [nvarchar](50) NOT NULL, 
[ImageName] [nvarchar](50) NOT NULL, 
[RegionName] [nvarchar](50) NOT NULL, 
[Label] [nvarchar](50) NULL, 
[Probability] [float] NOT NULL, 
[BoundingBox] [nvarchar](100) NULL, 
[URL] [nvarchar](300) NOT NULL 
) ON [PRIMARY] 

/****** Object:  Table [dbo].[Paragrass]    Script Date: 6/11/2019 12:16:11 AM ******/ 
SET ANSI_NULLS ON 

SET QUOTED_IDENTIFIER ON 

CREATE TABLE [dbo].[Paragrass]( 
[Id] [int] IDENTITY(1,1) NOT NULL, 
[DateOfFlight] [nvarchar](15) NOT NULL, 
[LocationOfFlight] [nvarchar](50) NOT NULL, 
[Season] [nvarchar](50) NOT NULL, 
[ImageName] [nvarchar](50) NOT NULL, 
[RegionName] [nvarchar](50) NOT NULL, 
[Label] [nvarchar](50) NULL, 
[Probability] [float] NOT NULL, 
[URL] [nvarchar](300) NOT NULL 
) ON [PRIMARY]
