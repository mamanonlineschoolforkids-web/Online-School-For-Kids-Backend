using AutoMapper;
using Domain.Entities;
using Domain.Entities.Users;
using Domain.Enums.Content;
using Domain.Interfaces.Repositories.Content;
using Domain.Interfaces.Repositories.Users;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands
{
    public class CreateCourseCommand : IRequest<CourseDto>
    {
        public string CreatorId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public CourseLevel Level { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int DurationHours { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Language { get; set; } = "English";

    }
    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, CourseDto>
    {
        private readonly ICourseRepository _courseRepo;
        private readonly IUserRepository _userRepo;

        public CreateCourseCommandHandler(ICourseRepository courseRepo, IUserRepository userRepo)
        {
            _courseRepo = courseRepo;
            _userRepo = userRepo;
        }

        public async Task<CourseDto> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            var instructor = await _userRepo.GetByIdAsync(request.CreatorId);

            if (instructor == null)
                throw new UnauthorizedAccessException("User is not an instructor");

            var course = new Domain.Entities.Content.Course
            {
                Id = Guid.NewGuid().ToString(),
                Title = request.Title,
                Description = request.Description,
                InstructorId = request.CreatorId,
                CategoryId = request.CategoryId,
                Level = request.Level,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                DurationHours = request.DurationHours,
                ThumbnailUrl = request.ThumbnailUrl,
                Language = request.Language,
                Rating = 0,
                TotalStudents = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsPublished = false,
                IsFeatured = false
            };

            await _courseRepo.CreateAsync(course);


            return MapToCourseDto(course, instructor);
        }

        private CourseDto MapToCourseDto(Domain.Entities.Content.Course course, User instructor)
        {
            return new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                InstructorName = instructor?.FullName ?? "Unknown",
                InstructorId = course.InstructorId,
                CategoryName = course.Category?.Name ?? "Unknown",
                CategoryId = course.CategoryId,
                Level = course.Level,
                LevelDisplay = course.Level.ToString(),
                Price = course.Price,
                DiscountPrice = course.DiscountPrice,
                Rating = course.Rating,
                TotalStudents = course.TotalStudents,
                DurationHours = course.DurationHours,
                ThumbnailUrl = course.ThumbnailUrl,
                Language = course.Language,
                IsFeatured = course.IsFeatured,
                IsInWishlist = false,
                IsInCart = false

            };
        }

        public class UpdateCourseCommand : IRequest<UpdateCourseResponse>
        {
            public string CreatorId { get; set; } = string.Empty;
            public string CourseId { get; set; } = string.Empty;
            public string InstructorId { get; set; } = string.Empty;

            [Required(ErrorMessage = "Title is required")]
            [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Description is required")]
            [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
            public string Description { get; set; } = string.Empty;

            [Required(ErrorMessage = "Category is required")]
            public string CategoryId { get; set; } = string.Empty;

            [Required(ErrorMessage = "Level is required")]
            public CourseLevel Level { get; set; }

            [Required(ErrorMessage = "Price is required")]
            [Range(0, 10000, ErrorMessage = "Price must be between 0 and 10000")]
            public decimal Price { get; set; }

            [Range(0, 10000, ErrorMessage = "Discount price must be between 0 and 10000")]
            public decimal? DiscountPrice { get; set; }

            [Required(ErrorMessage = "Duration is required")]
            [Range(1, 500, ErrorMessage = "Duration must be between 1 and 500 hours")]
            public int DurationHours { get; set; }

            public string ThumbnailUrl { get; set; } = string.Empty;

            [Required(ErrorMessage = "Language is required")]
            [StringLength(50)]
            public string Language { get; set; } = "English";

            public bool IsPublished { get; set; }
            public bool IsFeatured { get; set; }
        }
        public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, UpdateCourseResponse>
        {

            private readonly IMapper _mapper;
            private readonly ICourseRepository _courseRepo;

            public UpdateCourseCommandHandler(

                IMapper mapper
               , ICourseRepository courseRepo)
            {

                _mapper = mapper;
                _courseRepo = courseRepo;
            }
            public async Task<UpdateCourseResponse> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
            {

                // Get the course
                var course = await _courseRepo.GetByIdAsync(request.CourseId);
                if (course == null || course.IsDeleted || course.InstructorId != request.CreatorId)
                {
                    throw new InvalidOperationException("Course not found or access denied.");
                }
                // Validate discount price
                if (request.DiscountPrice.HasValue && request.DiscountPrice.Value > request.Price)
                {
                    return new UpdateCourseResponse
                    {
                        Success = false,
                        Message = "Discount price cannot be greater than regular price"
                    };
                }

                // Update course properties
                course.Title = request.Title;
                course.Description = request.Description;
                course.CategoryId = request.CategoryId;
                course.Level = request.Level;
                course.Price = request.Price;
                course.DiscountPrice = request.DiscountPrice;
                course.DurationHours = request.DurationHours;
                course.Language = request.Language;
                course.IsPublished = request.IsPublished;
                course.IsFeatured = request.IsFeatured;
                course.UpdatedAt = DateTime.UtcNow;

                // Only update thumbnail if provided
                if (!string.IsNullOrWhiteSpace(request.ThumbnailUrl))
                {
                    course.ThumbnailUrl = request.ThumbnailUrl;
                }

                // Save changes
                await _courseRepo.UpdateAsync(request.CourseId, course);
                //await _unitOfWork.SaveChangesAsync();

                // Map to DTO
                var courseDto = _mapper.Map<CourseDto>(course);

                return new UpdateCourseResponse
                {
                    Success = true,
                    Message = "Course updated successfully",
                    Course = courseDto
                };
            }
        }
    }

}
public class UpdateCourseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CourseDto? Course { get; set; }
}

// Delete Course (Soft Delete)
public class DeleteCourseCommand : IRequest<bool>
    {
        public string CreatorId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
    }
public class CourseDto : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public CourseLevel Level { get; set; }
    public string LevelDisplay { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal Rating { get; set; }
    public int TotalStudents { get; set; }
    public int DurationHours { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsInWishlist { get; set; }
    public bool IsInCart { get; set; }
}
