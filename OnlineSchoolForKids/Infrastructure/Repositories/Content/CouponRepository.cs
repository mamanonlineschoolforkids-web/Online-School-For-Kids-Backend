using Domain.Entities.Content.Orders;
using Domain.Interfaces.Repositories.Content;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Content;

public class CouponRepository : GenericRepository<Coupon>, ICouponRepository
{
    public CouponRepository(MongoDbContext context)
        : base(context.Coupons)
    {
    
    }

    // ── GetByCodeAsync ────────────────────────────────────────────────────────

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        // Case-insensitive match — stored codes are uppercase by convention
        var filter = Builders<Coupon>.Filter.Regex(
            c => c.Code,
            new MongoDB.Bson.BsonRegularExpression($"^{code.Trim()}$", "i"));

        return await _collection
            .Find(filter)
            .FirstOrDefaultAsync(ct);
    }

    // ── GetUsageCountByUserAsync ──────────────────────────────────────────────

    public async Task<int> GetUsageCountByUserAsync(
        string couponId, string userId, CancellationToken ct = default)
    {
        // Requires a separate CouponUsage collection (see note below).
        // For now returns 0 — MaxUsesPerUser enforcement is opt-in.
        // Implement when you add a CouponUsage entity.
        await Task.CompletedTask;
        return 0;
    }

    // ── IncrementUsageAsync ───────────────────────────────────────────────────

    public async Task IncrementUsageAsync(string couponId, CancellationToken ct = default)
    {
        var filter = Builders<Coupon>.Filter.Eq(c => c.Id, couponId);
        var update = Builders<Coupon>.Update
            .Inc(c => c.UsedCount, 1)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    // ── GetActiveCouponsAsync ─────────────────────────────────────────────────

    public async Task<IEnumerable<Coupon>> GetActiveCouponsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var filter = Builders<Coupon>.Filter.And(
            Builders<Coupon>.Filter.Eq(c => c.IsActive, true),
            Builders<Coupon>.Filter.Or(
                Builders<Coupon>.Filter.Eq(c => c.ExpiresAt, null),
                Builders<Coupon>.Filter.Gt(c => c.ExpiresAt, now)
            )
        );

        return await _collection
            .Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }
}