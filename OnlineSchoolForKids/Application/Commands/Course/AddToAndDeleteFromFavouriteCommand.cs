using Domain.Entities.Content;
using Domain.Interfaces.Repositories.Content;
using FluentValidation;
using MediatR;
using MongoDB.Bson;

namespace Application.Commands
{
    public class AddToFavouriteCommand : IRequest<AddToFavouriteResponse>
    {
        public string CourseId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class AddToFavouriteResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FavouriteId { get; set; }
    }
    public class AddToFavouriteCommandHandler : IRequestHandler<AddToFavouriteCommand, AddToFavouriteResponse>
    {


        private readonly ICourseRepository _courseRepo;
        private readonly IWishListRepository _wishRepo;

        public AddToFavouriteCommandHandler(
            ICourseRepository courseRepo, IWishListRepository wishRepo)
        {


            _courseRepo = courseRepo;
            _wishRepo = wishRepo;
        }

        public async Task<AddToFavouriteResponse> Handle(AddToFavouriteCommand request, CancellationToken cancellationToken)
        {

            // Check if course exists and is published
            var course = await _courseRepo.GetByIdAsync(request.CourseId);

            if (course == null)
            {
                return new AddToFavouriteResponse
                {
                    Success = false,
                    Message = "Course not found"
                };
            }

            if (!course.IsPublished)
            {
                return new AddToFavouriteResponse
                {
                    Success = false,
                    Message = "Course is not available"
                };
            }

            // Check if already in favourites
            var existingFavourite = await _wishRepo.GetAllAsync(w =>
                w.UserId == request.UserId && w.CourseId == request.CourseId);

            if (existingFavourite.Any())
            {
                return new AddToFavouriteResponse
                {
                    Success = false,
                    Message = "Course already in favourites",
                    FavouriteId = existingFavourite.First().Id
                };
            }

            // Create new favourite entry
            var favourite = new Wishlist
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UserId = request.UserId,
                CourseId = request.CourseId,
                CreatedAt = DateTime.UtcNow
            };

            await _wishRepo.CreateAsync(favourite);

            return new AddToFavouriteResponse
            {
                Success = true,
                Message = "Course added to favourites successfully",
                FavouriteId = favourite.Id
            };
        }
        public class AddToFavouriteCommandValidator : AbstractValidator<AddToFavouriteCommand>
        {
            public AddToFavouriteCommandValidator()
            {
                RuleFor(x => x.CourseId)
                    .NotEmpty()
                    .WithMessage("Course ID is required");

                RuleFor(x => x.UserId)
                    .NotEmpty()
                    .WithMessage("User ID is required");
            }
        }
   
    }
    public class DeleteFromFavouriteCommand : IRequest<DeleteFromFavouriteResponse>
    {
        public string CourseId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class DeleteFromFavouriteResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public class DeleteFromFavouriteCommandHandler : IRequestHandler<DeleteFromFavouriteCommand, DeleteFromFavouriteResponse>
    {

        private readonly IWishListRepository _wishRepo;


        public DeleteFromFavouriteCommandHandler(
             IWishListRepository wishRepo)
        {
            _wishRepo = wishRepo;
        }

        public async Task<DeleteFromFavouriteResponse> Handle(DeleteFromFavouriteCommand request, CancellationToken cancellationToken)
        {

            // Find favourite entry
            var favourite = (await _wishRepo.GetAllAsync(w =>
                w.UserId == request.UserId && w.CourseId == request.CourseId))
                .FirstOrDefault();

            if (favourite == null)
            {
                return new DeleteFromFavouriteResponse
                {
                    Success = false,
                    Message = "Course not found in favourites"
                };
            }

            // Delete from favourites
            await _wishRepo.DeleteAsync(favourite.Id);
            return new DeleteFromFavouriteResponse
            {
                Success = true,
                Message = "Course deleted from favourites successfully"
            };


        }
        public class DeleteFromFavouriteCommandValidator : AbstractValidator<DeleteFromFavouriteCommand>
        {
            public DeleteFromFavouriteCommandValidator()
            {
                RuleFor(x => x.CourseId)
                    .NotEmpty()
                    .WithMessage("Course ID is required");

                RuleFor(x => x.UserId)
                    .NotEmpty()
                    .WithMessage("User ID is required");
            }
        }
        public class AddToFavouriteRequest
        {
            public string CourseId { get; set; } = string.Empty;
        }

    }
}



