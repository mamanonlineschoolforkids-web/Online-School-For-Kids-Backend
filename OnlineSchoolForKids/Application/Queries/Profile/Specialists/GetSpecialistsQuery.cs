using Application.Queries.Admin;
using Application.Queries.Content;
using Domain.Interfaces.Repositories.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Profile.Specialists;

public record GetSpecialistsQuery(
    string? Search,
    string? Specialization,
    decimal? MinRate,
    decimal? MaxRate,
    double? MinRating,
    string? SortBy,
    string? SortOrder,
    int Page,
    int PageSize
) : IRequest<PagedSpecialistsResult>;
public class GetSpecialistsQueryHandler : IRequestHandler<GetSpecialistsQuery, PagedSpecialistsResult>
{
    private readonly IUserRepository _userRepository;

    public GetSpecialistsQueryHandler(IUserRepository userRepository)
        => _userRepository = userRepository;

    public async Task<PagedSpecialistsResult> Handle(GetSpecialistsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var (items, totalCount) = await _userRepository.GetSpecialistsPagedAsync(
            search: request.Search,
            specialization: request.Specialization,
            minRate: request.MinRate,
            maxRate: request.MaxRate,
            minRating: request.MinRating,
            sortBy: request.SortBy,
            sortOrder: request.SortOrder,
            skip: skip,
            limit: request.PageSize,
            ct: cancellationToken
        );

        var dtos = items.Select(u => new SpecialistListItemDto
        {
            Id                = u.Id,
            FullName          = u.FullName,
            ProfilePictureUrl = u.ProfilePictureUrl,
            Bio               = u.Bio,
            Country           = u.Country,
            ProfessionalTitle = u.ProfessionalTitle,
            Specializations   = u.ExpertiseTags ?? [],
            YearsOfExperience = u.YearsOfExperience ?? 0,
            HourlyRate        = u.HourlyRate ?? 0,
            Rating            = u.AverageRating ?? 5,
            StudentsHelped    = u.TotalStudents ?? 100,
            ReviewsCount      = u.ReviewsCount ?? 0,
        }).ToList();

        return new PagedSpecialistsResult
        {
            Items      = dtos,
            TotalCount = totalCount,
            Page       = request.Page,
            PageSize   = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
        };
    }
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public class SpecialistListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public string? Country { get; set; }
    public string? ProfessionalTitle { get; set; }
    public List<string> Specializations { get; set; } = [];
    public int YearsOfExperience { get; set; }
    public decimal HourlyRate { get; set; }
    public double Rating { get; set; }
    public int StudentsHelped { get; set; }
    public int ReviewsCount { get; set; }
}

public class PagedSpecialistsResult
{
    public List<SpecialistListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
