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
        // Temporarily add this line in InitAsync() right before CreateTableAsync:
        //await _dbConnection.DropTableAsync<Payment>(); // Or UserProfile / Tenant depending on model modified
        await _dbConnection.CreateTableAsync<Payment>();
        await _dbConnection.CreateTableAsync<UserProfile>();
    }

    // --- Property Operations ---
    public async Task<List<Property>> GetPropertiesAsync()
    {
        await InitAsync();
        return await _dbConnection!.Table<Property>().ToListAsync();
    }

    public async Task<int> SavePropertyAsync(Property property)
    {
        await InitAsync();
        return property.Id != 0
            ? await _dbConnection!.UpdateAsync(property)
            : await _dbConnection!.InsertAsync(property);
    }

    public async Task<int> DeletePropertyAsync(Property property)
    {
        await InitAsync();
        return await _dbConnection!.DeleteAsync(property);
    }

    // --- Unit Operations ---
    public async Task<List<Unit>> GetUnitsAsync()
    {
        await InitAsync();
        return await _dbConnection!.Table<Unit>().ToListAsync();
    }

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
        return unit.Id != 0
            ? await _dbConnection!.UpdateAsync(unit)
            : await _dbConnection!.InsertAsync(unit);
    }

    public async Task<int> DeleteUnitAsync(Unit unit)
    {
        await InitAsync();
        return await _dbConnection!.DeleteAsync(unit);
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

    public async Task<List<Tenant>> GetTenantsWithPaymentStatusAsync()
    {
        await InitAsync();
        var tenants = await _dbConnection!.Table<Tenant>().ToListAsync();

        var currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var currentMonthPayments = await _dbConnection!.Table<Payment>()
            .Where(p => p.PaymentDate >= currentMonthStart)
            .ToListAsync();

        var paidTenantIds = currentMonthPayments.Select(p => p.TenantId).ToHashSet();

        foreach (var tenant in tenants)
        {
            tenant.IsRentPaidThisMonth = paidTenantIds.Contains(tenant.Id);
        }

        return tenants;
    }

    // --- Payment Operations ---
    public async Task<List<Payment>> GetPaymentsAsync()
    {
        await InitAsync();
        return await _dbConnection!.Table<Payment>().OrderByDescending(p => p.PaymentDate).ToListAsync();
    }

    public async Task<List<Payment>> GetPaymentsByTenantAsync(int tenantId)
    {
        await InitAsync();
        return await _dbConnection!.Table<Payment>()
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<int> RecordPaymentAsync(Payment payment)
    {
        await InitAsync();
        return await _dbConnection!.InsertAsync(payment);
    }

    public async Task<int> DeletePaymentAsync(Payment payment)
    {
        await InitAsync();
        return await _dbConnection!.DeleteAsync(payment);
    }

    // --- Dashboard Analytics ---
    public async Task<int> GetTotalPropertiesCountAsync()
    {
        await InitAsync();
        return await _dbConnection!.Table<Property>().CountAsync();
    }

    public async Task<int> GetTotalUnitsCountAsync()
    {
        await InitAsync();
        return await _dbConnection!.Table<Unit>().CountAsync();
    }

    public async Task<int> GetOccupiedUnitsCountAsync()
    {
        await InitAsync();
        return await _dbConnection!.Table<Unit>().Where(u => u.Status == "Occupied").CountAsync();
    }

    public async Task<decimal> GetTotalMonthlyRevenueAsync()
    {
        await InitAsync();
        var occupiedUnits = await _dbConnection!.Table<Unit>().Where(u => u.Status == "Occupied").ToListAsync();
        return occupiedUnits.Sum(u => u.MonthlyRent);
    }
    public async Task<int> AddPaymentAsync(Payment payment)
    {
        await InitAsync();
        return await _dbConnection.InsertAsync(payment);
    }
    public async Task<List<UnitMonthlyStatus>> GetMonthlyUnitBreakdownAsync(int year, int month)
    {
        await InitAsync();

        var units = await _dbConnection.Table<Unit>().ToListAsync();
        var tenants = await _dbConnection.Table<Tenant>().ToListAsync();
        var payments = await _dbConnection.Table<Payment>().ToListAsync();

        var breakdown = new List<UnitMonthlyStatus>();

        foreach (var unit in units)
        {
            // Find tenant assigned to this unit
            var tenant = tenants.FirstOrDefault(t => t.UnitNumber == unit.UnitNumber);

            // Find payment made for this unit in the specified month & year
            var payment = payments.FirstOrDefault(p =>
                p.TenantId == tenant?.Id &&
                p.PaymentDate.Month == month &&
                p.PaymentDate.Year == year);

            bool isVacant = tenant == null || string.Equals(unit.Status, "Vacant", StringComparison.OrdinalIgnoreCase);

            breakdown.Add(new UnitMonthlyStatus
            {
                UnitId = unit.Id,
                UnitNumber = unit.UnitNumber,
                TargetRent = unit.MonthlyRent,
                MonthYearDisplay = new DateTime(year, month, 1).ToString("MMMM yyyy"),
                IsVacant = isVacant,
                TenantName = isVacant ? "No Tenant (Vacant)" : (tenant?.FullName ?? "Unknown"),
                AmountPaid = payment?.AmountPaid ?? 0m,
                PaymentMethod = payment?.PaymentMethod ?? "N/A",
                PaymentDate = payment?.PaymentDate
            });
        }

        return breakdown;
    }
    public async Task<UserProfile?> GetUserProfileAsync()
    {
        await InitAsync();
        return await _dbConnection.Table<UserProfile>().FirstOrDefaultAsync();
    }

    public async Task<int> SaveUserProfileAsync(UserProfile profile)
    {
        await InitAsync();
        if (profile.Id != 0)
            return await _dbConnection.UpdateAsync(profile);

        return await _dbConnection.InsertAsync(profile);
    }

    public async Task SavePaymentWithAdvanceAllocationAsync(Payment totalPayment)
    {
        await InitAsync();

        // 1. Fetch unit rent details
        var unit = await _dbConnection.Table<Unit>()
                                      .FirstOrDefaultAsync(u => u.Id == totalPayment.UnitId);

        // Default to full amount if unit rent isn't set
        decimal monthlyRent = (unit != null && unit.MonthlyRent > 0) ? unit.MonthlyRent : totalPayment.AmountPaid;

        decimal remainingAmount = totalPayment.AmountPaid;

        // Use selected target month/year, or fall back to payment date
        int startMonth = totalPayment.TargetMonth > 0 ? totalPayment.TargetMonth : totalPayment.PaymentDate.Month;
        int startYear = totalPayment.TargetYear > 0 ? totalPayment.TargetYear : totalPayment.PaymentDate.Year;

        DateTime targetPeriod = new DateTime(startYear, startMonth, 1);

        // 2. Loop and split payment across consecutive months
        while (remainingAmount > 0)
        {
            decimal amountForThisMonth = Math.Min(remainingAmount, monthlyRent);

            var splitPayment = new Payment
            {
                UnitId = totalPayment.UnitId,
                TenantId = totalPayment.TenantId,
                AmountPaid = amountForThisMonth,
                PaymentDate = totalPayment.PaymentDate,
                TargetMonth = targetPeriod.Month,
                TargetYear = targetPeriod.Year,
                PaymentMethod = totalPayment.PaymentMethod,
                ReferenceNumber = totalPayment.ReferenceNumber
            };

            await _dbConnection.InsertAsync(splitPayment);

            remainingAmount -= amountForThisMonth;
            targetPeriod = targetPeriod.AddMonths(1); // Advance to next month
        }
    }
}