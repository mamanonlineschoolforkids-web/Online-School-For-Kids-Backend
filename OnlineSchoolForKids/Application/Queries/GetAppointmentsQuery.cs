using Domain.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries;

public record GetMyAppointmentsQuery(string UserId, string Role) : IRequest<List<AppointmentDto>>;

public class GetMyAppointmentsQueryHandler : IRequestHandler<GetMyAppointmentsQuery, List<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepo;

    public GetMyAppointmentsQueryHandler(IAppointmentRepository appointmentRepo)
        => _appointmentRepo = appointmentRepo;

    public async Task<List<AppointmentDto>> Handle(GetMyAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = request.Role.ToLower() == "Specialist"
            ? await _appointmentRepo.GetBySpecialistIdAsync(request.UserId, cancellationToken)
            : await _appointmentRepo.GetByStudentIdAsync(request.UserId, cancellationToken);

        return appointments.Select(a => new AppointmentDto
        {
            Id              = a.Id,
            SpecialistId    = a.SpecialistId,
            StudentId       = a.StudentId,
            Title           = a.Title,
            Description     = a.Description,
            AppointmentDate = a.AppointmentDate,
            StartTime       = a.StartTime,
            EndTime         = a.EndTime,
            Status          = a.Status.ToString(),
        }).ToList();
    }
}

public record GetBookedSlotsQuery(string SpecialistId, string Date) : IRequest<List<string>>;

public class GetBookedSlotsQueryHandler : IRequestHandler<GetBookedSlotsQuery, List<string>>
{
    private readonly IAppointmentRepository _appointmentRepo;

    public GetBookedSlotsQueryHandler(IAppointmentRepository appointmentRepo)
        => _appointmentRepo = appointmentRepo;

    public async Task<List<string>> Handle(GetBookedSlotsQuery request, CancellationToken cancellationToken)
    {
        var slots = await _appointmentRepo.GetBookedSlotsAsync(
            request.SpecialistId, request.Date, cancellationToken);
        return slots.ToList();
    }
}

public class AppointmentDto
{
    public string Id { get; set; } = string.Empty;
    public string SpecialistId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AppointmentDate { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}