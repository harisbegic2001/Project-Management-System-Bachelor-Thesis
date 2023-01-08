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
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;

    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Project>> CreateProjectAsync(CreateProjectDto createProjectDto)
    {
        var newProject = await _projectService.CreateProjectAsync(createProjectDto,
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        return Ok(newProject);
    }

    [HttpGet("getAllProjects")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<IEnumerable<Project>>> GetAllProjectsAsync(string name)
    {
        var projects = await _projectService.GetAllProjectsAsync(name);
        
        
        return Ok(projects);
    }

    [HttpGet("getUserProjects")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Project>>> GetUserProjectsAsync()
    {
        var userProjects = await _projectService.GetUserProjectsAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        return Ok(userProjects);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProjectAsync(UpdateProjectDto updateProjectDto, int id)
    {
        try
        {
            var updateProject = await _projectService.UpdateProjectAsync(updateProjectDto, id,
                callerId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            return Ok(updateProject);
        }
        catch (UserNotOnProjectException e)
        {
            return Unauthorized("User not on the project");
        }
        catch (ProjectNotFoundException e)
        {
            return NotFound("Project Not Found");
        }
            

    }


    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteProjectAsync(int id)
    {
        try
        {
            var deleteProject = await _projectService.DeleteProjectAsync(id, callerId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            return Ok(deleteProject);
        }
        catch (ProjectNotFoundException e)
        {
            return NotFound("Project Not Found");
        }
        catch (UserNotOnProjectException e)
        {
            return Unauthorized("User not on the project");
        }
        
    }


    [HttpGet]
    [Authorize]
    public string? GetIdClaim()
    {
        //var identity = HttpContext.User.Identity as ClaimsIdentity;
        //IEnumerable<Claim> claim = identity.Claims;

        //var idClaim = claim.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;

        // var readUSer = new ReadUserDto();
        // var token = readUSer.Token;
        //var handler = new JwtSecurityTokenHandler().ReadJwtToken(token: "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTUxMiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJIYXJpc0JlZ2nEhyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZXhwIjoxNjcxODIzMDUyfQ.LCuP0X6iTQWHHGL3GJ0GUnAFc4IT9nE4Z8Ke4dMFb-d1uZzXMfSl4pg6jeP35j-37iarebPsNkdQutZYP1wquw");
        //var jti = handler.Claims.FirstOrDefault(claimm => claimm.Type == ClaimTypes.NameIdentifier)?.Value;

        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //return HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        return id;
    }

    [HttpPost("AddUserToProjectAsync")]
    [Authorize]
    public async Task<ActionResult> AddUserToProjectAsync(int userId, int projectIdentifier)
    {
        try
        {
            var addUserToProject = await _projectService.AddUserToProjectAsync(id:userId, callerId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value!, projectId:projectIdentifier);
            return Ok(addUserToProject);

        }
        catch (UserNotOnProjectException e)
        {
            return Unauthorized(e.Message);
        }
        catch (UserNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ProjectNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UserAlreadyOnProjectException e)
        {
            return BadRequest(e.Message);
        }
    }

}