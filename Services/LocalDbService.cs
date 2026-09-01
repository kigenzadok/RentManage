using SQLite;
using RentManage.Models;

namespace RentManage.Services;

public class LocalDbService
{
    private SQLiteAsyncConnection? _dbConnection;

    private async Task InitAsync()
    {
        if (_dbConnection != null)
            return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "rentmanage_v2.db3");
        _dbConnection = new SQLiteAsyncConnection(dbPath);

        await _dbConnection.CreateTableAsync<Property>();
        await _dbConnection.CreateTableAsync<Unit>();
        await _dbConnection.CreateTableAsync<Tenant>();
    }

    // --- Tenant Operations ---
    public async Task<List<Tenant>> GetTenantsAsync()
    {
        await InitAsync();
        return await _dbConnection!.Table<Tenant>().ToListAsync();
    }

    public async Task<List<Tenant>> SearchTenantsAsync(string query)
    {
        await InitAsync();

        if (string.IsNullOrWhiteSpace(query))
            return await GetTenantsAsync();

        return await _dbConnection!.Table<Tenant>()
            .Where(t => t.FullName.ToLower().Contains(query.ToLower()) ||
                        t.UnitNumber.ToLower().Contains(query.ToLower()))
            .ToListAsync();
    }

    public async Task<int> SaveTenantAsync(Tenant tenant)
    {
        await InitAsync();
        return tenant.Id != 0
            ? await _dbConnection!.UpdateAsync(tenant)
            : await _dbConnection!.InsertAsync(tenant);
    }

    public async Task<int> DeleteTenantAsync(Tenant tenant)
    {
        await InitAsync();
        return await _dbConnection!.DeleteAsync(tenant);
    }

    // --- Property CRUD ---
    public async Task<List<Property>> GetPropertiesAsync()
    {
        await InitAsync();
        return await _dbConnection!.Table<Property>().ToListAsync();
    }

    public async Task<int> SavePropertyAsync(Property property)
    {
        await InitAsync();
        return property.Id != 0 ? await _dbConnection!.UpdateAsync(property) : await _dbConnection!.InsertAsync(property);
    }

    // --- Unit CRUD ---
    public async Task<List<Unit>> GetUnitsByPropertyAsync(int propertyId)
    {
        await InitAsync();
        return await _dbConnection!.Table<Unit>().Where(u => u.PropertyId == propertyId).ToListAsync();
    }

    public async Task<List<Unit>> GetVacantUnitsAsync()
    {
        await InitAsync();
        return await _dbConnection!.Table<Unit>().Where(u => u.Status == "Vacant").ToListAsync();
    }

    public async Task<int> SaveUnitAsync(Unit unit)
    {
        await InitAsync();
        return unit.Id != 0 ? await _dbConnection!.UpdateAsync(unit) : await _dbConnection!.InsertAsync(unit);
    }

    public async Task<int> RegisterTenantAsync(Tenant tenant, int unitId)
    {
        await InitAsync();

        tenant.UnitId = unitId;
        int result = await SaveTenantAsync(tenant);

        var unit = await _dbConnection!.Table<Unit>().FirstOrDefaultAsync(u => u.Id == unitId);
        if (unit != null)
        {
            unit.Status = "Occupied";
            await SaveUnitAsync(unit);
        }

        return result;
    }
}