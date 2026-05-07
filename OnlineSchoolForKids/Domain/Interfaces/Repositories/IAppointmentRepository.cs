using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment> CreateAsync(Appointment appointment, CancellationToken ct = default);
    Task<Appointment?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<bool> UpdateAsync(string id, Appointment appointment, CancellationToken ct = default);
    Task<IEnumerable<Appointment>> GetBySpecialistIdAsync(string specialistId, CancellationToken ct = default);
    Task<IEnumerable<Appointment>> GetByStudentIdAsync(string studentId, CancellationToken ct = default);
    Task<IEnumerable<string>> GetBookedSlotsAsync(string specialistId, string date, CancellationToken ct = default);
    Task<bool> HasConflictAsync(string specialistId, string date, string startTime, string endTime, CancellationToken ct = default);
}