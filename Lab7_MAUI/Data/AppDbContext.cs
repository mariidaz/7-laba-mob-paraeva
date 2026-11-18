using Microsoft.EntityFrameworkCore;
using Lab7_MAUI.Models;

namespace Lab7_MAUI.Data;

public class AppDbContext : DbContext
{
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Apartment> Apartments { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<TenantService> TenantServices { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TenantService>()
            .HasKey(ts => new { ts.TenantId, ts.ServiceId });
    }

    public async Task InitializeDatabaseAsync()
    {
        await Database.EnsureCreatedAsync();

        if (!await Tenants.AnyAsync())
        {
            var t1 = new Tenant { LastName = "Шевченко", FirstName = "Тарас", MiddleName = "Григорович" };
            var t2 = new Tenant { LastName = "Франко", FirstName = "Іван", MiddleName = "Якович" };
            var t3 = new Tenant { LastName = "Українка", FirstName = "Леся", MiddleName = "Петрівна" };
            var t4 = new Tenant { LastName = "Коцюбинський", FirstName = "Михайло", MiddleName = "Михайлович" };

            await Tenants.AddRangeAsync(t1, t2, t3, t4);
            await SaveChangesAsync();

            var s1 = new Service
            {
                ServiceName = "Утримання прибудинкової території",
                PricePerSquareMeter = 8.50m,
                PricePerPerson = null,
                CalculationType = "за площею"
            };
            var s2 = new Service
            {
                ServiceName = "Опалення централізоване",
                PricePerSquareMeter = 34.20m,
                PricePerPerson = null,
                CalculationType = "за площею"
            };
            var s3 = new Service
            {
                ServiceName = "Вивезення твердих побутових відходів",
                PricePerSquareMeter = null,
                PricePerPerson = 45.00m,
                CalculationType = "за кількістю мешканців"
            };
            var s4 = new Service
            {
                ServiceName = "Холодне водопостачання та водовідведення",
                PricePerSquareMeter = null,
                PricePerPerson = 120.00m,
                CalculationType = "за кількістю мешканців"
            };
            var s5 = new Service
            {
                ServiceName = "Технічне обслуговування ліфтів",
                PricePerSquareMeter = 3.80m,
                PricePerPerson = null,
                CalculationType = "за площею"
            };

            await Services.AddRangeAsync(s1, s2, s3, s4, s5);
            await SaveChangesAsync();

            var a1 = new Apartment
            {
                TenantId = t1.TenantId,
                Address = "вул. Хрещатик, 15, кв. 42",
                AccountNumber = "ACC-1001",
                ResidentsCount = 3,
                Area = 65.5m
            };
            var a2 = new Apartment
            {
                TenantId = t2.TenantId,
                Address = "вул. Володимирська, 60, кв. 12",
                AccountNumber = "ACC-1002",
                ResidentsCount = 2,
                Area = 48.0m
            };
            var a3 = new Apartment
            {
                TenantId = t3.TenantId,
                Address = "бульв. Тараса Шевченка, 24, кв. 7",
                AccountNumber = "ACC-1003",
                ResidentsCount = 4,
                Area = 82.4m
            };
            var a4 = new Apartment
            {
                TenantId = t4.TenantId,
                Address = "просп. Перемоги, 10, кв. 89",
                AccountNumber = "ACC-1004",
                ResidentsCount = 1,
                Area = 36.0m
            };

            await Apartments.AddRangeAsync(a1, a2, a3, a4);
            await SaveChangesAsync();

            // Link services to tenants
            var tsList = new List<TenantService>
            {
                new() { TenantId = t1.TenantId, ServiceId = s1.ServiceId },
                new() { TenantId = t1.TenantId, ServiceId = s2.ServiceId },
                new() { TenantId = t1.TenantId, ServiceId = s3.ServiceId },
                new() { TenantId = t1.TenantId, ServiceId = s4.ServiceId },
                new() { TenantId = t1.TenantId, ServiceId = s5.ServiceId },

                new() { TenantId = t2.TenantId, ServiceId = s1.ServiceId },
                new() { TenantId = t2.TenantId, ServiceId = s2.ServiceId },
                new() { TenantId = t2.TenantId, ServiceId = s3.ServiceId },
                new() { TenantId = t2.TenantId, ServiceId = s4.ServiceId },

                new() { TenantId = t3.TenantId, ServiceId = s1.ServiceId },
                new() { TenantId = t3.TenantId, ServiceId = s2.ServiceId },
                new() { TenantId = t3.TenantId, ServiceId = s3.ServiceId },
                new() { TenantId = t3.TenantId, ServiceId = s4.ServiceId },
                new() { TenantId = t3.TenantId, ServiceId = s5.ServiceId },

                new() { TenantId = t4.TenantId, ServiceId = s1.ServiceId },
                new() { TenantId = t4.TenantId, ServiceId = s3.ServiceId },
                new() { TenantId = t4.TenantId, ServiceId = s4.ServiceId }
            };

            await TenantServices.AddRangeAsync(tsList);
            await SaveChangesAsync();
        }
    }
}
