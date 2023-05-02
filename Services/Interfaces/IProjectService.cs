using JWT_Implementation.DTOs;
using JWT_Implementation.Entities;

namespace JWT_Implementation.Services.Interfaces;

public interface IProjectService
{
    Task<Project> CreateProjectAsync(CreateProjectDto createProjectDto, string callerId);

    Task<IEnumerable<dynamic>> GetAllProjectsAsync(string name);

    Task<IEnumerable<dynamic>> GetUserProjectsAsync(string callerid);

    Task<int> UpdateProjectAsync(UpdateProjectDto updateProjectDto, int id, string callerId);

    Task<int> DeleteProjectAsync(int id, string callerId);

    Task<int> AddUserToProjectAsync(int id, string callerId, int projectId);

    Task<int> UpdateUserProjectRoleAsync(string callerId, int projectId, UpdateProjectRoleDto updateProjectRoleDto);

}