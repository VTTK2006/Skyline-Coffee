# CoffeeShopMVC_BlueGreen - ASP.NET Core MVC Coffee Website

Website bán Coffee + Admin Dashboard dựng cho Visual Studio theo mô hình MVC:
Models, Views, Controllers, ViewModels, Entity Framework Core và SQL Server.

## Điểm mới trong bản này
- Tone màu chủ đạo: xanh da trời + xanh lá.
- Database cụ thể: `CoffeeShopBlueGreenDb`.
- Tự tạo database khi chạy lần đầu bằng `Database.EnsureCreated()`.
- Seed sẵn nhiều dữ liệu mẫu:
  - 7 tài khoản người dùng.
  - 8 danh mục.
  - 40+ sản phẩm có ảnh minh họa SVG riêng từng món.
  - 90+ đơn hàng bán mẫu trải trên 30+ ngày.
  - Nhiều chi tiết đơn hàng để thống kê món bán chạy.
  - Lượt truy cập website mẫu.
- Admin Dashboard có biểu đồ doanh thu, đơn hàng và top sản phẩm bán chạy bằng Chart.js.
- Trang Thống kê có biểu đồ doanh thu 30 ngày, đơn hàng 30 ngày, top món bán chạy và pie chart trạng thái đơn hàng.

## Công nghệ
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server LocalDB / SQL Server
- Cookie Authentication
- Role Admin / Member
- Razor Views
- Chart.js
- CSS thuần responsive

## Cách chạy trong Visual Studio
1. Giải nén file ZIP.
2. Mở `CoffeeShopMVC.sln` bằng Visual Studio 2022 trở lên.
3. Kiểm tra connection string trong `appsettings.json`:

```json
"DefaultConnection": "Server=(localdb)\MSSQLLocalDB;Database=CoffeeShopBlueGreenDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

4. Bấm `Ctrl + F5` hoặc nút Run.
5. Database `CoffeeShopBlueGreenDb` sẽ được tạo tự động khi chạy lần đầu.

## Tài khoản mẫu
Admin:
- Email hoặc username: `admin@coffee.com` hoặc `admin`
- Password: `Admin123@`

Member:
- Email hoặc username: `member@coffee.com` hoặc `member`
- Password: `Member123@`

Các user member khác cũng dùng password: `Member123@`.

## Đường dẫn chính
- Website: `/`
- Đăng nhập: `/Auth/Login`
- Đăng ký: `/Auth/Register`
- Sản phẩm: `/Product`
- Giỏ hàng: `/Cart`
- Profile + upload avatar: `/Account/Profile`
- Admin Dashboard: `/Admin/Dashboard`
- Admin thống kê: `/Admin/Statistics`

## Lưu ý database
Nếu trước đó bạn đã tạo database cũ và muốn chạy lại sạch dữ liệu, mở Package Manager Console và chạy:

```powershell
Drop-Database
```

Sau đó chạy lại website. Vì project dùng `EnsureCreated()`, database sẽ được tạo lại khi app khởi động.

Có file schema tham khảo tại:

```text
Database/CoffeeShopBlueGreenDb.Schema.sql
```
