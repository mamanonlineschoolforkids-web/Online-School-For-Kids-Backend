using Domain.Enums.Users;
using Domain.Interfaces.Services.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Shared.Payment;

public class PaymentProcessorFactory : IPaymentProcessorFactory
{
    private readonly IServiceProvider _sp;
    public PaymentProcessorFactory(IServiceProvider sp) => _sp = sp;

    public IPaymentProcessor Resolve(PaymentMethodType type) => type switch
    {
        PaymentMethodType.Card => _sp.GetRequiredService<CardProcessor>(),
        PaymentMethodType.VodafoneCash => _sp.GetRequiredService<VodafoneCashProcessor>(),
        PaymentMethodType.Instapay => _sp.GetRequiredService<InstapayProcessor>(),
        PaymentMethodType.Fawry => _sp.GetRequiredService<FawryProcessor>(),
        PaymentMethodType.BankAccount => _sp.GetRequiredService<BankAccountProcessor>(),
        _ => throw new NotSupportedException($"No processor for {type}")
    };
}

// Put this in the same file or a separate Extensions file
public static class PaymentRegistration
{
    public static IServiceCollection AddPaymentProcessors(this IServiceCollection services)
    {
        services.AddScoped<CardProcessor>();
        services.AddScoped<VodafoneCashProcessor>();
        services.AddScoped<InstapayProcessor>();
        services.AddScoped<FawryProcessor>();
        services.AddScoped<BankAccountProcessor>();
        services.AddScoped<IPaymentProcessorFactory, PaymentProcessorFactory>();
        return services;
    }
}