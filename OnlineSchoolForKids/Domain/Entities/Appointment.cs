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
    public string StartTime { get; set; } = string.Empty; // "HH:mm"
    public string EndTime { get; set; } = string.Empty; // "HH:mm"

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? CancellationReason { get; set; }

    // --- NEW FIELDS ---

    /// <summary>UTC deadline by which the student must complete payment.
    /// After this time the background job frees the slot.</summary>
    public DateTime HoldExpiresAtUtc { get; set; }

    /// <summary>Google Meet join URL — set when Status → Confirmed.</summary>
    public string? GoogleMeetLink { get; set; }

    /// <summary>Payment transaction ID returned by the processor.</summary>
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// The student's saved PaymentMethod document ID used at confirm-time.
    /// Stored so cancellation can resolve the correct processor for refunds.
    /// </summary>
    public string? PaymentMethodId { get; set; }

    /// <summary>Amount actually charged (in USD/platform currency).</summary>
    public decimal? AmountPaid { get; set; }

    public DateTime? ConfirmedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }

    // ------------------

    /// <summary>True when the session can still be cancelled (>30 min before start).</summary>
    public bool CanCancel()
    {
        if (Status == AppointmentStatus.Cancelled || Status == AppointmentStatus.Completed)
            return false;

        // Parse "yyyy-MM-dd" + "HH:mm" into a UTC DateTime
        if (!DateTime.TryParse($"{AppointmentDate}T{StartTime}:00",
                out var meetingStart))
            return false;

        return DateTime.UtcNow < meetingStart.ToUniversalTime().AddMinutes(-30);
    }
}