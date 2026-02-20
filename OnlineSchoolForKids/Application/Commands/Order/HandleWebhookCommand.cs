using Domain.Entities;
using Domain.Entities.Content;
using Domain.Enums.Content;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Repositories.Content;
using Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.HandleWebhook
{
    public class HandleWebhookCommand : IRequest<bool>
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public bool IsSucceeded { get; set; }
        public string? TransactionId { get; set; }
        public string? ReceiptUrl { get; set; }
        public string? FailureReason { get; set; }
    }

    public class HandleWebhookCommandHandler : IRequestHandler<HandleWebhookCommand, bool>
    {
        private readonly IGenericRepository<Payment> _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IGenericRepository<Enrollment> _enrollmentRepository;
        private readonly ILogger<HandleWebhookCommandHandler> _logger;

        public HandleWebhookCommandHandler(
            IGenericRepository<Payment> paymentRepository,
            IOrderRepository orderRepository,
            IGenericRepository<Enrollment> enrollmentRepository,
            ILogger<HandleWebhookCommandHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _enrollmentRepository = enrollmentRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(
            HandleWebhookCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // STEP 1: Get order by PaymentIntentId
                var order = await _orderRepository.GetByPaymentIntentIdAsync(request.PaymentIntentId, CancellationToken.None);
                if (order == null)
                {
                    _logger.LogWarning(
                        "Order not found for PaymentIntentId {PaymentIntentId}",
                        request.PaymentIntentId);
                    return false;
                }

                if (request.IsSucceeded)
                {
                    _logger.LogInformation(
                        "Payment succeeded for Order {OrderId}, PaymentIntentId {PaymentIntentId}",
                        order.Id, request.PaymentIntentId);

                    // STEP 2: Create Payment record
                    var payment = new Payment
                    {
                        OrderId = order.Id,
                        UserId = order.UserId,
                        Amount = order.Total,
                        Currency = "USD",
                        Status = PaymentStatus.Paid,
                        PaymentMethod = order.PaymentMethod,
                        PaymentIntentId = request.PaymentIntentId,
                        TransactionId = request.TransactionId ?? request.PaymentIntentId,
                        ReceiptUrl = request.ReceiptUrl,
                        PaidAt = DateTime.UtcNow,
                        Metadata = new Dictionary<string, string>
                        {
                            { "order_id", order.Id },
                            { "order_number", order.OrderNumber },
                            { "payment_intent_id", request.PaymentIntentId }
                        }
                    };

                    await _paymentRepository.CreateAsync(payment, cancellationToken);

                    _logger.LogInformation(
                        "Payment record created for Order {OrderId}, Amount: {Amount}",
                        order.Id, payment.Amount);

                    // STEP 3: Update order payment status
                    await _orderRepository.UpdatePaymentStatusAsync(
                        order.Id,
                        PaymentStatus.Paid,
                        request.PaymentIntentId,
                        cancellationToken);

                    // STEP 4: Create enrollments
                    foreach (var item in order.Items)
                    {
                        var alreadyEnrolled = await _enrollmentRepository.ExistsAsync(
                            e => e.UserId == order.UserId && e.CourseId == item.CourseId,
                            cancellationToken);

                        if (!alreadyEnrolled)
                        {
                            var enrollment = new Enrollment
                            {
                                UserId = order.UserId,
                                CourseId = item.CourseId,
                                CreatedAt = DateTime.UtcNow,
                                Progress = 0,
                                IsCompleted = false
                            };

                            await _enrollmentRepository.CreateAsync(enrollment, cancellationToken);

                            _logger.LogInformation(
                                "Enrollment created for User {UserId}, Course {CourseId}",
                                order.UserId, item.CourseId);
                        }
                        else
                        {
                            _logger.LogInformation(
                                "User {UserId} already enrolled in Course {CourseId}, skipping",
                                order.UserId, item.CourseId);
                        }
                    }

                    // STEP 5: Mark order as completed
                    await _orderRepository.UpdateOrderStatusAsync(
                        order.Id,
                        OrderStatus.Completed,
                        cancellationToken);

                    _logger.LogInformation(
                        "Order {OrderId} marked as Completed, {EnrollmentCount} enrollments created",
                        order.Id, order.Items.Count);
                }
                else
                {
                    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                    //  PAYMENT FAILED
                    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

                    _logger.LogWarning(
                        "Payment failed for Order {OrderId}, PaymentIntentId {PaymentIntentId}, Reason: {Reason}",
                        order.Id, request.PaymentIntentId, request.FailureReason);

                    // Create Payment record with Failed status
                    var payment = new Payment
                    {
                        OrderId = order.Id,
                        UserId = order.UserId,
                        Amount = order.Total,
                        Currency = "USD",
                        Status = PaymentStatus.Failed,
                        PaymentMethod = order.PaymentMethod,
                        PaymentIntentId = request.PaymentIntentId,
                        FailureReason = request.FailureReason ?? "Payment declined",
                        Metadata = new Dictionary<string, string>
                        {
                            { "order_id", order.Id },
                            { "order_number", order.OrderNumber },
                            { "payment_intent_id", request.PaymentIntentId }
                        }
                    };

                    await _paymentRepository.CreateAsync(payment, cancellationToken);

                    _logger.LogInformation(
                        "Failed payment record created for Order {OrderId}",
                        order.Id);

                    // Update order status to PaymentFailed
                    await _orderRepository.UpdatePaymentStatusAsync(
                        order.Id,
                        PaymentStatus.Failed,
                        request.PaymentIntentId,
                        cancellationToken);

                    await _orderRepository.UpdateOrderStatusAsync(
                        order.Id,
                        OrderStatus.Failed,
                        cancellationToken);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error handling webhook for PaymentIntent {PaymentIntentId}",
                    request.PaymentIntentId);
                return false;
            }
        }
    }
}
public class CreateOrUpdatePaymentIntentCommand : IRequest<CreateOrUpdatePaymentIntentResponse>
{
    public string UserId { get; set; } = string.Empty;
}
public class CreateOrUpdatePaymentIntentCommandHandler
     : IRequestHandler<CreateOrUpdatePaymentIntentCommand, CreateOrUpdatePaymentIntentResponse>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<CreateOrUpdatePaymentIntentCommandHandler> _logger;

    public CreateOrUpdatePaymentIntentCommandHandler(
        IPaymentService paymentService,
        ILogger<CreateOrUpdatePaymentIntentCommandHandler> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<CreateOrUpdatePaymentIntentResponse> Handle(CreateOrUpdatePaymentIntentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var serviceResult = await _paymentService.CreateOrUpdatePaymentIntentAsync(
                command.UserId,
                cancellationToken);

            // Check if service returned null (cart empty)
            if (serviceResult == null)
            {
                return new CreateOrUpdatePaymentIntentResponse
                {
                    Success = false,
                    Message = "Cart is empty or no valid courses found"
                };
            }

            _logger.LogInformation(
                "PaymentIntent created/updated for user {UserId}: {PaymentIntentId}",
                command.UserId, serviceResult.PaymentIntentId);

            return new CreateOrUpdatePaymentIntentResponse
            {
                Success = true,
                Message = "Payment intent ready",
                PaymentIntentId = serviceResult.PaymentIntentId,   // ← From PaymentService
                ClientSecret = serviceResult.ClientSecret,      // ← From PaymentService
                Amount = serviceResult.Amount,            // ← From PaymentService
                Currency = serviceResult.Currency           // ← From PaymentService
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent for user {UserId}", command.UserId);

            return new CreateOrUpdatePaymentIntentResponse
            {
                Success = false,
                Message = "An error occurred while creating payment intent"
            };
        }
    }
}

public class CreateOrUpdatePaymentIntentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
}

