SET NOCOUNT ON
GO

USE master
GO
if exists (select * from sysdatabases where name='Grover')
		drop database Grover
GO

DECLARE @device_directory NVARCHAR(520)
SELECT @device_directory = SUBSTRING(filename, 1, CHARINDEX(N'master.mdf', LOWER(filename)) - 1)
FROM master.dbo.sysaltfiles WHERE dbid = 1 AND fileid = 1

EXECUTE (N'CREATE DATABASE Grover
  ON PRIMARY (NAME = N''Grover'', FILENAME = N''' + @device_directory + N'grover.mdf'')
  LOG ON (NAME = N''Grover_log'',  FILENAME = N''' + @device_directory + N'grover.ldf'')')
GO

set quoted_identifier on
GO

/* Set DATEFORMAT so that the date strings are interpreted correctly regardless of
   the default DATEFORMAT on the server.
*/
SET DATEFORMAT mdy
GO
use "Grover"
GO
if exists (select * from sysobjects where id = object_id('dbo.Employees') and sysstat & 0xf = 4)
	drop procedure "dbo"."Employees"

GO
CREATE TABLE "Employees" (
	"EmployeeID" "int" IDENTITY (1, 1) NOT NULL PRIMARY KEY ,
	"LastName" nvarchar (50) NOT NULL ,
	"FirstName" nvarchar (50) NOT NULL ,
	"Title" nvarchar (50) NULL
)

