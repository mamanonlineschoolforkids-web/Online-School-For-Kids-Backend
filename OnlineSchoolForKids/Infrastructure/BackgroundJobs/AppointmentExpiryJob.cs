using Application.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.BackgroundJobs;

/// <summary>
/// Runs every 2 minutes and cancels any Pending appointments
/// whose 30-minute hold has expired without payment.
/// Freed slots immediately become bookable by other users.
/// </summary>
public class AppointmentExpiryJob : BackgroundService
{
    private static readonly TimeSpan _interval = TimeSpan.FromMinutes(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AppointmentExpiryJob> _logger;

    public AppointmentExpiryJob(
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentExpiryJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AppointmentExpiryJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var released = await mediator.Send(
                    new ReleaseExpiredHoldsCommand(), stoppingToken);

                if (released > 0)
                    _logger.LogInformation(
                        "Released {Count} expired appointment hold(s) at {Time}.",
                        released, DateTime.UtcNow);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in AppointmentExpiryJob.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}