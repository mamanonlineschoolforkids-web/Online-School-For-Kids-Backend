using Domain.Entities.Content.Orders;
using Domain.Interfaces.Repositories.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Shared.Payment;

public class CouponValidationResult
{
    public bool IsValid { get; init; }
    public string? Error { get; init; }
    public decimal DiscountAmount { get; init; }
    public Coupon? Coupon { get; init; }

    public static CouponValidationResult Valid(Coupon coupon, decimal discount) =>
        new() { IsValid = true, Coupon = coupon, DiscountAmount = discount };

    public static CouponValidationResult Invalid(string error) =>
        new() { IsValid = false, Error = error };
}

// ═══════════════════════════════════════════════════════════════════════════════
//  Interface
// ═══════════════════════════════════════════════════════════════════════════════

public interface ICouponValidationService
{
    Task<CouponValidationResult> ValidateAsync(
        string code,
        string userId,
        decimal subtotal,
        List<string> courseIds,
        CancellationToken ct = default);

    /// <summary>
    /// Call after a successful payment to increment the coupon's usage count.
    /// Accepts the coupon <paramref name="code"/> (what the caller has) and looks up the ID internally.
    /// </summary>
    Task RecordUsageAsync(string code, string userId, CancellationToken ct = default);
}

// ═══════════════════════════════════════════════════════════════════════════════
//  Implementation
// ═══════════════════════════════════════════════════════════════════════════════

public class CouponValidationService : ICouponValidationService
{
    private readonly ICouponRepository _couponRepository;

    public CouponValidationService(ICouponRepository couponRepository)
        => _couponRepository = couponRepository;

    public async Task<CouponValidationResult> ValidateAsync(
        string code,
        string userId,
        decimal subtotal,
        List<string> courseIds,
        CancellationToken ct = default)
    {
        // 1. Find coupon
        var coupon = await _couponRepository.GetByCodeAsync(code, ct);
        if (coupon is null)
            return CouponValidationResult.Invalid("Coupon code not found.");

        // 2. Active flag
        if (!coupon.IsActive)
            return CouponValidationResult.Invalid("This coupon is no longer active.");

        // 3. Date range
        var now = DateTime.UtcNow;
        if (coupon.StartsAt.HasValue && coupon.StartsAt > now)
            return CouponValidationResult.Invalid("This coupon is not valid yet.");
        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < now)
            return CouponValidationResult.Invalid("This coupon has expired.");

        // 4. Global usage limit
        if (coupon.MaxUses.HasValue && coupon.UsedCount >= coupon.MaxUses)
            return CouponValidationResult.Invalid("This coupon has reached its usage limit.");

        // 5. Per-user usage limit
        if (coupon.MaxUsesPerUser.HasValue)
        {
            var userUsage = await _couponRepository.GetUsageCountByUserAsync(coupon.Id, userId, ct);
            if (userUsage >= coupon.MaxUsesPerUser)
                return CouponValidationResult.Invalid("You have already used this coupon the maximum number of times.");
        }

        // 6. Minimum order amount
        if (coupon.MinOrderAmount.HasValue && subtotal < coupon.MinOrderAmount)
            return CouponValidationResult.Invalid(
                $"A minimum order of {coupon.MinOrderAmount:F2} EGP is required to use this coupon.");

        // 7. Course scope (optional — empty list = applies to everything)
        if (coupon.ApplicableCourseIds.Any())
        {
            var hasApplicableCourse = courseIds.Any(id => coupon.ApplicableCourseIds.Contains(id));
            if (!hasApplicableCourse)
                return CouponValidationResult.Invalid(
                    "This coupon does not apply to any course in your cart.");
        }

        // 8. Calculate discount
        decimal discount = coupon.DiscountType.Equals("percentage", StringComparison.OrdinalIgnoreCase)
            ? subtotal * (coupon.Value / 100m)
            : coupon.Value;

        if (coupon.MaxDiscountAmount.HasValue)
            discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);

        discount = Math.Min(discount, subtotal);   // can't discount more than the subtotal

        return CouponValidationResult.Valid(coupon, discount);
    }

    public async Task RecordUsageAsync(string code, string userId, CancellationToken ct = default)
    {
        var coupon = await _couponRepository.GetByCodeAsync(code, ct);
        if (coupon is null) return;

        await _couponRepository.IncrementUsageAsync(coupon.Id, ct);
        // When you add a CouponUsage entity, also insert a per-user usage record here.
    }
}