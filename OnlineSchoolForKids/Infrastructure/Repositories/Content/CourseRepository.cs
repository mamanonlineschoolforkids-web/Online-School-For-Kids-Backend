using Domain.Entities.Content;
using Domain.Interfaces.Repositories.Content;
using Infrastructure.Data;

namespace Infrastructure.Repositories.Content
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(MongoDbContext context) : base(context.Courses)
        {
            
        }

    }
}
