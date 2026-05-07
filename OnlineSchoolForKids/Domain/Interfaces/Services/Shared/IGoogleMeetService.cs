using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services.Shared;

public interface IGoogleMeetService
{

    Task<string> CreateMeetingAsync(
          string title,
          DateTime startUtc,
          DateTime endUtc,
          string organizerEmail,
          string attendeeEmail,
          CancellationToken ct = default);
}