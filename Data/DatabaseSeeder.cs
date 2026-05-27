using CoffeeShopMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopMVC.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(CoffeeDbContext db, IPasswordHasher<User> hasher)
    {
        if (!await db.Users.AnyAsync())
        {
            var users = new List<User>
            {
                new User { Username = "admin", Email = "admin@coffee.com", Phone = "0900000001", Role = "Admin", Status = "Active", AvatarUrl = "/images/default-avatar.svg", CreatedAt = DateTime.Now.AddDays(-90) },
                new User { Username = "member", Email = "member@coffee.com", Phone = "0900000002", Role = "Member", Status = "Active", AvatarUrl = "/images/default-avatar.svg", CreatedAt = DateTime.Now.AddDays(-80) },
                new User { Username = "minhnguyen", Email = "minh@coffee.com", Phone = "0912345678", Role = "Member", Status = "Active", AvatarUrl = "/images/default-avatar.svg", CreatedAt = DateTime.Now.AddDays(-62) },
                new User { Username = "hoapham", Email = "hoa@coffee.com", Phone = "0923456789", Role = "Member", Status = "Active", AvatarUrl = "/images/default-avatar.svg", CreatedAt = DateTime.Now.AddDays(-54) },
                new User { Username = "longtran", Email = "long@coffee.com", Phone = "0934567890", Role = "Member", Status = "Active", AvatarUrl = "/images/default-avatar.svg", CreatedAt = DateTime.Now.AddDays(-40) },
                new User { Username = "linhdo", Email = "linh@coffee.com", Phone = "0945678901", Role = "Member", Status = "Active", AvatarUrl = "/images/default-avatar.svg", CreatedAt = DateTime.Now.AddDays(-22) },
                new User { Username = "ankhanh", Email = "ankhanh@coffee.com", Phone = "0956789012", Role = "Member", Status = "Active", AvatarUrl = "/images/default-avatar.svg", CreatedAt = DateTime.Now.AddDays(-15) }
            };

            foreach (var user in users)
            {
                user.PasswordHash = hasher.HashPassword(user, user.Role == "Admin" ? "Admin123@" : "Member123@");
            }

            db.Users.AddRange(users);
            await db.SaveChangesAsync();
        }

        if (!await db.Categories.AnyAsync())
        {
            var names = new[] { "Coffee", "Trà sữa", "Trà trái cây", "Nước ép", "Đá xay", "Soda", "Bánh ngọt", "Đồ ăn vặt" };
            db.Categories.AddRange(names.Select(n => new Category { Name = n, CreatedAt = DateTime.Now }));
            await db.SaveChangesAsync();
        }

        if (!await db.Products.AnyAsync())
        {
            int cat(string name) => db.Categories.First(c => c.Name == name).Id;
            db.Products.AddRange(
                new Product { Name = "Cà phê sữa đá", Price = 25000, CategoryId = cat("Coffee"), ImageUrl = "/images/products/ca-phe-sua-da.svg", Description = "Cà phê Việt đậm đà hòa cùng sữa đặc béo ngậy, dùng lạnh cực tỉnh táo.", IsBestSeller = true, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-1) },
                new Product { Name = "Cà phê đen", Price = 22000, CategoryId = cat("Coffee"), ImageUrl = "/images/products/ca-phe-den.svg", Description = "Cà phê rang xay nguyên chất, vị đắng mạnh, hậu vị thơm sâu.", IsBestSeller = true, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-2) },
                new Product { Name = "Bạc xỉu", Price = 28000, CategoryId = cat("Coffee"), ImageUrl = "/images/products/bac-xiu.svg", Description = "Sữa tươi béo thơm kết hợp vị cà phê nhẹ nhàng, phù hợp mọi thời điểm.", IsBestSeller = true, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-3) },
                new Product { Name = "Latte", Price = 42000, CategoryId = cat("Coffee"), ImageUrl = "/images/products/latte.svg", Description = "Espresso hòa quyện sữa tươi mềm mịn, vị cân bằng và hiện đại.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-4) },
                new Product { Name = "Cappuccino", Price = 45000, CategoryId = cat("Coffee"), ImageUrl = "/images/products/cappuccino.svg", Description = "Lớp foam sữa bồng bềnh, hương espresso rõ nét và thơm béo.", IsBestSeller = true, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-5) },
                new Product { Name = "Espresso", Price = 30000, CategoryId = cat("Coffee"), ImageUrl = "/images/products/espresso.svg", Description = "Shot espresso nguyên bản, đậm vị, mạnh mẽ và nhanh gọn.", IsBestSeller = false, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-6) },
                new Product { Name = "Americano", Price = 35000, CategoryId = cat("Coffee"), ImageUrl = "/images/products/americano.svg", Description = "Espresso pha cùng nước nóng, vị nhẹ hơn nhưng vẫn giữ hương cà phê.", IsBestSeller = false, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-7) },
                new Product { Name = "Mocha", Price = 47000, CategoryId = cat("Coffee"), ImageUrl = "/images/products/mocha.svg", Description = "Cà phê, sữa và chocolate tạo nên hương vị ngọt đắng hấp dẫn.", IsBestSeller = true, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-8) },
                new Product { Name = "Cold Brew Cam Sành", Price = 52000, CategoryId = cat("Coffee"), ImageUrl = "/images/products/cold-brew-cam-sanh.svg", Description = "Cold brew ủ chậm kết hợp cam sành tươi, mát lạnh và giàu hương trái cây.", IsBestSeller = true, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-9) },
                new Product { Name = "Coconut Coffee", Price = 49000, CategoryId = cat("Coffee"), ImageUrl = "/images/products/coconut-coffee.svg", Description = "Cà phê dừa béo thơm, xay mịn cùng đá tạo cảm giác nhiệt đới.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-10) },
                new Product { Name = "Caramel Macchiato", Price = 54000, CategoryId = cat("Coffee"), ImageUrl = "/images/products/caramel-macchiato.svg", Description = "Espresso, sữa và caramel thơm ngọt, topping sốt hấp dẫn.", IsBestSeller = true, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-11) },
                new Product { Name = "Trà sữa trân châu", Price = 35000, CategoryId = cat("Trà sữa"), ImageUrl = "/images/products/tra-sua-tran-chau.svg", Description = "Trà sữa truyền thống thơm trà, kèm trân châu dai ngon.", IsBestSeller = true, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-12) },
                new Product { Name = "Trà sữa matcha", Price = 39000, CategoryId = cat("Trà sữa"), ImageUrl = "/images/products/tra-sua-matcha.svg", Description = "Matcha thanh nhẹ hòa cùng sữa tươi, vị ngọt vừa và mùi trà xanh rõ.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-13) },
                new Product { Name = "Trà sữa ô long", Price = 38000, CategoryId = cat("Trà sữa"), ImageUrl = "/images/products/tra-sua-o-long.svg", Description = "Vị ô long rang thơm, hậu ngọt nhẹ, phù hợp khách thích vị trà rõ.", IsBestSeller = true, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-14) },
                new Product { Name = "Trà sữa socola", Price = 40000, CategoryId = cat("Trà sữa"), ImageUrl = "/images/products/tra-sua-socola.svg", Description = "Socola béo thơm hòa quyện nền trà sữa mềm mịn.", IsBestSeller = false, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-15) },
                new Product { Name = "Trà sữa khoai môn", Price = 41000, CategoryId = cat("Trà sữa"), ImageUrl = "/images/products/tra-sua-khoai-mon.svg", Description = "Khoai môn bùi thơm, màu tím dịu và vị sữa béo ngậy.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-16) },
                new Product { Name = "Trà đào cam sả", Price = 39000, CategoryId = cat("Trà trái cây"), ImageUrl = "/images/products/tra-dao-cam-sa.svg", Description = "Trà đào thơm, cam tươi và sả dịu nhẹ, best seller mùa hè.", IsBestSeller = true, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-17) },
                new Product { Name = "Trà vải hoa hồng", Price = 42000, CategoryId = cat("Trà trái cây"), ImageUrl = "/images/products/tra-vai-hoa-hong.svg", Description = "Trà vải ngọt thanh cùng hương hoa hồng nhẹ nhàng.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-18) },
                new Product { Name = "Trà chanh mật ong", Price = 32000, CategoryId = cat("Trà trái cây"), ImageUrl = "/images/products/tra-chanh-mat-ong.svg", Description = "Vị chanh tươi kết hợp mật ong dịu ngọt, thanh mát cổ họng.", IsBestSeller = false, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-19) },
                new Product { Name = "Trà dâu bạc hà", Price = 41000, CategoryId = cat("Trà trái cây"), ImageUrl = "/images/products/tra-dau-bac-ha.svg", Description = "Dâu tươi chua ngọt kết hợp bạc hà mát lạnh.", IsBestSeller = true, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-20) },
                new Product { Name = "Trà nhiệt đới", Price = 45000, CategoryId = cat("Trà trái cây"), ImageUrl = "/images/products/tra-nhiet-doi.svg", Description = "Hỗn hợp trái cây nhiệt đới cùng nền trà thanh, nhiều màu sắc.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-21) },
                new Product { Name = "Nước ép cam", Price = 38000, CategoryId = cat("Nước ép"), ImageUrl = "/images/products/nuoc-ep-cam.svg", Description = "Cam tươi ép nguyên chất, giàu vitamin và vị chua ngọt tự nhiên.", IsBestSeller = true, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-22) },
                new Product { Name = "Nước ép táo", Price = 39000, CategoryId = cat("Nước ép"), ImageUrl = "/images/products/nuoc-ep-tao.svg", Description = "Táo tươi ép mát, vị ngọt dịu và hương thơm nhẹ.", IsBestSeller = false, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-23) },
                new Product { Name = "Nước ép dưa hấu", Price = 35000, CategoryId = cat("Nước ép"), ImageUrl = "/images/products/nuoc-ep-dua-hau.svg", Description = "Dưa hấu đỏ mọng ép lạnh, giải nhiệt cực nhanh.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-24) },
                new Product { Name = "Nước ép cà rốt", Price = 36000, CategoryId = cat("Nước ép"), ImageUrl = "/images/products/nuoc-ep-ca-rot.svg", Description = "Cà rốt tươi ép mộc, phù hợp khách thích đồ uống lành mạnh.", IsBestSeller = false, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-25) },
                new Product { Name = "Nước ép ổi hồng", Price = 37000, CategoryId = cat("Nước ép"), ImageUrl = "/images/products/nuoc-ep-oi-hong.svg", Description = "Ổi hồng ép tươi, vị thanh, thơm và giàu vitamin.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-26) },
                new Product { Name = "Matcha đá xay", Price = 49000, CategoryId = cat("Đá xay"), ImageUrl = "/images/products/matcha-da-xay.svg", Description = "Matcha xay cùng đá, phủ kem béo, hương trà xanh nổi bật.", IsBestSeller = true, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-27) },
                new Product { Name = "Chocolate đá xay", Price = 50000, CategoryId = cat("Đá xay"), ImageUrl = "/images/products/chocolate-da-xay.svg", Description = "Chocolate đậm vị xay mịn cùng sữa và đá, phù hợp team ngọt.", IsBestSeller = true, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-28) },
                new Product { Name = "Cookies đá xay", Price = 52000, CategoryId = cat("Đá xay"), ImageUrl = "/images/products/cookies-da-xay.svg", Description = "Bánh cookies nghiền cùng kem sữa, vị béo thơm và giòn vui miệng.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-29) },
                new Product { Name = "Caramel Frappe", Price = 54000, CategoryId = cat("Đá xay"), ImageUrl = "/images/products/caramel-frappe.svg", Description = "Frappe caramel mát lạnh, béo ngậy và thơm sốt caramel.", IsBestSeller = true, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-30) },
                new Product { Name = "Soda việt quất", Price = 36000, CategoryId = cat("Soda"), ImageUrl = "/images/products/soda-viet-quat.svg", Description = "Soda mát lạnh, vị việt quất chua ngọt và màu sắc bắt mắt.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-31) },
                new Product { Name = "Soda chanh dây", Price = 35000, CategoryId = cat("Soda"), ImageUrl = "/images/products/soda-chanh-day.svg", Description = "Chanh dây thơm chua nhẹ kết hợp soda sảng khoái.", IsBestSeller = true, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-32) },
                new Product { Name = "Soda dâu", Price = 37000, CategoryId = cat("Soda"), ImageUrl = "/images/products/soda-dau.svg", Description = "Soda dâu tươi ngọt thơm, rất hợp ngày nắng.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-33) },
                new Product { Name = "Soda bạc hà", Price = 34000, CategoryId = cat("Soda"), ImageUrl = "/images/products/soda-bac-ha.svg", Description = "Bạc hà mát lạnh cùng soda có gas nhẹ, giải khát tức thì.", IsBestSeller = false, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-34) },
                new Product { Name = "Bánh tiramisu", Price = 42000, CategoryId = cat("Bánh ngọt"), ImageUrl = "/images/products/banh-tiramisu.svg", Description = "Bánh mềm mịn, thơm cà phê và cacao, vị ngọt cân bằng.", IsBestSeller = true, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-35) },
                new Product { Name = "Bánh croissant", Price = 32000, CategoryId = cat("Bánh ngọt"), ImageUrl = "/images/products/banh-croissant.svg", Description = "Croissant bơ thơm, vỏ giòn xốp, hợp dùng cùng latte.", IsBestSeller = true, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-36) },
                new Product { Name = "Cheesecake việt quất", Price = 48000, CategoryId = cat("Bánh ngọt"), ImageUrl = "/images/products/cheesecake-viet-quat.svg", Description = "Cheesecake béo mịn kèm sốt việt quất chua nhẹ.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-37) },
                new Product { Name = "Muffin socola", Price = 35000, CategoryId = cat("Bánh ngọt"), ImageUrl = "/images/products/muffin-socola.svg", Description = "Muffin mềm thơm cacao, có chip socola bên trong.", IsBestSeller = false, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-38) },
                new Product { Name = "Bánh su kem", Price = 30000, CategoryId = cat("Bánh ngọt"), ImageUrl = "/images/products/banh-su-kem.svg", Description = "Vỏ bánh mềm, nhân kem mát lạnh và thơm sữa.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-39) },
                new Product { Name = "Khoai tây chiên", Price = 30000, CategoryId = cat("Đồ ăn vặt"), ImageUrl = "/images/products/khoai-tay-chien.svg", Description = "Khoai chiên nóng giòn, dùng kèm tương cà hoặc tương ớt.", IsBestSeller = true, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-40) },
                new Product { Name = "Gà viên", Price = 38000, CategoryId = cat("Đồ ăn vặt"), ImageUrl = "/images/products/ga-vien.svg", Description = "Gà viên chiên giòn, phần ăn nhẹ được khách nhỏ tuổi yêu thích.", IsBestSeller = true, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-41) },
                new Product { Name = "Xúc xích chiên", Price = 28000, CategoryId = cat("Đồ ăn vặt"), ImageUrl = "/images/products/xuc-xich-chien.svg", Description = "Xúc xích chiên nóng, ăn nhanh cùng nước uống lạnh.", IsBestSeller = false, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-42) },
                new Product { Name = "Bánh mì bơ tỏi", Price = 34000, CategoryId = cat("Đồ ăn vặt"), ImageUrl = "/images/products/banh-mi-bo-toi.svg", Description = "Bánh mì nướng giòn thơm bơ tỏi, hợp ăn kèm cà phê.", IsBestSeller = false, IsTrending = true, Status = "Available", CreatedAt = DateTime.Now.AddDays(-43) },
                new Product { Name = "Snack rong biển", Price = 25000, CategoryId = cat("Đồ ăn vặt"), ImageUrl = "/images/products/snack-rong-bien.svg", Description = "Snack rong biển giòn nhẹ, vị mặn thơm và tiện dùng.", IsBestSeller = false, IsTrending = false, Status = "Available", CreatedAt = DateTime.Now.AddDays(-44) }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Addresses.AnyAsync())
        {
            var members = await db.Users.Where(u => u.Role == "Member").OrderBy(u => u.Id).ToListAsync();
            var addresses = new[]
            {
                "12 Nguyễn Huệ, Quận 1, TP.HCM",
                "45 Lê Lợi, Quận 3, TP.HCM",
                "88 Trần Phú, Hà Đông, Hà Nội",
                "19 Võ Văn Tần, Quận 3, TP.HCM",
                "72 Nguyễn Văn Linh, Hải Châu, Đà Nẵng",
                "21 Hai Bà Trưng, Hoàn Kiếm, Hà Nội"
            };
            for (var i = 0; i < members.Count; i++)
            {
                db.Addresses.Add(new Address
                {
                    UserId = members[i].Id,
                    FullName = members[i].Username,
                    Phone = members[i].Phone ?? "0900000000",
                    AddressDetail = addresses[i % addresses.Length],
                    IsDefault = true,
                    CreatedAt = DateTime.Now.AddDays(-20 + i)
                });
            }
            await db.SaveChangesAsync();
        }

        if (!await db.Orders.AnyAsync())
        {
            var rng = new Random(20260425);
            var members = await db.Users.Where(u => u.Role == "Member").OrderBy(u => u.Id).ToListAsync();
            var products = await db.Products.OrderBy(p => p.Id).ToListAsync();
            var statuses = new[] { "Hoàn thành", "Hoàn thành", "Hoàn thành", "Hoàn thành", "Đang giao", "Đang chuẩn bị", "Chờ xử lý", "Đã hủy" };
            var sizes = new[] { "S", "M", "L" };
            var toppings = new[] { "", "Trân châu", "Kem cheese", "Thạch trái cây", "Shot espresso", "Sốt caramel" };
            var notes = new[] { "Ít đá", "Ít đường", "Giao giờ hành chính", "Để tại lễ tân", "Thêm ống hút", "" };
            var paymentMethods = new[] { "Thanh toán khi nhận hàng", "Chuyển khoản" };

            for (var i = 0; i < 96; i++)
            {
                var user = members[i % members.Count];
                var createdAt = i < 14
                    ? DateTime.Today.AddHours(8 + (i % 12)).AddMinutes((i * 7) % 60)
                    : DateTime.Today.AddDays(-rng.Next(1, 35)).AddHours(rng.Next(8, 22)).AddMinutes(rng.Next(0, 60));
                var status = i < 5 ? "Chờ xử lý" : statuses[rng.Next(statuses.Length)];

                var order = new Order
                {
                    UserId = user.Id,
                    FullName = user.Username,
                    Phone = user.Phone ?? "0900000000",
                    Address = i % 2 == 0 ? "12 Nguyễn Huệ, Quận 1, TP.HCM" : "45 Lê Lợi, Quận 3, TP.HCM",
                    Note = notes[rng.Next(notes.Length)],
                    PaymentMethod = paymentMethods[rng.Next(paymentMethods.Length)],
                    Status = status,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt.AddHours(rng.Next(1, 12))
                };

                var detailCount = rng.Next(2, 6);
                var selectedProducts = products.OrderBy(_ => rng.Next()).Take(detailCount).ToList();
                foreach (var product in selectedProducts)
                {
                    var qty = rng.Next(1, 4);
                    var size = sizes[rng.Next(sizes.Length)];
                    var topping = toppings[rng.Next(toppings.Length)];
                    var price = product.Price + (size == "L" ? 7000 : size == "S" ? -3000 : 0);
                    if (price < 10000) price = product.Price;
                    order.TotalPrice += price * qty;
                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = qty,
                        Price = price,
                        Size = size,
                        Topping = string.IsNullOrWhiteSpace(topping) ? null : topping,
                        CreatedAt = createdAt
                    });
                }

                db.Orders.Add(order);
            }
            await db.SaveChangesAsync();
        }

        if (!await db.WebsiteVisits.AnyAsync())
        {
            var rng = new Random(20260426);
            for (var d = 0; d < 35; d++)
            {
                var visitDate = DateTime.Today.AddDays(-d);
                var count = d == 0 ? 42 : rng.Next(9, 35);
                for (var i = 0; i < count; i++)
                {
                    db.WebsiteVisits.Add(new WebsiteVisit
                    {
                        IpAddress = $"192.168.1.{rng.Next(2, 240)}",
                        UserAgent = i % 3 == 0 ? "Mobile Safari" : i % 3 == 1 ? "Chrome Desktop" : "Edge Desktop",
                        VisitDate = visitDate,
                        CreatedAt = visitDate.AddHours(rng.Next(7, 23)).AddMinutes(rng.Next(0, 60))
                    });
                }
            }
            await db.SaveChangesAsync();
        }

        if (!await db.Blogs.AnyAsync())
        {
            db.Blogs.AddRange(
                new Blog { Title = "Bí quyết thưởng thức cà phê mỗi sáng", ImageUrl = "/images/products/americano.jpg", Content = "Một ly cà phê ngon không chỉ nằm ở hạt rang mà còn ở cách thưởng thức. Hãy dành vài phút để cảm nhận hương thơm, vị đắng nhẹ và hậu vị ngọt sâu.", CreatedAt = DateTime.Now.AddDays(-5) },
                new Blog { Title = "Menu xanh mát cho mùa hè", ImageUrl = "/images/products/coldbrewcamsanh.jpg", Content = "Các món soda, trà trái cây và nước ép tươi được phối màu xanh da trời - xanh lá, mang đến cảm giác tươi mát ngay từ giao diện đến hương vị.", CreatedAt = DateTime.Now.AddDays(-3) },
                new Blog { Title = "Combo khuyến mãi cuối tuần", ImageUrl = "/images/products/tiramisu.jpg", Content = "Cuối tuần này, cửa hàng áp dụng combo đồ uống kèm bánh ngọt với giá ưu đãi. Đặt hàng sớm để nhận thêm topping miễn phí.", CreatedAt = DateTime.Now.AddDays(-1) }
            );
            await db.SaveChangesAsync();
        }
    }
}
