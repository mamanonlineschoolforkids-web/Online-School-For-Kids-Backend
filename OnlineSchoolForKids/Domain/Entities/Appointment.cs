using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class Appointment : BaseEntity
{
    public string SpecialistId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AppointmentDate { get; set; } = string.Empty; // "yyyy-MM-dd"
    public string StartTime { get; set; } = string.Empty;   // "HH:mm"
    public string EndTime { get; set; } = string.Empty;   // "HH:mm"
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? CancellationReason { get; set; }
}