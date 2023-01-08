using System.Data.SqlClient;
using Dapper;
using JWT_Implementation.Constants;
using JWT_Implementation.DTOs;
using JWT_Implementation.Entities;
using JWT_Implementation.EnvironmentSettings;
using JWT_Implementation.Exceptions;
using JWT_Implementation.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace JWT_Implementation.Services;

public class ProjectService : IProjectService
{
    private readonly ConnectionStrings _options;

    public ProjectService(IOptions<ConnectionStrings> options)
    {
        _options = options.Value;
    }


    public async Task<Project> CreateProjectAsync(CreateProjectDto createProjectDto, string callerId)
    {
        using var connection = CreateSqlConnection();

        var newProject = new Project
        {
            ProjectName = createProjectDto.ProjectName,
            ProjectKey = createProjectDto.ProjectKey,
            ProjectType = createProjectDto.ProjectType,
            ProjectDescription = createProjectDto.ProjectDescription
        };

        if (!ProjectTypeConstants.AvailableProjectTypes.Contains(createProjectDto.ProjectType!))
        {
            throw new ProjectTypeNotFoundException();
        }

        var addedProject = await connection.ExecuteAsync(
            "INSERT INTO Projects (ProjectName, ProjectKey, ProjectType, ProjectDescription) values (@ProjectName, @ProjectKey, @ProjectType, @ProjectDescription)",
            newProject);


        var projectId = connection.Query<int>("SELECT TOP 1 Id FROM Projects ORDER BY Id DESC;").Single();

        var addedRelation =
            await connection.ExecuteAsync(
                "INSERT INTO UsersProjectsRelation (UserId, ProjectId) values (@UserId, @ProjectId)",
                new { @ProjectId = projectId, @UserId = Int32.Parse(callerId) });

        return newProject;
    }

    public async Task<IEnumerable<dynamic>> GetAllProjectsAsync(string name)
    {
        using var connection = CreateSqlConnection();

        var allProjects = await connection.QueryAsync($"SELECT * FROM Projects WHERE ProjectName LIKE '%{name}%'");

        return allProjects;
    }

    public async Task<IEnumerable<dynamic>> GetUserProjectsAsync(string callerid)
    {
        using var connection = CreateSqlConnection();

        var provjeraCallera = callerid;

        var singleUserProjects = await
            connection.QueryAsync(
                $"SELECT Projects.ProjectName, Projects.ProjectKey, Projects.ProjectType, Projects.ProjectDescription, Users.Username FROM Projects JOIN UsersProjectsRelation ON Projects.Id = UsersProjectsRelation.ProjectId JOIN Users ON Users.Id = UsersProjectsRelation.UserId WHERE UserId = '{callerid}'");

        return singleUserProjects;
    }

    public async Task<int> UpdateProjectAsync(UpdateProjectDto updateProjectDto, int id, string callerId)
    {
        using var connection = CreateSqlConnection();

        var checkIfProjectExists =
            await connection.QueryFirstOrDefaultAsync($"SELECT * FROM Projects WHERE Projects.Id = '{id}'");

        if (checkIfProjectExists is null)
        {
            throw new ProjectNotFoundException();
        }

      
      
        var checkIfUserOnProject = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM UsersProjectsRelation WHERE UserId = '{Int32.Parse(callerId)}' AND ProjectId = '{id}'");

        if (checkIfUserOnProject is null)
        {
            throw new UserNotOnProjectException();
        }
        
        var updateProject = await connection.ExecuteAsync(
            $"UPDATE Projects SET ProjectName = '{updateProjectDto.ProjectName}', ProjectKey = '{updateProjectDto.ProjectKey}', ProjectDescription = '{updateProjectDto.ProjectDescription}' WHERE Id = '{id}'");

        return updateProject;
    }

    public async Task<int> DeleteProjectAsync(int id, string callerId)
    {
        using var connection = CreateSqlConnection();
        
        
        //CHECKS IF PROJECT EXISTS
        var checkIfProjectExists =
            await connection.QueryFirstOrDefaultAsync($"SELECT * FROM Projects WHERE Projects.Id = '{id}'");

        if (checkIfProjectExists is null)
        {
            throw new ProjectNotFoundException();
        }
        
        //CHECKS IF THE USER THAT CALLS THE ENDPOINT IS ON THAT PROJECT
        var checkIfUserOnProject = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM UsersProjectsRelation WHERE UserId = '{Int32.Parse(callerId)}' AND ProjectId = '{id}'");

        if (checkIfUserOnProject is null)
        {
            throw new UserNotOnProjectException();
        }
        
        
        //DELETES THE DATA FROM REF TABLE
        var deletFromRefTable = await connection.ExecuteAsync($"DELETE FROM UsersProjectsRelation WHERE UserId = '{Int32.Parse(callerId)}' AND ProjectId = '{id}'");
        
        
        //DELETES THE PROJECT IF ALL REQUIREMENTS ABOVE ARE MET (IF ANY EXCEPTION WAS NOT THROWN)
        var deletingUser = await connection.ExecuteAsync($"DELETE FROM Projects WHERE Id = '{id}'");


        return deletingUser;

    }

    public async Task<int> AddUserToProjectAsync(int id, string callerId, int projectId)
    {
        using var connection = CreateSqlConnection();
        
        //CHECKS IF THE CALLER OF THE ENDPOINT IS ON THE PROJECT
        var checkIfUserOnProject = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM UsersProjectsRelation WHERE UserId = '{Int32.Parse(callerId)}' AND ProjectId = '{projectId}'");

        if (checkIfUserOnProject is null)
        {
            throw new UserNotOnProjectException("Unauthorized acess");
        }
        
        //CHECKS IF USER EXISTS
        var checkIfUserExists = await connection.QueryFirstOrDefaultAsync($"SELECT Users.Id FROM Users WHERE Id = '{id}'");
        if (checkIfUserExists is null)
        {
            throw new UserNotFoundException("User does not exist!");
        }
        
        //CHECKS IF PROJECT EXISTS //MOGUĆE DA JE VIŠKA QUERY!!
        var checkIfProjectExists = await connection.QueryFirstOrDefaultAsync($"SELECT Projects.Id FROM Projects WHERE Id = '{projectId}'");
        if (checkIfProjectExists is null)
        {
            throw new ProjectNotFoundException();
        }
        
        //CHECKS IF USER ALREADY ON THE PROJECT
        var checkIfUserAlreadyOnProject = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM UsersProjectsRelation WHERE UserId = '{id}' AND ProjectId = '{projectId}'");
        if (checkIfUserAlreadyOnProject is not null)
        {
            throw new UserAlreadyOnProjectException();
        }
        
        //ADDING USER TO PROJECT
        var addUserToProject = await connection.ExecuteAsync("INSERT INTO UsersProjectsRelation (UserId, ProjectId) values (@UserId, @ProjectId)",
            new { @ProjectId = projectId, @UserId = id });

        return addUserToProject;
    }

    private SqlConnection CreateSqlConnection()
    {
        return new SqlConnection(_options.DefaultConnection);
    }
}