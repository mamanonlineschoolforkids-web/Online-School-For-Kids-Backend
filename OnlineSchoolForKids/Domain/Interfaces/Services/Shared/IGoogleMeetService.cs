using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services.Shared;

public interface IGoogleMeetService
{
    /// <summary>
    /// Creates a Google Calendar event with an auto-generated Google Meet link.
    /// Returns (meetLink, eventId).
    /// </summary>
    Task<(string MeetLink, string EventId)> CreateMeetingAsync(
        string title,
        DateTime startUtc,
        int durationMinutes,
        string organizerEmail,
        string attendeeEmail,
        CancellationToken ct = default);

    /// <summary>Deletes the calendar event (e.g. on cancellation).</summary>
    Task DeleteMeetingAsync(string eventId, CancellationToken ct = default);
}