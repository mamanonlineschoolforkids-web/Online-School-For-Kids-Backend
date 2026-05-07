using Domain.Enums;
using Domain.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands;

public record UpdateAppointmentStatusCommand(
    string RequesterId,
    string AppointmentId,
    AppointmentStatus NewStatus,
    string? CancellationReason = null
) : IRequest;

public class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand>
{
    private readonly IAppointmentRepository _appointmentRepo;

    public UpdateAppointmentStatusCommandHandler(IAppointmentRepository appointmentRepo)
        => _appointmentRepo = appointmentRepo;

    public async Task Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepo.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new KeyNotFoundException("Appointment not found.");

        // Only the specialist can confirm/cancel, student can only cancel their own
        var isSpecialist = appointment.SpecialistId == request.RequesterId;
        var isStudent = appointment.StudentId    == request.RequesterId;

        if (!isSpecialist && !isStudent)
            throw new UnauthorizedAccessException("You don't have access to this appointment.");

        if (request.NewStatus == AppointmentStatus.Confirmed && !isSpecialist)
            throw new UnauthorizedAccessException("Only the specialist can confirm appointments.");

        appointment.Status             = request.NewStatus;
        appointment.CancellationReason = request.CancellationReason;

        await _appointmentRepo.UpdateAsync(appointment.Id, appointment, cancellationToken);
    }
}