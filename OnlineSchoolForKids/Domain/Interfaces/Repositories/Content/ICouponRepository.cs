
using Domain.Entities.Content.Orders;

namespace Domain.Interfaces.Repositories.Content;

public interface ICouponRepository : IGenericRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<int> GetUsageCountByUserAsync(string couponId, string userId, CancellationToken ct = default);
    Task IncrementUsageAsync(string couponId, CancellationToken ct = default);
    Task<IEnumerable<Coupon>> GetActiveCouponsAsync(CancellationToken ct = default);
}