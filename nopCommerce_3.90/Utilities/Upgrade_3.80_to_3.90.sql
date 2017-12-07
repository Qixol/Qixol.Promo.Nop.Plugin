/*
 * update script for Qixol Promo Nop Commerce plugins from v3.80 to v3.90
 *
 */

/* schema changes */
-- removal of obsolete PromoIssuedCoupons table (data held in PromoOrder - PromoOrderCoupon tables)
if exists(select * from sys.tables where [Name] = 'PromoIssuedCoupon')
	DROP TABLE PromoIssuedCoupon

-- alter PromoOrder table to have customerId and update this from the Order table
BEGIN TRANSACTION
	SET QUOTED_IDENTIFIER ON
	SET ARITHABORT ON
	SET NUMERIC_ROUNDABORT OFF
	SET CONCAT_NULL_YIELDS_NULL ON
	SET ANSI_NULLS ON
	SET ANSI_PADDING ON
	SET ANSI_WARNINGS ON
COMMIT
GO

BEGIN TRANSACTION

CREATE TABLE dbo.Tmp_PromoOrder
	(
	Id int NOT NULL IDENTITY (1, 1),
	OrderId int NOT NULL,
	CustomerId int NOT NULL,
	RequestXml nvarchar(MAX) NULL,
	ResponseXml nvarchar(MAX) NULL,
	DeliveryOriginalPrice decimal(18, 2) NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_PromoOrder SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_PromoOrder ON
GO
IF EXISTS(SELECT * FROM dbo.PromoOrder)
	 EXEC('INSERT INTO dbo.Tmp_PromoOrder (Id, OrderId, CustomerId, RequestXml, ResponseXml, DeliveryOriginalPrice)
		SELECT po.Id, po.OrderId, no.CustomerId, po.RequestXml, po.ResponseXml, po.DeliveryOriginalPrice FROM dbo.PromoOrder po WITH (HOLDLOCK TABLOCKX) inner join dbo.[Order] no WITH (HOLDLOCK TABLOCKX) on po.OrderId = no.Id')
GO
SET IDENTITY_INSERT dbo.Tmp_PromoOrder OFF
GO
ALTER TABLE dbo.PromoOrderCoupon
	DROP CONSTRAINT PromoOrderCoupon_PromoOrder
GO
ALTER TABLE dbo.PromoOrderItem
	DROP CONSTRAINT PromoOrderItem_PromoOrder
GO
DROP TABLE dbo.PromoOrder
GO
EXECUTE sp_rename N'dbo.Tmp_PromoOrder', N'PromoOrder', 'OBJECT' 
GO
ALTER TABLE dbo.PromoOrder ADD CONSTRAINT
	PK__PromoOrd__3214EC07C9E60DE1 PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.PromoOrderItem ADD CONSTRAINT
	PromoOrderItem_PromoOrder FOREIGN KEY
	(
	PromoOrderId
	) REFERENCES dbo.PromoOrder
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.PromoOrderItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.PromoOrderCoupon ADD CONSTRAINT
	PromoOrderCoupon_PromoOrder FOREIGN KEY
	(
	PromoOrderId
	) REFERENCES dbo.PromoOrder
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.PromoOrderCoupon SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
