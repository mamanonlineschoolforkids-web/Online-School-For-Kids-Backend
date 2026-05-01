using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Content.Orders;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;           // e.g. "SAVE20"
    public string? Description { get; set; }                   // shown to user

    public string DiscountType { get; set; } = "percentage";
    public decimal Value { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }                   // null = never expires
    public DateTime? StartsAt { get; set; }                    // null = active immediately

    public int? MaxUses { get; set; }

    public int UsedCount { get; set; } = 0;

    public int? MaxUsesPerUser { get; set; }

    public List<string> ApplicableCourseIds { get; set; } = new();

    public string? CreatedByUserId { get; set; }               // admin who created it
}