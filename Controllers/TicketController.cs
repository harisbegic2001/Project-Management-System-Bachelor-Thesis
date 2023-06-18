using System.Security.Claims;
using JWT_Implementation.DTOs;
using JWT_Implementation.Entities;
using JWT_Implementation.Exceptions;
using JWT_Implementation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWT_Implementation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;
    
    
    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }


    [HttpPost("{projectId}")]
    [Authorize]
    public async Task<ActionResult<ReadTicketDto>> CreateTicketAsync(CreateTicketDto createTicketDto,  int projectId)
    {
        try
        {
            var createdTicket = await _ticketService.CreateTicketAsync(createTicketDto, User.FindFirst(ClaimTypes.NameIdentifier)!.Value, projectId);

            return Ok(createdTicket);
        }
        catch (UserNotOnProjectException e)
        {
            return BadRequest(e.Message);
        }
        catch (ProjectNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (TicketPriorityDoesNotExistException e)
        {
            return BadRequest(e.Message);
        }
        catch (TicketTaskDoesNotExistException e)
        {
            return BadRequest(e.Message);
        }


    }


    [HttpGet("GetTicketsOnProject/{projectIdentifier}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReadTicketDto>>> GetAllTicketsOProjectAsync(int projectIdentifier)
    {
        var ticketsOnProject = await _ticketService.GetAllProjectTicketsAsync(callerId: User.FindFirst(ClaimTypes.NameIdentifier)!.Value, projectId:projectIdentifier);
        return Ok(ticketsOnProject);
    }


    [HttpGet("GetUserTickets")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReadTicketDto>>> GetAllTicketsAssignedToUserAsync()
    {

        var ticketsAssignedToUser = await _ticketService.GetAllTicketsAssignedToUserAsync(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(ticketsAssignedToUser);
    }


    [Authorize]
    [HttpPut("{ticketId}")]
    public async Task<ActionResult> UpdateTicketAsync(UpdateTicketDto updateTicketDto, int ticketId)
    {
        try
        {
            var updatedUser = await _ticketService.UpdateTicketAsync(updateTicketDto, ticketId,
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return Ok(updatedUser);
        }
        catch (TicketNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UserNotOnProjectException e)
        {
            return Unauthorized(e.Message);
        }
        catch (TicketPriorityDoesNotExistException e)
        {
            return BadRequest(e.Message);
        }
        catch (TicketTaskDoesNotExistException e)
        {
            return BadRequest(e.Message);
        }
        catch (TicketStageNotFoundException e)
        {
            return BadRequest(e.Message);
        }
    }

    [Authorize]
    [HttpDelete("{ticketId}")]
    public async Task<ActionResult<int>> DeleteTicketAsync(int ticketId)
    {
        try
        {
            var updatedUser = await _ticketService.DeleteTicketAsync(ticketId, User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return Ok(updatedUser);
        }
        catch (TicketNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UserNotOnProjectException e)
        {
            return Unauthorized(e.Message);
        }
        
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<ReadTicketDto>> GetTicektById(int id)
    {
        try
        {
            var ticket =  await _ticketService.GetTicketByIdAsync(id, User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            return Ok(ticket);

        }
        catch (UnauthorizedAccessException e)
        {
            return BadRequest(e.Message);
        }
    }


}