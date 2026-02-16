using Application.Commands.Profile;
using Application.Commands.Profile.Parents;
using Application.DTOs.Profile;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ParentController : ControllerBase
{
    private readonly IMediator _mediator;

    public ParentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search for a child account by email
    /// </summary>
    [HttpGet("search-child")]
    [ProducesResponseType(typeof(SearchChildDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchChild([FromQuery] string email)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var query = new SearchChildCommand
            {
                ParentUserId = userId,
                Email = email
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Send link invitation to an existing child account
    /// </summary>
    [HttpPost("send-invite/{childId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendChildInvite(string childId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var command = new SendChildInviteCommand
            {
                ParentUserId = userId,
                ChildId = childId
            };

            await _mediator.Send(command);

            return Ok(new { message = "Invitation sent successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new child account and link it to parent
    /// </summary>
    [HttpPost("create-child")]
    [ProducesResponseType(typeof(ChildDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAndLinkChild([FromBody] CreateChildDto createChildDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var command = new CreateAndLinkChildCommand
            {
                ParentUserId = userId,
                FullName = createChildDto.FullName,
                Email = createChildDto.Email,
                Password = createChildDto.Password,
                ConfirmPassword = createChildDto.ConfirmPassword,
                DateOfBirth = createChildDto.DateOfBirth,
                Country = createChildDto.Country
            };

            var childDto = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetLinkedChildren), childDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    ///// <summary>
    ///// Get child's course progress and learning stats
    ///// </summary>
    //[HttpGet("children/{childId}/progress")]
    //[ProducesResponseType(typeof(ChildProgressDto), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //[ProducesResponseType(StatusCodes.Status403Forbidden)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //public async Task<IActionResult> GetChildProgress(string childId)
    //{
    //    try
    //    {
    //        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //        if (userId == null) return Unauthorized();

    //        var query = new GetChildProgressQuery
    //        {
    //            ParentUserId = userId,
    //            ChildId = childId
    //        };

    //        var progress = await _mediator.Send(query);

    //        return Ok(progress);
    //    }
    //    catch (KeyNotFoundException ex)
    //    {
    //        return NotFound(new { message = ex.Message });
    //    }
    //    catch (UnauthorizedAccessException)
    //    {
    //        return Forbid();
    //    }
    //}

    /// <summary>
    /// Get linked children for current parent
    /// </summary>
    [HttpGet("children")]
    [ProducesResponseType(typeof(List<ChildDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLinkedChildren()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var query = new GetLinkedChildrenCommand { UserId = userId };
            var children = await _mediator.Send(query);

            return Ok(children);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Remove/unlink a child from parent's account
    /// </summary>
    [HttpDelete("children/{childId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveChild(string childId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var command = new RemoveChildCommand
            {
                UserId = userId,
                ChildId = childId
            };

            await _mediator.Send(command);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Get notification preferences for a specific child
    /// </summary>
    /// <param name="childId">Child user ID</param>
    /// <returns>Notification preferences for the child</returns>
    [HttpGet("children/{childId}/notifications")]
    [ProducesResponseType(typeof(NotificationPreferences), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChildNotificationPreferences(string childId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var query = new GetChildNotificationPreferencesCommand
            {
                ParentUserId = userId,
                ChildId = childId
            };

            var preferences = await _mediator.Send(query);
            return Ok(preferences);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update notification preferences for a specific child
    /// </summary>
    /// <param name="childId">Child user ID</param>
    /// <param name="preferences">Updated notification preferences</param>
    /// <returns>Updated notification preferences</returns>
    [HttpPut("children/{childId}/notifications")]
    [ProducesResponseType(typeof(NotificationPreferences), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateChildNotificationPreferences(
        string childId,
        [FromBody] NotificationPreferences preferences)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var command = new UpdateChildNotificationPreferencesCommand
            {
                ParentUserId = userId,
                ChildId = childId,
                Preferences = preferences
            };

            var updatedPreferences = await _mediator.Send(command);
            return Ok(updatedPreferences);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}


