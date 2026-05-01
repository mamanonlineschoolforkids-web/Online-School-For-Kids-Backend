using Application.Queries.Admin;
using Application.Queries.Content;
using Domain.Interfaces.Repositories.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Session_Booking;

public record GetSpecialistsQuery(
    string? SearchQuery,
    string? Specialization,
    decimal? MinRate,
    decimal? MaxRate,
    double? MinRating,
    string? SortBy,
    string? SortOrder,
    int Page,
    int PageSize
) : IRequest<PagedResult<SpecialistDto>>;

public class GetSpecialistsHandler(IUserRepository userRepository)
    : IRequestHandler<GetSpecialistsQuery, PagedResult<SpecialistDto>>
{
    public async Task<PagedResult<SpecialistDto>> Handle(
     GetSpecialistsQuery request,
     CancellationToken ct)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var (items, totalCount) = await userRepository.GetSpecialistsPagedAsync(
            search: request.SearchQuery,
            specialization: request.Specialization,
            minRate: request.MinRate,
            maxRate: request.MaxRate,
            minRating: request.MinRating,
            sortBy: request.SortBy,
            sortOrder: request.SortOrder,
            skip: skip,
            limit: request.PageSize,
            ct: ct
        );

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResult<SpecialistDto>
        {
            Items      = dtos,
            TotalCount = (int)totalCount,
            Page       = request.Page,
            PageSize   = request.PageSize,
        };
    }
    private static SpecialistDto MapToDto(Domain.Entities.Users.User u) => new(
        Id: u.Id,
        FullName: u.FullName,
        AvatarUrl: u.ProfilePictureUrl,
        Bio: u.Bio,
        ProfessionalTitle: u.ProfessionalTitle,
        Location: u.Country,
        ExpertiseTags: u.ExpertiseTags ?? [],
        AverageRating: u.AverageRating,
        ReviewsCount: u.ReviewsCount,
        StudentsCount: u.StudentsCount,
        YearsOfExperience: u.YearsOfExperience,
        HourlyRate: u.HourlyRate,
        SessionRates: u.SessionRates is null ? null : new SessionRatesDto(
            u.SessionRates.ThirtyMinSession,
            u.SessionRates.SixtyMinSession
        ),
        IsVerified: u.IsVerifiedCreator ?? false
    );
}

public record SessionRatesDto(
    decimal ThirtyMin,
    decimal SixtyMin
);

public record SpecialistDto(
    string Id,
    string FullName,
    string? AvatarUrl,
    string? Bio,
    string? ProfessionalTitle,
    string? Location,
    List<string> ExpertiseTags,
    double? AverageRating,
    int? ReviewsCount,
    int? StudentsCount,
    int? YearsOfExperience,
    decimal? HourlyRate,
    SessionRatesDto? SessionRates,
    bool IsVerified
);

