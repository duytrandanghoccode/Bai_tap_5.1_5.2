USE QuanLyHeThong;
GO

-- 1. Bảng Khóa học (Dùng để Data Binding lên cboCourse)
CREATE TABLE Courses (
    CourseID NVARCHAR(20) PRIMARY KEY,
    CourseName NVARCHAR(100) NOT NULL,
    TuitionFee DECIMAL(18, 0)
);
GO

-- Chèn dữ liệu danh mục khóa học mẫu
INSERT INTO Courses (CourseID, CourseName, TuitionFee) VALUES
('CS01', N'Lập trình C# WinForms căn bản & nâng cao', 2500000),
('CS02', N'Thiết kế CSDL SQL Server chuyên sâu', 2000000),
('CS03', N'Lập trình Web ASP.NET Core MVC', 3500000),
('CS04', N'Phát triển ứng dụng Di động Flutter', 3000000);
GO

-- 2. Bảng lưu trữ thông tin Đăng ký học viên
CREATE TABLE StudentRegistrations (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    BirthDate DATE NOT NULL,
    Gender NVARCHAR(10),
    CourseID NVARCHAR(20) FOREIGN KEY REFERENCES Courses(CourseID),
    ReceiveEmail BIT DEFAULT 0,
    RegisterDate DATETIME DEFAULT GETDATE()
);
GO
select * from StudentRegistrations