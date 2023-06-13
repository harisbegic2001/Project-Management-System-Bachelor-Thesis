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
public class TicketStagesController : ControllerBase
{
    private readonly ITicketStageService _ticketStageService;


    public TicketStagesController(ITicketStageService ticketStageService)
    {
        _ticketStageService = ticketStageService;
    }

    [HttpPost("{projectId}")]
    [Authorize]
    public async Task<ActionResult> CreateTicketStageAsync(CreateTicketStageDto createTicketStageDto, int projectId)
    {
        try
        {
            await _ticketStageService.CreateTicketStageAsync(createTicketStageDto, projectId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            return Ok();
        }
        catch (UserNotOnProjectException e)
        {
            return Unauthorized(e.Message);
        }        
        
    }

    /*[HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ReadTicekStageDto>> UpdateTicketStageAsync(UpdateTicketStageDto updateTicketStageDto, int id)
    {
        try
        {
            var updatedTicketStage =  await _ticketStageService.UpdateTicketStageAsync(updateTicketStageDto, id,
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            return Ok(updatedTicketStage);
        }
        catch (TicketStageNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UserNotOnProjectException e)
        {
            return Unauthorized(e.Message);
        }  
    }*/

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteTicketStageAsync(int id)
    {
        try
        {
            await _ticketStageService.DeleteTicekStageAsync(id);
            return NoContent();
        }
        catch (TicketStageNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (NotEnoughStagesException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{projectId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TicketStage>>> GetTicketStagesForProjectAsync(int projectId)
    {
        try
        {
            var ticketStagesToReturn = await _ticketStageService.GetTicketStagesOnProjectAsync(projectId);

            return Ok(ticketStagesToReturn);

        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPut("{ticketId}")]
    [Authorize]
    public async Task<ActionResult> UpdateTicketCurrentStageAsync(int ticketId,  UpdateTicketCurrentStageDto stageName)
    {
        try
        {
            await _ticketStageService.UpdateTicketCurrentStageAsync(ticketId, stageName);
            return Ok();
        }
        catch (TicketStageNotFoundException e)
        {
            return BadRequest(e.Message);
        }
    }
}