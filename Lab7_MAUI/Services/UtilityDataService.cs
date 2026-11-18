using Microsoft.EntityFrameworkCore;
using Lab7_MAUI.Data;
using Lab7_MAUI.Models;

namespace Lab7_MAUI.Services;

public class UtilityDataService : IUtilityDataService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    private static bool _initialized = false;
    private static readonly SemaphoreSlim _initLock = new(1, 1);

    public UtilityDataService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.InitializeDatabaseAsync();
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task InitializeAsync()
    {
        await EnsureInitializedAsync();
    }

    #region Tenants

    public async Task<List<Tenant>> GetTenantsAsync(string? search = null)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Tenants.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(t =>
                t.LastName.ToLower().Contains(term) ||
                t.FirstName.ToLower().Contains(term) ||
                (t.MiddleName != null && t.MiddleName.ToLower().Contains(term)));
        }

        return await query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName).ToListAsync();
    }

    public async Task<Tenant?> GetTenantByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Tenants.FindAsync(id);
    }

    public async Task AddTenantAsync(Tenant tenant)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
    }

    public async Task UpdateTenantAsync(Tenant tenant)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Tenants.Update(tenant);
        await context.SaveChangesAsync();
    }

    public async Task DeleteTenantAsync(int id)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();

        // 1. Remove related tenant services
        var tenantServices = context.TenantServices.Where(ts => ts.TenantId == id);
        context.TenantServices.RemoveRange(tenantServices);

        // 2. Remove related apartments
        var apartments = context.Apartments.Where(a => a.TenantId == id);
        context.Apartments.RemoveRange(apartments);

        // 3. Remove tenant
        var tenant = await context.Tenants.FindAsync(id);
        if (tenant != null)
        {
            context.Tenants.Remove(tenant);
        }

        await context.SaveChangesAsync();
    }

    #endregion

    #region Apartments

    public async Task<List<Apartment>> GetApartmentsAsync(string? search = null)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var apartments = await context.Apartments.AsNoTracking().ToListAsync();
        var tenants = await context.Tenants.AsNoTracking().ToDictionaryAsync(t => t.TenantId, t => t.FullName);

        foreach (var apt in apartments)
        {
            if (tenants.TryGetValue(apt.TenantId, out var name))
            {
                apt.TenantName = name;
            }
            else
            {
                apt.TenantName = "Не закріплено";
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            apartments = apartments.Where(a =>
                a.Address.ToLower().Contains(term) ||
                a.AccountNumber.ToLower().Contains(term) ||
                a.TenantName.ToLower().Contains(term)).ToList();
        }

        return apartments.OrderBy(a => a.AccountNumber).ToList();
    }

    public async Task<Apartment?> GetApartmentByIdAsync(int id)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var apt = await context.Apartments.FindAsync(id);
        if (apt != null)
        {
            var tenant = await context.Tenants.FindAsync(apt.TenantId);
            apt.TenantName = tenant?.FullName ?? "Не закріплено";
        }
        return apt;
    }

    public async Task AddApartmentAsync(Apartment apartment)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Apartments.Add(apartment);
        await context.SaveChangesAsync();
    }

    public async Task UpdateApartmentAsync(Apartment apartment)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Apartments.Update(apartment);
        await context.SaveChangesAsync();
    }

    public async Task DeleteApartmentAsync(int id)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var apt = await context.Apartments.FindAsync(id);
        if (apt != null)
        {
            context.Apartments.Remove(apt);
            await context.SaveChangesAsync();
        }
    }

    #endregion

    #region Services

    public async Task<List<Service>> GetServicesAsync(string? search = null)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Services.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                s.ServiceName.ToLower().Contains(term) ||
                s.CalculationType.ToLower().Contains(term));
        }

        return await query.OrderBy(s => s.ServiceName).ToListAsync();
    }

    public async Task<Service?> GetServiceByIdAsync(int id)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Services.FindAsync(id);
    }

    public async Task AddServiceAsync(Service service)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Services.Add(service);
        await context.SaveChangesAsync();
    }

    public async Task UpdateServiceAsync(Service service)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Services.Update(service);
        await context.SaveChangesAsync();
    }

    public async Task DeleteServiceAsync(int id)
    {
        await EnsureInitializedAsync();
        using var context = await _contextFactory.CreateDbContextAsync();

        // Remove links first
        var links = context.TenantServices.Where(ts => ts.ServiceId == id);
        context.TenantServices.RemoveRange(links);

        var service = await context.Services.FindAsync(id);
        if (service != null)
        {
            context.Services.Remove(service);
        }

        await context.SaveChangesAsync();
    }

    #endregion

    #region Tenant Services Link

    public async Task<List<Service>> GetServicesForTenantAsync(int tenantId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var serviceIds = await context.TenantServices
            .Where(ts => ts.TenantId == tenantId)
            .Select(ts => ts.ServiceId)
            .ToListAsync();

        return await context.Services
            .Where(s => serviceIds.Contains(s.ServiceId))
            .ToListAsync();
    }

    public async Task<List<int>> GetSelectedServiceIdsForTenantAsync(int tenantId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TenantServices
            .Where(ts => ts.TenantId == tenantId)
            .Select(ts => ts.ServiceId)
            .ToListAsync();
    }

    public async Task SaveTenantServicesAsync(int tenantId, List<int> serviceIds)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var existing = context.TenantServices.Where(ts => ts.TenantId == tenantId);
        context.TenantServices.RemoveRange(existing);

        foreach (var sid in serviceIds)
        {
            context.TenantServices.Add(new TenantService { TenantId = tenantId, ServiceId = sid });
        }

        await context.SaveChangesAsync();
    }

    #endregion

    #region Bills

    public async Task<BillViewModel?> GetBillForApartmentAsync(int apartmentId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var apartment = await context.Apartments.FindAsync(apartmentId);
        if (apartment == null) return null;

        var tenant = await context.Tenants.FindAsync(apartment.TenantId);
        var tenantFullName = tenant != null ? tenant.FullName : "Не вказано";

        var serviceIds = await context.TenantServices
            .Where(ts => ts.TenantId == apartment.TenantId)
            .Select(ts => ts.ServiceId)
            .ToListAsync();

        var services = await context.Services
            .Where(s => serviceIds.Contains(s.ServiceId))
            .ToListAsync();

        var model = new BillViewModel
        {
            ApartmentId = apartment.ApartmentId,
            AccountNumber = apartment.AccountNumber,
            TenantFullName = tenantFullName,
            Address = apartment.Address,
            ResidentsCount = apartment.ResidentsCount,
            Area = apartment.Area,
            BillDate = DateTime.Now
        };

        foreach (var s in services)
        {
            decimal tariff = 0;
            string unit = "-";
            decimal volume = 0;
            decimal total = 0;

            if (s.PricePerSquareMeter.HasValue && s.PricePerSquareMeter.Value > 0)
            {
                tariff = s.PricePerSquareMeter.Value;
                unit = "кв. м";
                volume = apartment.Area;
                total = Math.Round(volume * tariff, 2);
            }
            else if (s.PricePerPerson.HasValue && s.PricePerPerson.Value > 0)
            {
                tariff = s.PricePerPerson.Value;
                unit = "осіб";
                volume = apartment.ResidentsCount;
                total = Math.Round(volume * tariff, 2);
            }

            model.Items.Add(new BillItemViewModel
            {
                ServiceName = s.ServiceName,
                CalculationType = s.CalculationType,
                Tariff = tariff,
                Unit = unit,
                ConsumptionVolume = volume,
                TotalAmount = total
            });
        }

        return model;
    }

    public async Task<BillViewModel?> GetBillByAccountNumberAsync(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber)) return null;

        using var context = await _contextFactory.CreateDbContextAsync();
        var apt = await context.Apartments
            .FirstOrDefaultAsync(a => a.AccountNumber.ToLower() == accountNumber.Trim().ToLower());

        if (apt == null) return null;

        return await GetBillForApartmentAsync(apt.ApartmentId);
    }

    public async Task<List<string>> GetAvailableAccountNumbersAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Apartments
            .OrderBy(a => a.AccountNumber)
            .Select(a => a.AccountNumber)
            .ToListAsync();
    }

    #endregion

    #region Stats

    public async Task<(int TotalTenants, int TotalApartments, int TotalServices, decimal TotalEstimatedMonthlyRevenue)> GetStatisticsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var tenantCount = await context.Tenants.CountAsync();
        var aptCount = await context.Apartments.CountAsync();
        var servCount = await context.Services.CountAsync();

        var apartments = await context.Apartments.ToListAsync();
        decimal totalRevenue = 0;

        foreach (var apt in apartments)
        {
            var bill = await GetBillForApartmentAsync(apt.ApartmentId);
            if (bill != null)
            {
                totalRevenue += bill.GrandTotal;
            }
        }

        return (tenantCount, aptCount, servCount, totalRevenue);
    }

    #endregion
}
