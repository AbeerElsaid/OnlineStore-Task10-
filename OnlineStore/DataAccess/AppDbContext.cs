using Microsoft.EntityFrameworkCore;
using OnlineStore.Enums;
using OnlineStore.Models;

namespace OnlineStore.DataAccess;

public class AppDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlServer("Data Source=.;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Initial Catalog=OnlineStore");
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Review> Reviews { get; set; }


    public async Task<(List<Product> Items, int TotalCount)> SearchProducts(ProductFilter f)
    {
        IQueryable<Product> products = Products;
        if (!string.IsNullOrWhiteSpace(f.Text))
            products = products.Where(p => p.Name.Contains(f.Text));

        if (!string.IsNullOrWhiteSpace(f.Category))
            products = products.Where(p => p.Category!.Name == f.Category);

        if (f.MinPrice.HasValue)
            products = products.Where(p => p.Price >= f.MinPrice.Value);

        if (f.MaxPrice.HasValue)
            products = products.Where(p => p.Price <= f.MaxPrice.Value);

        if (f.InStockOnly)
            products = products.Where(p => p.Stock > 0);

        products = f.SortBy.ToLower() switch
        {
            "price" => products.OrderBy(p => p.Price),
            "rating" => products.OrderByDescending(p => p.Reviews.Average(r => (double?)r.Rating) ?? 0),
            _ => products.OrderBy(p => p.Name)
        };
        int totalCount = await products.CountAsync();
        int page = Math.Max(1, f.Page);
        int pageSize = Math.Max(1, f.PageSize);

        var items = await products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }


    public async Task<int> IncreaseAccessoriesPrices()
    {
        return await Products.Where(p => p.Category!.Name == "Accessories" && p.Stock < 5)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Price, p => p.Price * 1.10m));
    }

    public async Task<int> DeleteOldCancelledOrders()
    {
        var oneYearAgo = DateTime.Now.AddYears(-1);
        return await Orders.Where(o => o.Status == OrderStatus.Cancelled && o.OrderDate < oneYearAgo).ExecuteDeleteAsync();
    }
}
        