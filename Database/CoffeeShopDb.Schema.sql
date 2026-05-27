-- Script tham khảo. Project dùng Entity Framework Core EnsureCreated() để tự tạo database khi chạy lần đầu.
CREATE DATABASE CoffeeShopDb;
GO
USE CoffeeShopDb;
GO
-- Nếu muốn tự quản lý schema thủ công, có thể tạo migration trong Visual Studio:
-- Tools > NuGet Package Manager > Package Manager Console
-- Add-Migration InitialCreate
-- Update-Database
