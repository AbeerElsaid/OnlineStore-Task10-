using Microsoft.EntityFrameworkCore;
using OnlineStore.DataAccess;
using OnlineStore.Enums;
using System.Runtime.CompilerServices;

namespace OnlineStore;

internal class Program
{
    static void Main(string[] args)
    {
        using AppDbContext _db = new AppDbContext();

        var deliveredOrders = _db.Orders.AsQueryable()
        .Where(o => o.Status == OrderStatus.Delivered)
        .Select(o => new
        {
            OrderId = o.Id,
            CustomerName = o.Customer!.FullName,
            Total = o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount))
        })
        .OrderByDescending(o => o.Total);

        //foreach (var order in deliveredOrders)
        //{
        //    Console.WriteLine($"OrderId: {order.OrderId}, Customer: {order.CustomerName}, Total: {order.Total:F2}");
        //}

        var totalOrders = _db.Orders.Count();

        var ordersStatisticsByStatus = _db.Orders.GroupBy(o => o.Status)
            .Select(s => new
            {
                Status = s.Key,
                Count = s.Count(),
                Percentage = totalOrders == 0 ? 0 : (double)s.Count() / totalOrders * 100
            }).AsQueryable();

        //foreach (var S in ordersStatisticsByStatus)
        //{
        //    Console.WriteLine($"Status: {S.Status+1}, Count: {S.Count}, Percentage: {S.Percentage:F2}%");
        //}

        var productsAboveCategoryAverage = _db.Products.AsQueryable()
            .Where(p => p.Price > _db.Products.Where(CP => CP.CategoryId == p.CategoryId)
            .Average(CP => CP.Price)).Select(p => new
            {
                ProductName = p.Name,
                CategoryName = p.Category!.Name,
                Price = p.Price
            });

        //foreach (var product in productsAboveCategoryAverage)
        //{
        //    Console.WriteLine($"Product: {product.ProductName}, Category: {product.CategoryName}, Price: {product.Price:F2}");
        //}

        var neverOrderedProducts = _db.Products.AsQueryable()
           .Where(p => !p.OrderItems.Any())
           .Select(p => new
           {
               ProductName = p.Name,
               Price = p.Price,
               Stock = p.Stock
           });

        //foreach (var product in neverOrderedProducts)
        //{
        //    Console.WriteLine($"Product: {product.ProductName}, Price: {product.Price:F2}, Stock: {product.Stock}");
        //}


        var recentCustomers = _db.Customers.AsQueryable()
           .Where(c => c.RegisteredAt >= DateTime.Now.AddDays(-30) && c.Orders.Any())
           .Select(c => new
           {
               CustomerName = c.FullName,
               Email = c.Email,
               RegisteredAt = c.RegisteredAt
           });

        //foreach (var customer in recentCustomers)
        //{
        //    Console.WriteLine($"Customer: {customer.CustomerName}, Email: {customer.Email}, Registered At: {customer.RegisteredAt}");
        //}


        var customersWithManyCancelledOrders = _db.Customers.AsQueryable()
            .Where(c => c.Orders.Count(o => o.Status == OrderStatus.Cancelled) > 2)
            .Select(c => new
            {
                CustomerName = c.FullName,
                Email = c.Email,
                CancelledOrdersCount = c.Orders.Count(o => o.Status == OrderStatus.Cancelled)
            });

        //foreach (var customer in customersWithManyCancelledOrders)
        //{
        //    Console.WriteLine($"Customer: {customer.CustomerName}, Email: {customer.Email}, Cancelled Orders: {customer.CancelledOrdersCount}");
        //}

        var topCustomersBySpending = _db.Customers.AsQueryable()
            .Select(c => new
            {
                CustomerName = c.FullName,
                OrderCount = c.Orders.Count(o => o.Status == OrderStatus.Delivered),
                TotalSpent = c.Orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount))),
                AverageOrderValue = c.Orders.Count(o => o.Status == OrderStatus.Delivered) == 0 ? 0 : c.Orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount))) / c.Orders.Count(o => o.Status == OrderStatus.Delivered)
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(5);

        //foreach (var customer in topCustomersBySpending)
        //{ 
        //    Console.WriteLine($"Customer: {customer.CustomerName}, Orders: {customer.OrderCount}, Total Spent: {customer.TotalSpent:F2}, Average Order Value: {customer.AverageOrderValue:F2}");
        //}

        var monthlyReport = _db.Orders.AsQueryable()
            .Where(O => O.OrderDate.Year == 2025 && O.Status == OrderStatus.Delivered)
            .GroupBy(o => new
            {
                Year = o.OrderDate.Year,
                Month = o.OrderDate.Month
            })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                OrderCount = g.Count(),
                Revenue = g.Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount)))
            }).OrderBy(r => r.Month);

        //foreach (var report in monthlyReport)
        //{
        //    Console.WriteLine($"Year: {report.Year}, Month: {report.Month}, Orders: {report.OrderCount}, Revenue: {report.Revenue:F2}");
        //}

        var revenuePerCategory = _db.Categories.AsQueryable()
            .Select(O => new
            {
                CategoryName = O.Name,
                Revenue = O.Products
                   .SelectMany(p => p.OrderItems)
                   .Where(oi => oi.Order!.Status == OrderStatus.Delivered)
                   .Sum(oi => oi.Quantity * oi.UnitPrice * (1 - oi.Discount))
            }).OrderByDescending(o => o.Revenue);


        //foreach(var report in revenuePerCategory)
        //{
        //    Console.WriteLine($"Category: {report.CategoryName}, Revenue: {report.Revenue:F2}");
        //}


        var customersWithoutOrders = _db.Customers.AsQueryable()
            .Where(c => !c.Orders.Any())
            .GroupBy(c => c.City)
            .Select(g => new
            {
                City = g.Key,
                CustomerCount = g.Count()
            })
            .OrderByDescending(g => g.CustomerCount);

        //foreach (var city in customersWithoutOrders)
        //{
        //    Console.WriteLine($"City: {city.City}, Customers Without Orders: {city.CustomerCount}");
        //}

        var highlyRatedProducts = _db.Products.AsQueryable()
            .Where(p => p.Reviews.Count() >= 10 && p.Reviews.Average(r => r.Rating) >= 4.5)
            .Select(p => new
            {
                ProductName = p.Name,
                ProductCategory = p.Category!.Name,
                AverageRating = p.Reviews.Average(r => r.Rating),
                ReviewCount = p.Reviews.Count()
            });

        //foreach (var product in highlyRatedProducts)
        //{
        //    Console.WriteLine($"Product: {product.ProductName}, Category: {product.ProductCategory}, Average Rating: {product.AverageRating:F2}, Review Count: {product.ReviewCount}");
        //}

        var topProductsPerCategory = _db.Categories.AsQueryable()
            .SelectMany(c => c.Products.OrderByDescending(p => p.Price)
            .Take(2)
            .Select(p => new
            {
                ProductName = p.Name,
                CategoryName = p.Category!.Name,
                Price = p.Price
            })
        );

        //foreach (var product in topProductsPerCategory)
        //{
        //    Console.WriteLine($"Product: {product.ProductName}, Category: {product.CategoryName}, Price: {product.Price:F2}");
        //}

        var lowStockProducts = _db.Products.AsQueryable()
            .Where(P => P.Stock < (P.OrderItems.Where(oi => oi.Order!.Status == OrderStatus.Pending).Sum(oi => oi.Quantity)))
            .Select(p => new
            {
                ProductName = p.Name,
                CategoryName = p.Category!.Name,
                Stock = p.Stock,
                PendingOrdersQuantity = p.OrderItems.Where(oi => oi.Order!.Status == OrderStatus.Pending).Sum(oi => oi.Quantity)
            });

        //foreach (var product in lowStockProducts)
        //{
        //    Console.WriteLine($"Product: {product.ProductName}, Category: {product.CategoryName}, Stock: {product.Stock}, Pending Orders Quantity: {product.PendingOrdersQuantity}");
        //}


        var averageSpending = _db.Customers.AsQueryable()
            .Select(c => c.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount))))
            .Average();

        var customersAboveAverage = _db.Customers
            .Where(c => c.Orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount))) > averageSpending)
            .Select(c => new
            {
                CustomerName = c.FullName,
                TotalSpent = c.Orders
                    .Where(o => o.Status == OrderStatus.Delivered)
                    .Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount)))
            });

        //foreach (var customer in customersAboveAverage)
        //{
        //    Console.WriteLine($"Customer: {customer.CustomerName}, Total Spent: {customer.TotalSpent:F2}");
        //}


        var bestSellingProducts = _db.Categories.AsQueryable()
            .Select(c => new
            {
                CategoryName = c.Name,
                BestSellingProduct = c.Products
                   .Select(p => new
                   {
                       ProductName = p.Name,
                       QuantitySold = p.OrderItems.Where(oi => oi.Order!.Status == OrderStatus.Delivered).Sum(oi => oi.Quantity)
                   })
            .OrderByDescending(p => p.QuantitySold)
            .FirstOrDefault()
            });

        //foreach (var category in bestSellingProducts)
        //{
        //    Console.WriteLine($"Category: {category.CategoryName}, Product: {category.BestSellingProduct?.ProductName ?? "No products"}, Quantity Sold: {category.BestSellingProduct?.QuantitySold ?? 0}");
        //}

        var laptopProducts = _db.Products.AsQueryable()
            .Where(p => p.Category!.Name == "Laptops")
            .Select(p => p.Id);

        var customersWhoBoughtAllLaptops = _db.Customers.AsQueryable()
            .Where(c => laptopProducts.All(lp => c.Orders.Where(o => o.Status == OrderStatus.Delivered)
                .SelectMany(o => o.Items)
                .Any(oi => oi.ProductId == lp)))
            .Select(c => c.FullName);

        //foreach(var customer in customersWhoBoughtAllLaptops)
        //{
        //    Console.WriteLine($"Customer: {customer}");
        //}

        var reviewsForUnpurchasedProducts = _db.Reviews.AsQueryable()
            .Where(r => !r.Customer!.Orders.Where(o => o.Status == OrderStatus.Delivered)
                .SelectMany(o => o.Items)
                .Any(oi => oi.ProductId == r.ProductId))
            .Select(r => new
            {
                ProductName = r.Product!.Name,
                CustomerName = r.Customer.FullName,
                Rating = r.Rating
            });

        //foreach(var review in reviewsForUnpurchasedProducts)
        //{
        //    Console.WriteLine($"Product: {review.ProductName}, Customer: {review.CustomerName}, Rating: {review.Rating}");
        //}

        var customersWithOrderCount = _db.Customers.AsQueryable()
            .GroupJoin(
                _db.Orders.AsQueryable(),
                c => c.Id,
                o => o.CustomerId,
                (c, o) => new
                {
                    CustomerName = c.FullName,
                    OrderCount = o.Count()
                });

        // More Easier
        //var customersWithOrderCount = _db.Customers.AsQueryable().
        //    Select(c => new
        //    {
        //        CustomerName = c.FullName,
        //        OrderCount = c.Orders.Count()
        //    });


        //foreach (var customer in customersWithOrderCount)
        //{
        //    Console.WriteLine($"Customer: {customer.CustomerName}, Order Count: {customer.OrderCount}");
        //}


        var productPairs = _db.Orders.AsQueryable()
            .SelectMany(o => o.Items
                .SelectMany(i1 => o.Items
                    .Where(i2 => i1.ProductId < i2.ProductId)
                    .Select(i2 => new
                    {
                        Product1Id = i1.ProductId,
                        Product2Id = i2.ProductId
                    })))
            .GroupBy(pair => new
            {
                pair.Product1Id,
                pair.Product2Id
            })
            .Select(g => new
            {
                Product1 = _db.Products
                    .Where(p => p.Id == g.Key.Product1Id)
                    .Select(p => p.Name)
                    .FirstOrDefault(),

                Product2 = _db.Products
                    .Where(p => p.Id == g.Key.Product2Id)
                    .Select(p => p.Name)
                    .FirstOrDefault(),

                TogetherCount = g.Count()
            })
            .OrderByDescending(pair => pair.TogetherCount)
            .Take(5);

        //foreach (var pair in productPairs)
        //{
        //    Console.WriteLine($"{pair.Product1} + {pair.Product2}: {pair.TogetherCount} times");
        //}

        var customersLatestOrders = _db.Customers.AsQueryable()
            .Select(c => new
            {
                CustomerName = c.FullName,
                LatestOrderDate = c.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new
                    {
                        OrderDate = o.OrderDate,
                        OrderItems = o.Items.Select(i => new
                        {
                            ProductName = i.Product!.Name,
                            Quantity = i.Quantity
                        })
                    })
                    .FirstOrDefault()
            });

        //foreach (var customer in customersLatestOrders)
        //{
        //    Console.WriteLine($"Customer: {customer.CustomerName}, Latest Order Date: {customer.LatestOrderDate?.OrderDate}");
        //    if (customer.LatestOrderDate != null)
        //    {
        //        foreach (var item in customer.LatestOrderDate.OrderItems)
        //        {
        //            Console.WriteLine($"  Product: {item.ProductName}, Quantity: {item.Quantity}");
        //        }
        //    }
        //}

        var customersFavoriteCategories = _db.Customers.AsQueryable()
            .Select(c => new
            {
                CustomerName = c.FullName,
                FavoriteCategory = c.Orders
                    .Where(o => o.Status == OrderStatus.Delivered)
                    .SelectMany(o => o.Items)
                    .GroupBy(i => i.Product!.Category!.Name)
                    .OrderByDescending(g => g.Sum(i => i.Quantity))
                    .Select(g => new
                    {
                        CategoryName = g.Key,
                        TotalQuantity = g.Sum(i => i.Quantity)
                    })
                    .FirstOrDefault()
            });

        //foreach (var customer in customersFavoriteCategories)
        //{
        //    Console.WriteLine($"Customer: {customer.CustomerName}, Favorite Category: {customer.FavoriteCategory?.CategoryName ?? "No orders"}");
        //    if (customer.FavoriteCategory != null)
        //    {
        //        Console.WriteLine($"  Total Quantity: {customer.FavoriteCategory.TotalQuantity}");
        //    }
        //}


        var productsRevenue = _db.Products.AsQueryable()
            .Select(p => new
            {
                ProductName = p.Name,
                Revenue = p.OrderItems
                    .Where(oi => oi.Order!.Status == OrderStatus.Delivered)
                    .Sum(oi => oi.Quantity * oi.UnitPrice * (1 - oi.Discount))
            })
            .OrderByDescending(p => p.Revenue);

        //int productRank = 1;
        //foreach (var product in productsRevenue)
        //{
        //    Console.WriteLine(
        //        $"Rank: {productRank}, Product: {product.ProductName}, Revenue: {product.Revenue}");
        //    productRank++;
        //}

        // Bonus • (a)Print the SQL generated for question 8 using ToQueryString().
        Console.WriteLine(monthlyReport.ToQueryString());

        // Bonus • (b) Explain the N+1 problem and show where it would appear if question 20 were written with a foreach loop.
        // N + 1 Query Problem(Lazy Loading): بتحصل لما نعمل Query واحدة تجيب الـ Main Data،
        // وبعدها Query إضافية لكل عنصر داخل الـ foreach،
        // وده بيزود عدد الـ Queries وبيبطّأ الأداء.

        // Bonus • c) Why can't Select((x, i) => ...) (with an index) be translated to SQL in question 22, and what is the fix ?
        // EF Core مش بيقدر يترجم الـ index داخل Select إلى SQL،
        // لذلك استخدمنا foreach وعدّاد لحساب ترتيب كل منتج.
    }
}
