namespace Domain.Entities.Content.Orders;

public class OrderItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string CourseId { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseThumbnail { get; set; }
    public string? InstructorName { get; set; }
    public string? InstructorId { get; set; }

    public decimal Price { get; set; }            // actual price paid
    public decimal? OriginalPrice { get; set; }   // before discount (null if no discount)
    public int DiscountPercentage { get; set; } = 0;
}