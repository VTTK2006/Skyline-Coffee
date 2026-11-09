# CoffeeShopMVC_BlueGreen - ASP.NET Core MVC Coffee Website

Website bán Coffee + Admin Dashboard dựng cho Visual Studio theo mô hình MVC:
Models, Views, Controllers, ViewModels, Entity Framework Core và SQL Server.

## Điểm mới trong bản này
- Tone màu chủ đạo: trắng, xanh da trời, xanh dương và xanh navy.
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
- Giá size S/M/L và topping được cộng vào giá giỏ hàng, đơn hàng và thanh toán.
- Đơn chọn chuyển khoản hiển thị QR VietQR theo đúng tổng tiền và mã đơn.
- Khôi phục mật khẩu qua OTP SMS, mã hết hạn sau 5 phút và xác minh xong chuyển về đăng nhập.
- Trang Combo có nội dung gợi ý theo từng dịp và thông tin áp dụng ưu đãi.

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

## Cấu hình VNPay Sandbox
Mã website và URL sandbox được khai báo trong `appsettings.json`. Không lưu secret key vào Git; đặt bằng biến môi trường `Vnpay__HashSecret`.

Đặt `Vnpay__PublicBaseUrl` thành URL HTTPS công khai của ứng dụng khi tích hợp IPN, ví dụ URL tunnel trong lúc phát triển. Khai báo IPN URL `https://<domain>/Vnpay/Ipn` trong Merchant Admin của VNPay. URL Return do ứng dụng tạo tại `/Vnpay/Return`.

Mặc định `Vnpay:BankCode` để trống để VNPay tự hiển thị các ngân hàng/phương thức mà merchant được hỗ trợ. Nếu VNPay xác nhận mã phương thức cụ thể được bật cho merchant, có thể đặt `Vnpay__BankCode` (ví dụ `VNPAYQR`). URL Return chỉ hiển thị kết quả đã ký; chỉ IPN hợp lệ, đúng số tiền và mã đơn mới xác nhận trạng thái đơn.

## Cấu hình SMS khôi phục mật khẩu
Luồng OTP dùng Twilio SMS. Đặt các thông tin nhạy cảm bằng biến môi trường, không commit credential:

- `Sms__Twilio__AccountSid`
- `Sms__Twilio__AuthToken`
- `Sms__Twilio__FromNumber` (số gửi đã đăng ký với Twilio, định dạng E.164)

Số điện thoại tài khoản cần là số Việt Nam hợp lệ. OTP có hiệu lực 5 phút, tối đa 5 lần nhập và giới hạn yêu cầu gửi lại.

## Đường dẫn chính
- Website: `/`
- Đăng nhập: `/Auth/Login`
- Quên mật khẩu: `/Auth/ForgotPassword`
- Đăng ký: `/Auth/Register`
- Combo: `/Home/Promo`
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
