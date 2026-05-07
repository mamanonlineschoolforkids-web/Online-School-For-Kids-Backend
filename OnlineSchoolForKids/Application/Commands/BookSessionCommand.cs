using Domain.Entities;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Repositories.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands;

public record BookSessionCommand(
    string StudentId,
    string SpecialistId,
    string Title,
    string? Description,
    string AppointmentDate,
    string StartTime,
    string EndTime
) : IRequest<BookSessionResult>;

public class BookSessionCommandHandler : IRequestHandler<BookSessionCommand, BookSessionResult>
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IUserRepository _userRepo;

    public BookSessionCommandHandler(
        IAppointmentRepository appointmentRepo,
        IUserRepository userRepo)
    {
        _appointmentRepo = appointmentRepo;
        _userRepo        = userRepo;
    }

    public async Task<BookSessionResult> Handle(BookSessionCommand request, CancellationToken cancellationToken)
    {
        // Validate specialist exists
        var specialist = await _userRepo.GetByIdAsync(request.SpecialistId, cancellationToken)
            ?? throw new KeyNotFoundException("Specialist not found.");

        // Check for conflicts
        var hasConflict = await _appointmentRepo.HasConflictAsync(
            request.SpecialistId,
            request.AppointmentDate,
            request.StartTime,
            request.EndTime,
            cancellationToken);

        if (hasConflict)
            throw new InvalidOperationException("This time slot is already booked.");

        var appointment = new Appointment
        {
            SpecialistId    = request.SpecialistId,
            StudentId       = request.StudentId,
            Title           = request.Title,
            Description     = request.Description,
            AppointmentDate = request.AppointmentDate,
            StartTime       = request.StartTime,
            EndTime         = request.EndTime,
            Status          = Domain.Enums.AppointmentStatus.Pending,
        };

        var created = await _appointmentRepo.CreateAsync(appointment, cancellationToken);

        return new BookSessionResult(created.Id, created.Status.ToString());
    }
}

public record BookSessionResult(string Id, string Status);