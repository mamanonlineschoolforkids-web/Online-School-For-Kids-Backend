using Domain.Entities.Content.Progress;
using Domain.Interfaces.Repositories.Content;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Application.Commands.Course
{
    public class AddMaterialCommand : IRequest<bool>
    {
        public string InstructorId { get; set; } = string.Empty;
        public AddMaterialDto Dto { get; set; } = new();
    }

    public class AddMaterialHandler : IRequestHandler<AddMaterialCommand, bool>
    {
        private readonly ICourseRepository _courseRepo;
        private readonly ILogger<AddMaterialHandler> _logger;

        public AddMaterialHandler(
            ICourseRepository courseRepo,
            ILogger<AddMaterialHandler> logger)
        {
            _courseRepo = courseRepo;
            _logger = logger;
        }

        public async Task<bool> Handle(AddMaterialCommand request, CancellationToken ct)
        {
            try
            {
                var course = await _courseRepo.GetByIdAsync(request.Dto.CourseId, ct);
                if (course == null) return false;

                var section = course.Sections.FirstOrDefault(s => s.Id == request.Dto.SectionId);
                if (section == null) return false;

                var lesson = section.Lessons.FirstOrDefault(l => l.Id == request.Dto.LessonId);
                if (lesson == null) return false;

                var material = new Material
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = request.Dto.Title,
                    Type = request.Dto.Type,
                    Url = request.Dto.Url,
                    FileSize = request.Dto.FileSize
                };

                lesson.Materials.Add(material);
                course.UpdatedAt = DateTime.UtcNow;

                await _courseRepo.UpdateAsync(course.Id, course, ct);

                _logger.LogInformation("Material added: {MaterialId} to Lesson {LessonId}", material.Id, lesson.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding material");
                return false;
            }
        }
    }

    public class AddMaterialDto
    {
        public string CourseId { get; set; } = string.Empty;
        public string SectionId { get; set; } = string.Empty;
        public string LessonId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}


