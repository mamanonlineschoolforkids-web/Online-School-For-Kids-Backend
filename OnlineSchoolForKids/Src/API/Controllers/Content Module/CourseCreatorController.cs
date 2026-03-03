using Application.Commands;
using Application.Commands.Course;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Application.Commands.Course.CreateSectionHandler;
using static Application.Commands.UpdateCourseHandler;
using CreateCourseDto = Application.Commands.CreateCourseDto;


namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ContentCreator")]
    public class CourseCreatorController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CourseCreatorController> _logger;

        public CourseCreatorController(
            IMediator mediator,
            ILogger<CourseCreatorController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("courses")]
        public async Task<IActionResult> CreateCourse(
            [FromBody] CreateCourseDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();
                var command = new CreateCourseCommand { Dto = dto, InstructorId = userId };
                var result = await _mediator.Send(command, cancellationToken);

                if (result == null)
                    return BadRequest(new { message = "Failed to create course", success = false });

                return Ok(new
                {
                    data = result,
                    message = "Course created successfully",
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }
        [HttpPut("courses/{courseId}")]
        public async Task<IActionResult> UpdateCourse(
           string courseId,
           [FromBody] UpdateCourseDto dto,
           CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();
                var command = new UpdateCourseCommand
                {
                    CourseId = courseId,
                    InstructorId = userId,
                    Dto = dto
                };

                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return NotFound(new { message = "Course not found", success = false });

                return Ok(new { message = "Course updated successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }
        [HttpDelete("courses/{courseId}")]
        public async Task<IActionResult> DeleteCourse(
            string courseId,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();
                var command = new DeleteCourseCommand { CourseId = courseId,InstructorId=userId };
                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return NotFound(new { message = "Course not found", success = false });

                return Ok(new { message = "Course deleted successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }
        [HttpPost("courses/{courseId}/publish")]
        public async Task<IActionResult> PublishCourse(
           string courseId,
           [FromBody] PublishCourseRequest request,
           CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();
                var command = new PublishCourseCommand
                {
                    CourseId = courseId,
                    InstructorId = userId,
                    Publish = request.Publish
                };

                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return NotFound(new { message = "Course not found", success = false });

                return Ok(new
                {
                    message = request.Publish ? "Course published" : "Course unpublished",
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing course");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }
        [HttpPost("sections")]
        public async Task<IActionResult> CreateSection(
           [FromBody] CreateSectionDto dto,
           CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();
                var command = new CreateSectionCommand { Dto = dto ,InstructorId=userId};
                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return BadRequest(new { message = "Failed to create section", success = false });

                return Ok(new { message = "Section created successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating section");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }
        [HttpPut("sections/{courseId}/{sectionId}")]
        public async Task<IActionResult> UpdateSection(
                  string courseId,
                  string sectionId,
                  [FromBody] UpdateSectionDto dto,
                  CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();
                var command = new UpdateSectionCommand
                {
                    CourseId = courseId,
                    SectionId = sectionId,
                    InstructorId = userId,
                    Dto = dto
                };

                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return NotFound(new { message = "Section not found", success = false });

                return Ok(new { message = "Section updated successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating section");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }
        [HttpDelete("sections/{courseId}/{sectionId}")]
        public async Task<IActionResult> DeleteSection(
            string courseId,
            string sectionId,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();
                var command = new DeleteSectionCommand
                {
                    CourseId = courseId,
                    SectionId = sectionId,
                    InstructorId = userId
                };

                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return NotFound(new { message = "Section not found", success = false });

                return Ok(new { message = "Section deleted successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting section");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }
        [HttpPost("lessons")]
        public async Task<IActionResult> CreateLesson(
           [FromBody] CreateLessonDto dto,
           CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();
                var command = new CreateLessonCommand { Dto = dto ,InstructorId = userId};
                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return BadRequest(new { message = "Failed to create lesson", success = false });

                return Ok(new { message = "Lesson created successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lesson");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }

        [HttpPut("lessons/{courseId}/{sectionId}/{lessonId}")]
        public async Task<IActionResult> UpdateLesson(
            string courseId,
            string sectionId,
            string lessonId,
            [FromBody] UpdateLessonDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();
                var command = new UpdateLessonCommand
                {
                    InstructorId = userId,
                    CourseId = courseId,
                    SectionId = sectionId,
                    LessonId = lessonId,
                    Dto = dto
                };

                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return NotFound(new { message = "Lesson not found", success = false });

                return Ok(new { message = "Lesson updated successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lesson");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }

        [HttpDelete("lessons/{courseId}/{sectionId}/{lessonId}")]
        public async Task<IActionResult> DeleteLesson(
            string courseId,
            string sectionId,
            string lessonId,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();
                var command = new DeleteLessonCommand
                {
                    InstructorId = userId,
                    CourseId = courseId,
                    SectionId = sectionId,
                    LessonId = lessonId
                };

                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return NotFound(new { message = "Lesson not found", success = false });

                return Ok(new { message = "Lesson deleted successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lesson");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }
        [HttpPost("materials")]
        public async Task<IActionResult> AddMaterial(
           [FromBody] AddMaterialDto dto,
           CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();
                var command = new AddMaterialCommand { Dto = dto,InstructorId = userId };
                var result = await _mediator.Send(command, cancellationToken);

                if (!result)
                    return BadRequest(new { message = "Failed to add material", success = false });

                return Ok(new { message = "Material added successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding material");
                return StatusCode(500, new { message = "An error occurred", success = false });
            }
        }

    }
}