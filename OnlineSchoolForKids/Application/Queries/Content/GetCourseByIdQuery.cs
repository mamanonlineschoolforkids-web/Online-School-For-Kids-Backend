using AutoMapper;
using Domain.Interfaces.Repositories.Content;
using FluentValidation;
using MediatR;

namespace Application.Queries.Content
{
    public class GetCourseByIdQuery : IRequest<CourseDto>
    {
        public string CourseId { get; set; } = string.Empty;
        public string? UserId { get; set; }
    }
    public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, CourseDto>
    {
        private readonly ICourseRepository _courseRepo;
        private readonly IMapper _mapper;
        private readonly IWishListRepository _wishListRepo;
        private readonly ICartItemRepository _cartItemRepo;

        public GetCourseByIdQueryHandler(ICourseRepository courseRepo, IMapper mapper,IWishListRepository wishListRepo,ICartItemRepository cartItemRepo)
        {
            _courseRepo = courseRepo;
            _mapper = mapper;
            _wishListRepo = wishListRepo;
            _cartItemRepo = cartItemRepo;
        }
        public async Task<CourseDto?> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
        {
            // Get course by ID
            var course = await _courseRepo.GetByIdAsync(request.CourseId);

            // Return null if course not found or not published
            if (course == null || !course.IsPublished)
                return null;

            // Get wishlist and cart status if user is logged in
            var isInWishlist = false;
            var isInCart = false;

            if (!string.IsNullOrEmpty(request.UserId))
            {
                // Check if course is in user's wishlist
                var wishlist = await _wishListRepo.GetAllAsync(w =>
                    w.UserId == request.UserId && w.CourseId == request.CourseId);
                isInWishlist = wishlist.Any();

                // Check if course is in user's cart
                var cartItem = await _cartItemRepo.ExistsInCartAsync(request.UserId, request.CourseId);
                isInCart = cartItem != null;
            }

            // Map to DTO using AutoMapper
            var courseDto = _mapper.Map<CourseDto>(course);

            // Set wishlist and cart status
            courseDto.IsInWishlist = isInWishlist;
            courseDto.IsInCart = isInCart;

            return courseDto;
        }
        public class GetCourseByIdQueryValidator : AbstractValidator<GetCourseByIdQuery>
        {
            public GetCourseByIdQueryValidator()
            {
                RuleFor(x => x.CourseId)
                    .NotEmpty()
                    .WithMessage("Course ID is required");
            }
        }
    }
}

