-- CoffeeShopBlueGreenDb - SQL Server schema tham khảo
-- Project ASP.NET Core MVC này tự tạo database bằng Entity Framework Core EnsureCreated()
-- khi chạy lần đầu. Database cụ thể dùng trong appsettings.json:
-- Server=(localdb)\MSSQLLocalDB;Database=CoffeeShopBlueGreenDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True

CREATE DATABASE CoffeeShopBlueGreenDb;
GO
USE CoffeeShopBlueGreenDb;
GO

CREATE TABLE Users (
    Id INT IDENTITY PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(120) NOT NULL UNIQUE,
    Phone NVARCHAR(20) NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'Member',
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    AvatarUrl NVARCHAR(300) NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL
);

CREATE TABLE Categories (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(80) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL
);

CREATE TABLE Products (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(120) NOT NULL,
    ImageUrl NVARCHAR(300) NULL,
    Price DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(1000) NULL,
    CategoryId INT NOT NULL FOREIGN KEY REFERENCES Categories(Id),
    Status NVARCHAR(30) NOT NULL,
    IsBestSeller BIT NOT NULL,
    IsTrending BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL
);

CREATE TABLE Orders (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Address NVARCHAR(300) NOT NULL,
    Note NVARCHAR(500) NULL,
    PaymentMethod NVARCHAR(50) NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(40) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL
);

CREATE TABLE OrderDetails (
    Id INT IDENTITY PRIMARY KEY,
    OrderId INT NOT NULL FOREIGN KEY REFERENCES Orders(Id),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id),
    ProductName NVARCHAR(120) NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Size NVARCHAR(10) NULL,
    Topping NVARCHAR(120) NULL,
    CreatedAt DATETIME2 NOT NULL
);

CREATE TABLE CartItems (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(Id),
    Quantity INT NOT NULL,
    Size NVARCHAR(10) NULL,
    Topping NVARCHAR(120) NULL,
    CreatedAt DATETIME2 NOT NULL
);

CREATE TABLE Addresses (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    AddressDetail NVARCHAR(300) NOT NULL,
    IsDefault BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);

CREATE TABLE Blogs (
    Id INT IDENTITY PRIMARY KEY,
    Title NVARCHAR(180) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    ImageUrl NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL
);

CREATE TABLE WebsiteVisits (
    Id INT IDENTITY PRIMARY KEY,
    IpAddress NVARCHAR(64) NULL,
    UserAgent NVARCHAR(300) NULL,
    VisitDate DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);
GO

-- Dữ liệu mẫu chi tiết gồm 40+ sản phẩm, 90+ đơn hàng, khách hàng và lượt truy cập
-- được seed tự động trong Data/DatabaseSeeder.cs để mật khẩu Admin/Member luôn được hash đúng chuẩn.
