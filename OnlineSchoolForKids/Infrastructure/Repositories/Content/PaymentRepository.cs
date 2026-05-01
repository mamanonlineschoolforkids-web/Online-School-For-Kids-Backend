using Domain.Entities.Content;
using Domain.Entities.Content.Orders;
using Domain.Interfaces.Repositories.Content;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Content;

public class PaymentRepository : GenericRepository<Payment> , IPaymentRepository
{
    public PaymentRepository(MongoDbContext context) : base(context.Payments)
    {
        
    }
}
