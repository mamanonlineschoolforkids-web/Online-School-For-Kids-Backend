using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Infrastructure.Data;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(MongoDbContext context) : base(context.GetCollection<Appointment>("Appointments"))
    {
    }

    public async Task<IEnumerable<Appointment>> GetBySpecialistIdAsync(string specialistId, CancellationToken ct = default)
    {
        return await _collection
            .Find(a => a.SpecialistId == specialistId && !a.IsDeleted)
            .SortBy(a => a.AppointmentDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Appointment>> GetByStudentIdAsync(string studentId, CancellationToken ct = default)
    {
        return await _collection
            .Find(a => a.StudentId == studentId && !a.IsDeleted)
            .SortBy(a => a.AppointmentDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<string>> GetBookedSlotsAsync(string specialistId, string date, CancellationToken ct = default)
    {
        var filter = Builders<Appointment>.Filter.And(
            Builders<Appointment>.Filter.Eq(a => a.SpecialistId, specialistId),
            Builders<Appointment>.Filter.Eq(a => a.AppointmentDate, date),
            Builders<Appointment>.Filter.Eq(a => a.IsDeleted, false),
            Builders<Appointment>.Filter.Ne(a => a.Status, AppointmentStatus.Cancelled)
        );

        var appointments = await _collection.Find(filter).ToListAsync(ct);
        return appointments.Select(a => a.StartTime);
    }

    public async Task<bool> HasConflictAsync(string specialistId, string date, string startTime, string endTime, CancellationToken ct = default)
    {
        var filter = Builders<Appointment>.Filter.And(
            Builders<Appointment>.Filter.Eq(a => a.SpecialistId, specialistId),
            Builders<Appointment>.Filter.Eq(a => a.AppointmentDate, date),
            Builders<Appointment>.Filter.Eq(a => a.IsDeleted, false),
            Builders<Appointment>.Filter.Ne(a => a.Status, AppointmentStatus.Cancelled),
            Builders<Appointment>.Filter.Lt(a => a.StartTime, endTime),
            Builders<Appointment>.Filter.Gt(a => a.EndTime, startTime)
        );

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: ct);
        return count > 0;
    }
}