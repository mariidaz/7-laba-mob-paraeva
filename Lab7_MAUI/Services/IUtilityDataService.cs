using Lab7_MAUI.Models;

namespace Lab7_MAUI.Services;

public interface IUtilityDataService
{
    Task InitializeAsync();

    // Tenants
    Task<List<Tenant>> GetTenantsAsync(string? search = null);
    Task<Tenant?> GetTenantByIdAsync(int id);
    Task AddTenantAsync(Tenant tenant);
    Task UpdateTenantAsync(Tenant tenant);
    Task DeleteTenantAsync(int id);

    // Apartments
    Task<List<Apartment>> GetApartmentsAsync(string? search = null);
    Task<Apartment?> GetApartmentByIdAsync(int id);
    Task AddApartmentAsync(Apartment apartment);
    Task UpdateApartmentAsync(Apartment apartment);
    Task DeleteApartmentAsync(int id);

    // Services
    Task<List<Service>> GetServicesAsync(string? search = null);
    Task<Service?> GetServiceByIdAsync(int id);
    Task AddServiceAsync(Service service);
    Task UpdateServiceAsync(Service service);
    Task DeleteServiceAsync(int id);

    // Tenant Services Link
    Task<List<Service>> GetServicesForTenantAsync(int tenantId);
    Task<List<int>> GetSelectedServiceIdsForTenantAsync(int tenantId);
    Task SaveTenantServicesAsync(int tenantId, List<int> serviceIds);

    // Bills
    Task<BillViewModel?> GetBillForApartmentAsync(int apartmentId);
    Task<BillViewModel?> GetBillByAccountNumberAsync(string accountNumber);
    Task<List<string>> GetAvailableAccountNumbersAsync();

    // Stats
    Task<(int TotalTenants, int TotalApartments, int TotalServices, decimal TotalEstimatedMonthlyRevenue)> GetStatisticsAsync();
}
