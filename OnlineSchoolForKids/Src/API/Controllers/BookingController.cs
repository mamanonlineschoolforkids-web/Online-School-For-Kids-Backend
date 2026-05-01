using Application.Queries.Content;
using Application.Queries.Session_Booking;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingController : ControllerBase
{

    private readonly IMediator _mediator;

    public BookingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SpecialistDto>>> GetSpecialists(
        [FromQuery] string? searchQuery,
        [FromQuery] string? specialization,
        [FromQuery] decimal? minRate,
        [FromQuery] decimal? maxRate,
        [FromQuery] double? minRating,
        [FromQuery] string sortBy = "rating",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetSpecialistsQuery(
            searchQuery, specialization,
            minRate, maxRate, minRating,
            sortBy, sortOrder, page, pageSize
        ), ct);

        return Ok(result);
    }
}
