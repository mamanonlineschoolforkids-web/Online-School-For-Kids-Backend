using Domain.Interfaces.Repositories.Content;
using MediatR;

namespace Application.Queries.Leaderboard
{
    // ── Query ──────────────────────────────────────────────────────────────────

    public class GetMyTransactionsQuery : IRequest<List<PointTransactionDto>>
    {
        public string UserId { get; set; } = string.Empty;
        public int Limit { get; set; } = 20;
    }

    // ── DTO ────────────────────────────────────────────────────────────────────

    public class PointTransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int Points { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Handler ────────────────────────────────────────────────────────────────

    public class GetMyTransactionsHandler : IRequestHandler<GetMyTransactionsQuery, List<PointTransactionDto>>
    {
        private readonly IPointTransactionRepository _transactionRepo;

        public GetMyTransactionsHandler(IPointTransactionRepository transactionRepo)
        {
            _transactionRepo = transactionRepo;
        }

        public async Task<List<PointTransactionDto>> Handle(
            GetMyTransactionsQuery request,
            CancellationToken ct)
        {
            // GetPagedAsync already exists on GenericRepository
            var (items, _) = await _transactionRepo.GetPagedAsync(
                filter: tx => tx.UserId == request.UserId,
                orderBy: tx => tx.CreatedAt,
                orderByDescending: true,
                skip: 0,
                limit: request.Limit,
                cancellationToken: ct);

            return items.Select(tx => new PointTransactionDto
            {
                Id = tx.Id,
                UserId = tx.UserId,
                Points = tx.Points,
                Reason = tx.Reason.ToString(),
                Description = tx.Description,
                RelatedEntityId = tx.RelatedEntityId,
                CreatedAt = tx.CreatedAt
            }).ToList();
        }
    }
}