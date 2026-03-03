namespace Application.Commands.Order
{
    using Domain.Entities.Content;
    using Domain.Entities.Content.Order;
    using Domain.Entities.Users;
    using Domain.Enums.Content;
    using Domain.Enums.Users;
    using Domain.Interfaces.Repositories;
    using Domain.Interfaces.Repositories.Content;
    using Domain.Interfaces.Services;
    using MediatR;
    using Microsoft.Extensions.Logging;

    namespace Application.Commands.Checkout
    {
        public class CheckoutCommand : IRequest<CheckoutResponse>
        {
            public string UserId { get; set; } = string.Empty;
            public string? Notes { get; set; }
        }

        public class CheckoutResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string? OrderId { get; set; }
            public string? OrderNumber { get; set; }
            public decimal Total { get; set; }
            public string? PaymentIntentId { get; set; }
            public string? ClientSecret { get; set; }
        }

        public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResponse>
        {
            private readonly ICartItemRepository _cartRepository;
            private readonly IOrderRepository _orderRepository;
            private readonly IGenericRepository<Course> _courseRepository;
            private readonly IGenericRepository<Enrollment> _enrollmentRepository;
            private readonly IPaymentService _paymentService;
            private readonly IGenericRepository<User> _userRepository;
            private readonly ILogger<CheckoutCommandHandler> _logger;

            public CheckoutCommandHandler(
                ICartItemRepository cartRepository,
                IOrderRepository orderRepository,
                IGenericRepository<Course> courseRepository,
                IGenericRepository<Enrollment> enrollmentRepository,
                IPaymentService paymentService,
                IGenericRepository<User> userRepository,
                ILogger<CheckoutCommandHandler> logger)
            {
                _cartRepository = cartRepository;
                _orderRepository = orderRepository;
                _courseRepository = courseRepository;
                _enrollmentRepository = enrollmentRepository;
                _paymentService = paymentService;
                _userRepository = userRepository;
                _logger = logger;
            }

            public async Task<CheckoutResponse> Handle(
                CheckoutCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    //  STEP 1: Get cart items from Redis
                    var cartItems = await _cartRepository.GetUserCartItemsAsync(
                        request.UserId, cancellationToken);

                    if (!cartItems.Any())
                        return Fail("Your cart is empty");

                    //  STEP 2: Get latest course prices from MongoDB
                    var courseIds = cartItems.Select(c => c.CourseId).ToList();
                    var courses = await _courseRepository.GetAllAsync(
                        filter: c => courseIds.Contains(c.Id) && c.IsPublished,
                        cancellationToken);

                    var courseDict = courses.ToDictionary(c => c.Id);

                    //  STEP 3: Build order items (validate + calculate)
                    var orderItems = new List<OrderItem>();
                    decimal subtotal = 0;

                    foreach (var cartItem in cartItems)
                    {
                        if (!courseDict.TryGetValue(cartItem.CourseId, out var course))
                            continue;

                        // Skip already enrolled
                        var alreadyEnrolled = await _enrollmentRepository.ExistsAsync(
                            e => e.UserId == request.UserId && e.CourseId == cartItem.CourseId,
                            cancellationToken);

                        if (alreadyEnrolled)
                            continue;

                        var price = course.DiscountPrice ?? course.Price;

                        orderItems.Add(new OrderItem
                        {
                            Id = Guid.NewGuid().ToString(),
                            CourseId = course.Id,
                            CourseTitle = course.Title,
                            CourseThumbnail = course.ThumbnailUrl,
                            InstructorName = await GetInstructorNameAsync(course),
                            Price = price,
                            OriginalPrice = course.DiscountPrice.HasValue ? course.Price : null,
                            DiscountPercentage = course.DiscountPrice.HasValue
                                ? (int)Math.Round((1 - course.DiscountPrice.Value / course.Price) * 100)
                                : 0
                        });

                        subtotal += price;
                    }

                    if (!orderItems.Any())
                        return Fail("No valid items to checkout. You may already be enrolled.");

                    //  STEP 4: Create or Update Payment Intent via PaymentService
                    var paymentIntentResult = await _paymentService.CreateOrUpdatePaymentIntentAsync(
                        request.UserId, cancellationToken);

                    if (paymentIntentResult == null)
                        return Fail("Failed to create payment intent. Please try again.");

                    //  STEP 5: Create Order in MongoDB
                    var order = new Order
                    {
                        UserId = request.UserId,
                        Status = OrderStatus.Pending,
                        PaymentStatus = PaymentStatus.Pending,
                        PaymentMethod = Domain.Enums.Content.PaymentMethod.Stripe,
                        Subtotal = subtotal,
                        Tax = 0,
                        Total = subtotal,
                        Items = orderItems,
                        Notes = request.Notes,
                        PaymentIntentId = paymentIntentResult.PaymentIntentId
                    };

                    var createdOrder = await _orderRepository.CreateAsync(order, cancellationToken);

                    //  STEP 6: Clear cart from Redis
                    await _cartRepository.ClearUserCartAsync(request.UserId, cancellationToken);

                    _logger.LogInformation(
                        "Checkout successful: Order {OrderNumber}, PaymentIntent {PaymentIntentId}",
                        createdOrder.OrderNumber, paymentIntentResult.PaymentIntentId);

                    return new CheckoutResponse
                    {
                        Success = true,
                        Message = "Checkout successful! Please complete payment.",
                        OrderId = createdOrder.Id,
                        OrderNumber = createdOrder.OrderNumber,
                        Total = subtotal,
                        PaymentIntentId = paymentIntentResult.PaymentIntentId,
                        ClientSecret = paymentIntentResult.ClientSecret
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during checkout for user {UserId}", request.UserId);
                    return Fail("An error occurred during checkout. Please try again.");
                }
            }

            private static CheckoutResponse Fail(string message) =>
                new() { Success = false, Message = message };

            public async Task<string> GetInstructorNameAsync(Course course)
            {
                if (string.IsNullOrEmpty(course.InstructorId)) return "Unknown";
                var instructorUser = await _userRepository.GetByIdAsync(course.InstructorId);
                if (instructorUser != null && instructorUser.Role == UserRole.ContentCreator)
                {
                    return $"{instructorUser.FullName}".Trim();
                }
                return "Unknown";
            }
        }
    }

}
