CREATE TABLE [dbo].[Order]
(
	[Id] [int] IDENTITY (1,1) NOT NULL,
	[Placed] [datetime] NULL,
	[Customer_id] [int] NULL,
	CONSTRAINT [PK__Order__3214EC070AD2A005] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY  = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[OrderItem]
(
	[Id] [int] IDENTITY (1,1) NOT NULL,
	[Cost] [decimal] (19,5) NULL,
	[Product_id] [int] NULL,
	[Order_id] [int] NULL,
	CONSTRAINT [PK__OrderIte__3214EC070EA330E9] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY  = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[Product]
(
	[Id] [int] IDENTITY (1,1) NOT NULL,
	[Price] [decimal] (19,5) NULL,
	[Name] [nvarchar] (100) COLLATE Latin1_General_CI_AS NULL,
	CONSTRAINT [PK__Product__3214EC0703317E3D] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY  = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[Address]
(
	[Id] [int] IDENTITY (1,1) NOT NULL,
	[TownOrArea] [nvarchar] (100) COLLATE Latin1_General_CI_AS NULL,
	[Country] [nvarchar] (100) COLLATE Latin1_General_CI_AS NULL,
	[HouseNumberOrName] [nvarchar] (100) COLLATE Latin1_General_CI_AS NULL,
	[Street] [nvarchar] (100) COLLATE Latin1_General_CI_AS NULL,
	CONSTRAINT [PK__Address__3214EC0707020F21] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY  = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[Customer]
(
	[Id] [int] IDENTITY (1,1) NOT NULL,
	[Name] [nvarchar] (100) COLLATE Latin1_General_CI_AS NULL,
	[Address_id] [int] NULL,
	CONSTRAINT [PK__Customer__3214EC077F60ED59] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY  = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[OrderItem] ADD CONSTRAINT [FK3EF888586B613A63] FOREIGN KEY
	(
		[Product_id]
	)
	REFERENCES [dbo].[Product]
	(
		[Id]
	)
GO

ALTER TABLE [dbo].[OrderItem] ADD CONSTRAINT [FK3EF88858812711D6] FOREIGN KEY
	(
		[Order_id]
	)
	REFERENCES [dbo].[Order]
	(
		[Id]
	)
GO

ALTER TABLE [dbo].[Customer] ADD CONSTRAINT [FKFE9A39C0A3472FF7] FOREIGN KEY
	(
		[Address_id]
	)
	REFERENCES [dbo].[Address]
	(
		[Id]
	)
GO

ALTER TABLE [dbo].[Order] ADD CONSTRAINT [FK3117099B9BB6B0EA] FOREIGN KEY
	(
		[Customer_id]
	)
	REFERENCES [dbo].[Customer]
	(
		[Id]
	)
GO