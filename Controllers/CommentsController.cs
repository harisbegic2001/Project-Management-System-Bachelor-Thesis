using System.Security.Claims;
using JWT_Implementation.DTOs;
using JWT_Implementation.Exceptions;
using JWT_Implementation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWT_Implementation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    
    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [Authorize]
    [HttpPost("{ticketId}")]
    public async Task<ActionResult> CreateCommentAsync(CreateCommentDto createCommentDto, int ticketId)
    {
        try
        {  
            var createdComment = await _commentService.CreateCommentAsync(createCommentDto, ticketId, User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return Ok(createdComment);
        }
        catch (TicketNotFoundException)
        {
            return NotFound();
        }

    }

    [Authorize]
    [HttpGet("{ticketId}")]
    public async Task<ActionResult> GetCommentsByTicketAsync(int ticketId)
    {
        try
        {
            var getCommentsbyTicket = await _commentService.GetCommentsByTicketAsync(ticketId);

            return Ok(getCommentsbyTicket);
        }
        catch (TicketNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize]
    [HttpPut("{commentId}")]
    public async Task<ActionResult> UpdateCommentAsync(int commentId, UpdateCommentDto updateCommentDto)
    {
        try
        {
            var updatedComment = await _commentService.UpdateCommentAsync(User.FindFirst(ClaimTypes.NameIdentifier)!.Value, commentId, updateCommentDto);
            return Ok(updatedComment);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }


    [Authorize]
    [HttpDelete("{commentId}")]
    public async Task<ActionResult> DeleteCommentAsync(int commentId)
    {
        try
        {
            var deletedComment = await _commentService.DeleteCommentAsync(User.FindFirst(ClaimTypes.NameIdentifier)!.Value, commentId);
            return Ok(deletedComment);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }
}