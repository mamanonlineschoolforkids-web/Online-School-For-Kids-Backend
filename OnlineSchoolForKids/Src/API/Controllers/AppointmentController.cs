using Application.Commands;
using Application.Queries;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AppointmentController : ControllerBase
{
    private readonly IMediator _mediator;
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public AppointmentController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> BookSession([FromBody] BookSessionDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new BookSessionCommand(
            UserId,
            dto.SpecialistId,
            dto.Title,
            dto.Description,
            dto.AppointmentDate,
            dto.StartTime,
            dto.EndTime), ct);

        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyAppointments(CancellationToken ct)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Student";
        var result = await _mediator.Send(new GetMyAppointmentsQuery(UserId, role), ct);
        return Ok(result);
    }

    [HttpGet("booked-slots")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBookedSlots(
        [FromQuery] string specialistId,
        [FromQuery] string date,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBookedSlotsQuery(specialistId, date), ct);
        return Ok(result);
    }

    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm(string id, CancellationToken ct)
    {
        await _mediator.Send(new UpdateAppointmentStatusCommand(
            UserId, id, AppointmentStatus.Confirmed), ct);
        return NoContent();
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(string id, [FromBody] CancelDto dto, CancellationToken ct)
    {
        await _mediator.Send(new UpdateAppointmentStatusCommand(
            UserId, id, AppointmentStatus.Cancelled, dto.Reason), ct);
        return NoContent();
    }

    [HttpPut("{id}/complete")]
    public async Task<IActionResult> Complete(string id, CancellationToken ct)
    {
        await _mediator.Send(new UpdateAppointmentStatusCommand(
            UserId, id, AppointmentStatus.Completed), ct);
        return NoContent();
    }
}

public class BookSessionDto
{
    public string SpecialistId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AppointmentDate { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}

public class CancelDto
{
    public string? Reason { get; set; }
}