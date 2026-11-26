CREATE DATABASE DB_BaiTap6_6_6_7;
GO

USE DB_BaiTap6_6_6_7;
GO

CREATE TABLE AtmStation (
    StationId NVARCHAR(20) PRIMARY KEY,
    Address NVARCHAR(200) NOT NULL,
    CurrentCash DECIMAL(18, 0) NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL,
    ImagePath NVARCHAR(500) NULL
);
GO

CREATE TABLE AtmReconciliationLog (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    ReconcileTime DATETIME NOT NULL DEFAULT GETDATE(),
    TotalTransactions INT NOT NULL,
    MatchedCount INT NOT NULL,
    DiscrepancyCount INT NOT NULL,
    ResultStatus NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE Substation (
    SubstationId NVARCHAR(20) PRIMARY KEY,
    SubstationName NVARCHAR(150) NOT NULL
);
GO

CREATE TABLE GridOverloadLog (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    SubstationName NVARCHAR(150) NOT NULL,
    RecordedTime DATETIME NOT NULL,
    VoltageKV FLOAT NOT NULL,
    LoadPercent FLOAT NOT NULL,
    AlertMessage NVARCHAR(250) NOT NULL
);
GO

INSERT INTO AtmStation (StationId, Address, CurrentCash, Status, ImagePath) VALUES
(N'ATM-HN01', N'Số 12 Trần Hưng Đạo, Hoàn Kiếm, Hà Nội', 1450000000, N'Đang hoạt động', N''),
(N'ATM-HN02', N'Số 235 Hoàng Quốc Việt, Cầu Giấy, Hà Nội', 820000000, N'Đang hoạt động', N''),
(N'ATM-HN03', N'Số 54 Nguyễn Chí Thanh, Đống Đa, Hà Nội', 120000000, N'Sắp hết tiền', N''),
(N'ATM-HN04', N'Số 18 Giải Phóng, Hai Bà Trưng, Hà Nội', 0, N'Bảo trì', N'');
GO

INSERT INTO Substation (SubstationId, SubstationName) VALUES
(N'TBA-HD', N'Trạm 110kV Hà Đông'),
(N'TBA-CH', N'Trạm 220kV Chèm'),
(N'TBA-MD', N'Trạm 110kV Mai Động'),
(N'TBA-LB', N'Trạm 220kV Long Biên');
GO