CREATE DATABASE QuanLyHeThong;
GO

USE QuanLyHeThong;
GO

-- Tạo bảng Users lưu trữ tài khoản
CREATE TABLE Users (
    Username NVARCHAR(50) PRIMARY KEY,
    Password NVARCHAR(100) NOT NULL,
    FullName NVARCHAR(100),
    Role NVARCHAR(30) DEFAULT 'User'
);
GO

-- Chèn dữ liệu mẫu để thử đăng nhập
INSERT INTO Users (Username, Password, FullName, Role) 
VALUES 
('admin', '123456', N'Quản trị viên', 'Admin'),
('user01', 'password@123', N'Nguyễn Văn A', 'User');
GO