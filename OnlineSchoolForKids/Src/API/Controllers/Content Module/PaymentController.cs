using Application.Commands.HandleWebhook;
using Application.Commands.Order.Application.Commands.Checkout;
using Domain.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;
        private readonly IPaymentService _paymentService;

        public PaymentController(
            IMediator mediator,
            IConfiguration configuration,
            ILogger<PaymentController> logger,
            IPaymentService paymentService)
        {
            _mediator = mediator;
            _configuration = configuration;
            _logger = logger;
            _paymentService = paymentService;
        }
        [HttpPost("createOrUpdateIntent")]
        [Authorize]
        [ProducesResponseType(typeof(CreateOrUpdatePaymentIntentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreateOrUpdatePaymentIntentResponse>> CreateOrUpdatePaymentIntent(
              CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthenticated user attempted to create payment intent");
                    return Unauthorized(new
                    {
                        message = "User not authenticated",
                        success = false
                    });
                }
                var command = new CreateOrUpdatePaymentIntentCommand
                {
                    UserId = userId
                };
                var result = await _mediator.Send(command, cancellationToken);
                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Failed to create payment intent for user {UserId}: {Message}",
                        userId, result.Message);

                    return BadRequest(new
                    {
                        message = result.Message,
                        success = false
                    });
                }

                _logger.LogInformation(
                    "Payment intent created successfully for user {UserId}: {PaymentIntentId}",
                    userId, result.PaymentIntentId);

                return Ok(new
                {
                    data = new
                    {
                        paymentIntentId = result.PaymentIntentId,
                        clientSecret = result.ClientSecret,
                        amount = result.Amount,
                        currency = result.Currency
                    },
                    message = result.Message,
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating payment intent");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An unexpected error occurred while creating payment intent",
                    success = false
                });
            }
        }
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var webhookSecret = _configuration["StripeSettings:WebhookSecret"];
                var signature = Request.Headers["Stripe-Signature"];

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signature,
                    webhookSecret,
                    throwOnApiVersionMismatch: false);

                _logger.LogInformation("Stripe webhook: {EventType}", stripeEvent.Type);

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        var succeededIntent = stripeEvent.Data.Object as PaymentIntent;
                        if (succeededIntent != null)
                        {
                            var command = new HandleWebhookCommand
                            {
                                PaymentIntentId = succeededIntent.Id,
                                IsSucceeded = true,
                                TransactionId = succeededIntent.Id,
                                ReceiptUrl = succeededIntent.LatestChargeId
                            };

                            await _mediator.Send(command, cancellationToken);
                        }
                        break;

                    case "payment_intent.payment_failed":
                        var failedIntent = stripeEvent.Data.Object as PaymentIntent;
                        if (failedIntent != null)
                        {
                            var command = new HandleWebhookCommand
                            {
                                PaymentIntentId = failedIntent.Id,
                                IsSucceeded = false,
                                FailureReason = failedIntent.LastPaymentError?.Message ?? "Payment failed"
                            };

                            await _mediator.Send(command, cancellationToken);
                        }
                        break;

                    default:
                        _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Invalid webhook signature");
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500);
            }
        }
        /// <summary>
        /// Process checkout:
        /// 1. Validates cart
        /// 2. Creates / updates payment intent (Stripe)
        /// 3. Creates order in MongoDB
        /// 4. Clears cart from Redis
        /// Returns clientSecret for frontend Stripe confirmation
        /// </summary>
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(
            [FromBody] CheckoutDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated", success = false });

                var command = new CheckoutCommand
                {
                    UserId = userId,
                    Notes = dto.Notes
                };

                var result = await _mediator.Send(command, cancellationToken);

                if (!result.Success)
                    return BadRequest(new { message = result.Message, success = false });

                return Ok(new
                {
                    data = new
                    {
                        orderId = result.OrderId,
                        orderNumber = result.OrderNumber,
                        total = result.Total,
                        paymentIntentId = result.PaymentIntentId,
                        clientSecret = result.ClientSecret,
                        currency = "usd"
                    },
                    message = result.Message,
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }

        }
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserPayments(CancellationToken cancellationToken)
        {
            try
            {

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated", success = false });
                var payments = await _paymentService.GetUserPaymentsAsync(userId, cancellationToken);

                return Ok(new { data = payments, success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user payments");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPaymentById(
           string id,
           CancellationToken cancellationToken)
        {
            try
            {

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated", success = false });
                var payment = await _paymentService.GetPaymentByIdAsync(
                    id, userId, cancellationToken);

                if (payment == null)
                    return NotFound(new { message = "Payment not found", success = false });

                return Ok(new { data = payment, success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {Id}", id);
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }

        [HttpPost("confirm")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmPayment(
            [FromBody] ConfirmPaymentDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var (success, message) = await _paymentService.ConfirmPaymentAsync(
                    dto.PaymentIntentId, cancellationToken);

                if (!success)
                    return BadRequest(new { message, success = false });

                return Ok(new { message, success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }

        [HttpPost("{id}/refund")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RefundPayment(
           string id,
           [FromBody] RefundPaymentDto dto,
           CancellationToken cancellationToken)
        {
            try
            {
                var (success, message) = await _paymentService.RefundPaymentAsync(
                    id, dto.Amount, dto.Reason, cancellationToken);

                if (!success)
                    return BadRequest(new { message, success = false });

                return Ok(new { message, success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment {Id}", id);
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }

    }

    public class CheckoutDto
    {
        public string? Notes { get; set; }
    }
    public class ConfirmPaymentDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
    }

    public class RefundPaymentDto
    {
        public decimal? Amount { get; set; } // null = full refund
        public string? Reason { get; set; }
    }
}







