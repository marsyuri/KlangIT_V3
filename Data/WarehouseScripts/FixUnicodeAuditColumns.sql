-- ============================================================================
-- Fix BorrowITName / ReturnITName columns varchar → nvarchar (Warehouse DB)
-- Purpose: ช่อง ผู้ดำเนินการ (IT) เก็บ DisplayName ภาษาไทยไม่ได้
-- แสดงผลเป็น "????? ???????????" แทน
--
-- Scope (แก้เฉพาะ 2 column นี้):
--   BorrowHistory.BorrowITName → NVARCHAR(1000)
--   BorrowHistory.ReturnITName → NVARCHAR(1000)
--
-- ไม่แก้ CreatedBy / ModifiedBy — ออกแบบให้เป็น username ภาษาอังกฤษ (ตัด @domain)
--
-- Run once on IT_LPT_Warehouse DB (แนะนำ backup ก่อน)
-- ============================================================================

USE [IT_LPT_Warehouse];
GO

BEGIN TRANSACTION;

ALTER TABLE [BorrowHistory] ALTER COLUMN [BorrowITName] NVARCHAR(1000) NOT NULL;
ALTER TABLE [BorrowHistory] ALTER COLUMN [ReturnITName] NVARCHAR(1000) NULL;

COMMIT TRANSACTION;
GO

-- ============================================================================
-- Optional: ล้างข้อมูลเก่าที่ถูกแปลงเป็น "????" (เปิด comment ถ้าต้องการ)
-- ============================================================================

-- UPDATE [BorrowHistory] SET [BorrowITName] = N'(ไม่ระบุ)' WHERE [BorrowITName] LIKE '%?%';
-- UPDATE [BorrowHistory] SET [ReturnITName] = N'(ไม่ระบุ)' WHERE [ReturnITName] LIKE '%?%';
