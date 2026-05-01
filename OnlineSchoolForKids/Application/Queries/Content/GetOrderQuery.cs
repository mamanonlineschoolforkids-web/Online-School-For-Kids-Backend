using Domain.Entities.Content.Orders;
using Domain.Interfaces.Repositories.Content;
using MediatR;
using static Application.Queries.GetUserOrders.GetUserOrdersQueryHandler;

namespace Application.Queries.Content
{
    public class GetOrderQuery : IRequest<OrderDto?>
    {
        public string OrderId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto?>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

            if (order == null || order.UserId != request.UserId)
                return null;

            return MapToDto(order);
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                Status = order.Status.ToString(),
                PaymentStatus = order.PaymentStatus.ToString(),
                PaymentMethod = order.PaymentMethod.ToString(),
                Subtotal = order.Subtotal,
                Tax = order.Tax,
                Total = order.Total,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    CourseId = i.CourseId,
                    CourseTitle = i.CourseTitle,
                    CourseThumbnail = i.CourseThumbnail,
                    InstructorName = i.InstructorName,
                    Price = i.Price,
                    OriginalPrice = i.OriginalPrice,
                    DiscountPercentage = i.DiscountPercentage
                }).ToList(),
                CreatedAt = order.CreatedAt,
                CompletedAt = order.CompletedAt
            };
        }
    }
}

namespace Application.Queries.GetUserOrders
{
    public class GetUserOrdersQuery : IRequest<IEnumerable<OrderSummaryDto>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, IEnumerable<OrderSummaryDto>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetUserOrdersQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderSummaryDto>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _orderRepository.GetUserOrdersAsync(request.UserId, cancellationToken);

            return orders.Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status.ToString(),
                PaymentStatus = o.PaymentStatus.ToString(),
                Total = o.Total,
                ItemCount = o.Items.Count,
                CreatedAt = o.CreatedAt
            });
        }
        public class OrderDto
        {
            public string Id { get; set; } = string.Empty;
            public string OrderNumber { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string PaymentStatus { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public decimal Subtotal { get; set; }
            public decimal Tax { get; set; }
            public decimal Total { get; set; }
            public List<OrderItemDto> Items { get; set; } = new();
            public DateTime CreatedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
        }

        public class OrderItemDto
        {
            public string Id { get; set; } = string.Empty;
            public string CourseId { get; set; } = string.Empty;
            public string CourseTitle { get; set; } = string.Empty;
            public string CourseThumbnail { get; set; } = string.Empty;
            public string InstructorName { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public decimal? OriginalPrice { get; set; }
            public int DiscountPercentage { get; set; }
        }

        public class OrderSummaryDto
        {
            public string Id { get; set; } = string.Empty;
            public string OrderNumber { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string PaymentStatus { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public int ItemCount { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}



