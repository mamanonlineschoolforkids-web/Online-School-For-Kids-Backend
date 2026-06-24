using Domain.Entities.Content;
using Domain.Interfaces.Repositories.Content;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Content;

public class VideoProcessingJobRepository
        : GenericRepository<VideoProcessingJob>, IVideoProcessingJobRepository
{
    public VideoProcessingJobRepository(MongoDbContext context)
        : base(context.VideoProcessingJobs)
    {
    }
}