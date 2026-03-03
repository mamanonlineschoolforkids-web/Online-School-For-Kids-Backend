using Domain.Entities;
using Domain.Entities.Content;
using Domain.Entities.Content.Order;
using Domain.Enums.Content;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Repositories.Content;
using Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;


namespace Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ICartItemRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IGenericRepository<Domain.Entities.Content.Course> _courseRepository;
        private readonly IGenericRepository<Payment> _paymentRepository;
        private readonly IGenericRepository<Enrollment> _enrollmentRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IConfiguration configuration,
            ICartItemRepository cartRepository,
            IOrderRepository orderRepository,
            IGenericRepository<Domain.Entities.Content.Course> courseRepository,
            IGenericRepository<Payment> paymentRepository,
            IGenericRepository<Enrollment> enrollmentRepository,
            ILogger<PaymentService> logger)
        {
            _configuration = configuration;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _courseRepository = courseRepository;
            _paymentRepository = paymentRepository;
            _enrollmentRepository = enrollmentRepository;
            _logger = logger;

            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
        }

        public async Task<PaymentIntentResult?> CreateOrUpdatePaymentIntentAsync(
         string userId,
         CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Get Cart and Validate
                var cartItems = await _cartRepository.GetUserCartItemsAsync(userId, cancellationToken);
                if (cartItems == null || !cartItems.Any()) return null;

                var courseIds = cartItems.Select(c => c.CourseId).ToList();
                var courses = await _courseRepository.GetAllAsync(
                    filter: c => courseIds.Contains(c.Id) && c.IsPublished,
                    cancellationToken);

                var courseDict = courses.ToDictionary(c => c.Id);
                decimal totalAmount = cartItems
                    .Where(ci => courseDict.ContainsKey(ci.CourseId))
                    .Sum(ci => courseDict[ci.CourseId].DiscountPrice ?? courseDict[ci.CourseId].Price);

                if (totalAmount <= 0) return null;

                // 2. CHECK FOR EXISTING PENDING ORDER
                // This prevents creating a new Order record every time the user clicks "Checkout"
                var existingOrder = (await _orderRepository.GetAllAsync(
                    filter: o => o.UserId == userId && o.Status == OrderStatus.Pending,
                    cancellationToken)).OrderByDescending(o => o.CreatedAt).FirstOrDefault();

                Order order;
                if (existingOrder != null)
                {
                    // Update existing order details (in case cart items changed)
                    order = existingOrder;
                    order.Total = totalAmount;
                    order.Items = cartItems.Select(c => new OrderItem
                    {
                        CourseId = c.CourseId,
                        Price = courseDict[c.CourseId].DiscountPrice ?? courseDict[c.CourseId].Price
                    }).ToList();

                    await _orderRepository.UpdateAsync(order.Id, order, cancellationToken);
                }
                else
                {
                    // Create new order if none exists
                    order = new Order
                    {
                        UserId = userId,
                        Total = totalAmount,
                        Status = OrderStatus.Pending,
                        PaymentStatus = PaymentStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        Items = cartItems.Select(c => new OrderItem
                        {
                            CourseId = c.CourseId,
                            Price = courseDict[c.CourseId].DiscountPrice ?? courseDict[c.CourseId].Price
                        }).ToList()
                    };
                    await _orderRepository.CreateAsync(order, cancellationToken);
                }

                // 3. STRIPE LOGIC
                var paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntent;

                if (string.IsNullOrEmpty(order.PaymentIntentId))
                {
                    // Create New Intent
                    var createOptions = new PaymentIntentCreateOptions
                    {
                        Amount = (long)(totalAmount * 100),
                        Currency = "usd",
                        PaymentMethodTypes = new List<string> { "card" },
                        Metadata = new Dictionary<string, string>
                {
                    { "user_id", userId },
                    { "order_id", order.Id }
                }
                    };
                    paymentIntent = await paymentIntentService.CreateAsync(createOptions, cancellationToken: cancellationToken);

                    // Link Intent to Order
                    order.PaymentIntentId = paymentIntent.Id;
                    await _orderRepository.UpdateAsync(order.Id, order, cancellationToken);
                }
                else
                {
                    // Update Existing Intent (in case price changed)
                    var updateOptions = new PaymentIntentUpdateOptions
                    {
                        Amount = (long)(totalAmount * 100)
                    };
                    paymentIntent = await paymentIntentService.UpdateAsync(order.PaymentIntentId, updateOptions, cancellationToken: cancellationToken);
                }

                return new PaymentIntentResult
                {
                    PaymentIntentId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Amount = totalAmount,
                    Currency = "usd"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateOrUpdatePaymentIntent for user {UserId}", userId);
                throw;
            }
        }
        public async Task<bool> UpdatePaymentIntentToSucceededOrFailedAsync(
            string paymentIntentId,
            bool isSucceeded,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByPaymentIntentIdAsync(
                    paymentIntentId,
                    cancellationToken);

                if (order == null)
                {
                    _logger.LogWarning(
                        "Order not found for PaymentIntentId {PaymentIntentId}",
                        paymentIntentId);
                    return false;
                }

                if (isSucceeded)
                {
                    await _orderRepository.UpdateOrderStatusAsync(
                        order.Id, OrderStatus.Completed, cancellationToken);

                    await _orderRepository.UpdatePaymentStatusAsync(
                        order.Id, PaymentStatus.Paid, paymentIntentId, cancellationToken);

                    _logger.LogInformation(
                        "Payment succeeded for Order {OrderId}", order.Id);
                }
                else
                {
                    await _orderRepository.UpdateOrderStatusAsync(
                        order.Id, OrderStatus.Failed, cancellationToken);

                    await _orderRepository.UpdatePaymentStatusAsync(
                        order.Id, PaymentStatus.Failed, paymentIntentId, cancellationToken);

                    _logger.LogWarning(
                        "Payment failed for Order {OrderId}", order.Id);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating payment status for {PaymentIntentId}", paymentIntentId);
                throw;
            }
        }
        public async Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(
           string userId,
           CancellationToken cancellationToken = default)
        {
            try
            {
                var payments = await _paymentRepository.GetAllAsync(
                    filter: p => p.UserId == userId,
                    cancellationToken);

                return payments
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for user {UserId}", userId);
                throw;
            }
        }
        public async Task<PaymentDto?> GetPaymentByIdAsync(
           string paymentId,
           string userId,
           CancellationToken cancellationToken = default)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);

                if (payment == null || payment.UserId != userId)
                    return null;

                return MapToDto(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {PaymentId}", paymentId);
                throw;
            }
        }
        public async Task<(bool success, string message)> ConfirmPaymentAsync(
              string paymentIntentId,
              CancellationToken cancellationToken = default)
        {
            try
            {
                var payment = await _paymentRepository.GetOneAsync(
                    p => p.PaymentIntentId == paymentIntentId,
                    cancellationToken);

                if (payment == null)
                    return (false, "Payment not found");

                // Get payment intent from Stripe
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

                if (paymentIntent.Status != "succeeded")
                    return (false, $"Payment status: {paymentIntent.Status}");

                // Update payment
                payment.Status = PaymentStatus.Paid;
                payment.PaidAt = DateTime.UtcNow;
                payment.TransactionId = paymentIntent.Id;
                payment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment.Id, payment, cancellationToken);

                var order = await _orderRepository.GetByIdAsync(payment.OrderId);
                if (order != null)
                {
                    order.Status = OrderStatus.Completed;
                    order.PaymentStatus = PaymentStatus.Paid;
                    await _orderRepository.UpdateAsync(order.Id, order, cancellationToken);


                }

                if (order.Items != null && order.Items.Any())
                {
                    foreach (var item in order.Items)
                    {
                        var enrollmentExists = await _enrollmentRepository.GetOneAsync(e =>
                            e.UserId == order.UserId && e.CourseId == item.CourseId);

                        if (enrollmentExists == null)
                        {
                            var enrollment = new Enrollment
                            {
                                UserId = order.UserId,
                                CourseId = item.CourseId,
                                CreatedAt = DateTime.UtcNow
                            };

                            await _enrollmentRepository.CreateAsync(enrollment);
                        }
                    }
                    _logger.LogInformation("Enrollments created for User {UserId}", order.UserId);
                }

                return (true, "Payment and Order confirmed successfully");


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment {PaymentIntentId}", paymentIntentId);
                return (false, "An error occurred");
            }

        }
        public async Task<(bool success, string message)> RefundPaymentAsync(
            string paymentId,
            decimal? amount,
            string? reason,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);

                if (payment == null)
                    return (false, "Payment not found");

                if (payment.Status != PaymentStatus.Paid)
                    return (false, "Only paid payments can be refunded");

                var refundAmount = amount ?? payment.Amount;
                if (refundAmount > payment.Amount)
                    return (false, "Refund amount exceeds payment amount");

                // Create refund in Stripe
                var refundService = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = payment.PaymentIntentId,
                    Amount = (long)(refundAmount * 100),
                    Reason = reason ?? "requested_by_customer"
                };
                var refund = await refundService.CreateAsync(refundOptions, cancellationToken: cancellationToken);

                // Update payment
                payment.Status = PaymentStatus.Refunded;
                payment.RefundAmount = refundAmount;
                payment.RefundedAt = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment.Id, payment, cancellationToken);

                // Update order if exists
                if (!string.IsNullOrEmpty(payment.OrderId))
                {
                    await _orderRepository.UpdateOrderStatusAsync(
                        payment.OrderId,
                        OrderStatus.Refunded,
                        cancellationToken);
                }

                return (true, "Payment refunded successfully");
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error refunding payment {PaymentId}", paymentId);
                return (false, $"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment {PaymentId}", paymentId);
                return (false, "An error occurred");
            }
        }
        private static PaymentDto MapToDto(Payment payment) => new()
        {
            Id = payment.Id,
            UserId = payment.UserId,
            OrderId = payment.OrderId,
            PaymentIntentId = payment.PaymentIntentId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status.ToString(),
            TransactionId = payment.TransactionId,
            ReceiptUrl = payment.ReceiptUrl,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        };
    }
}

