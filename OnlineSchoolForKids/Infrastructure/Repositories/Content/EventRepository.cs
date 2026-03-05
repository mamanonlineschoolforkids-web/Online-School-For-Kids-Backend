using Domain.Entities.Content.Calendar;
using Domain.Interfaces.Repositories.Content;
using Infrastructure.Data;

namespace Infrastructure.Repositories.Content
{
    public class EventRepository:GenericRepository<Event>, IEventRepository
    {
        public EventRepository(MongoDbContext context) : base(context.Events)
        {

        }
    }
}
