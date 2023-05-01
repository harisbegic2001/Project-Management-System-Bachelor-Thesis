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

public class TicketService : ITicketService
{

    private readonly ConnectionStrings _options;
    
    public TicketService(IOptions<ConnectionStrings> options)
    {
        _options = options.Value;
    }
    
    
    public async Task<ReadTicketDto> CreateTicketAsync(CreateTicketDto createTicketDto, string callerId, int projectId)
    {
        using var connection = CreateSqlConnection();
        
        //CHECKS IF PROJECT EXISTS
        var checkIfProjectExists = await connection.QueryFirstOrDefaultAsync($"SELECT Projects.Id FROM Projects WHERE Id = '{projectId}'");

        if (checkIfProjectExists is null)
        {
            throw new ProjectNotFoundException("Project does not exist");
        }

        //CHECKS IF USER IS ON THE SPECIFIED PROJECT
        var checkIfUserOnProject = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM UsersProjectsRelation WHERE UserId = '{Int32.Parse(callerId)}' AND ProjectId = '{projectId}'");
        if (checkIfUserOnProject is null)
        {
            throw new UserNotOnProjectException("Unauthorized acess");
        }

        if (!TicketConstants.TicketPriority.Contains(createTicketDto.TicketPriority!))
        {
            throw new TicketPriorityDoesNotExistException("Priority Not Found!");
        }

        if (!TicketConstants.TicketType.Contains(createTicketDto.TicketType!))
        {
            throw new TicketTaskDoesNotExistException("Task Not Found!");
        }


        var creatorUserName = await connection.QueryFirstOrDefaultAsync<string>($"SELECT Users.Username FROM Users WHERE Id = '{Int32.Parse(callerId)}'");
        
        var projectName = await connection.QueryFirstOrDefaultAsync<string>($"SELECT Projects.ProjectName FROM Projects WHERE Id = '{projectId}'");

        // LOGIKA ZA IMENOVANJE TICKETA
        
        var projectKey = await connection.QueryFirstOrDefaultAsync<string>($"SELECT Projects.ProjectKey FROM Projects WHERE Projects.Id = '{projectId}'");

        var checkIfProjectHasTickets = await connection.QueryFirstOrDefaultAsync<string>($"SELECT Id FROM Tickets WHERE ProjectId = '{projectId}'");

        var alternativeApproach = await connection.QueryFirstOrDefaultAsync<string>($"SELECT TOP 1 TicketKey FROM Tickets WHERE ProjectId = '{projectId}' ORDER BY Id DESC ");

        
        var numberToIncrement =  alternativeApproach is null ? 0 : Convert.ToInt32(alternativeApproach.Substring(alternativeApproach.LastIndexOf("-") + 1));

        var finalNumber = numberToIncrement + 1;
        
        var ticketName = checkIfProjectExists is null ? $"{projectKey}-1" : $"{projectKey}-{finalNumber}";
        
        // KRAJ LOGIKE ZA IMENOVANJE TICKETA
        var newTicket = new Ticket
        {
            TicketName = createTicketDto.TicketName,
            TicketKey = ticketName,
            TicketDescription = createTicketDto.TicketDescription,
            TicketPriority = createTicketDto.TicketPriority,
            TicketTask = createTicketDto.TicketType,
            TicketReporter = creatorUserName,
            UserId = createTicketDto.Asignee,
            ProjectId = projectId
        };


        var createTicket = await connection.ExecuteAsync("INSERT INTO Tickets (TicketName, TicketKey, TicketDescription, TicketPriority, TicketType, TicketReporter, UserId, ProjectId) VALUES (@TicketName, @TicketKey, @TicketDescription, @TicketPriority, @TicketTask, @TicketReporter, @UserId, @ProjectId)", newTicket);

        var readTicket = new ReadTicketDto
        {
            TicketName = newTicket.TicketName,
            TicketDescription = newTicket.TicketDescription,
            TicketPriority = newTicket.TicketPriority,
            TicketTask = newTicket.TicketTask,
            TicketReporter = newTicket.TicketReporter,
            TicketKey = newTicket.TicketKey,
            TicketProject = projectName
        };

        return readTicket;

    }

    public async Task<IEnumerable<ReadTicketDto>> GetAllProjectTicketsAsync(int projectId, string callerId)
    {
        using var connection = CreateSqlConnection();
        
        //CHECKS IF PROJECT EXISTS
        var checkIfProjectExists = await connection.QueryFirstOrDefaultAsync($"SELECT Projects.Id FROM Projects WHERE Id = '{projectId}'");

        if (checkIfProjectExists is null)
        {
            throw new ProjectNotFoundException("Project does not exist");
        }

        //CHECKS IF USER IS ON THE SPECIFIED PROJECT
        var checkIfUserOnProject = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM UsersProjectsRelation WHERE UserId = '{Int32.Parse(callerId)}' AND ProjectId = '{projectId}'");
        if (checkIfUserOnProject is null)
        {
            throw new UserNotOnProjectException("Unauthorized acess");
        }

        var ticketsOnProject = await connection.QueryAsync<ReadTicketDto>($"SELECT * FROM Tickets WHERE Tickets.ProjectId = '{projectId}'");


        return ticketsOnProject;
    }

    public async Task<IEnumerable<dynamic>> GetAllTicketsAssignedToUserAsync(string callerId)
    {
        using var connection = CreateSqlConnection();
        
        var ticketsOnProject = await connection.QueryAsync($"SELECT Tickets.TicketName, Tickets.TicketKey, Tickets.TicketDescription, Tickets.TicketPriority, Tickets.TicketTask, Tickets.TicketReporter, Projects.ProjectName  FROM Tickets JOIN Projects ON Tickets.ProjectId = Projects.Id WHERE Tickets.UserId = '{Int32.Parse(callerId)}'");

        return ticketsOnProject;
    }

    public async Task<int> UpdateTicketAsync(UpdateTicketDto updateTicketDto, int ticketId, string callerId)
    {
        using var connection = CreateSqlConnection();

        var chechIfTicketExists = await connection.QueryFirstOrDefaultAsync<int?>($"SELECT Tickets.ProjectId FROM Tickets WHERE Tickets.Id = '{ticketId}'");

        if (chechIfTicketExists is null)
        {
            throw new TicketNotFoundException("Ticket does not exist");
        }
        
        //CHECK IF THE CALLER IS ON PROJECT
        var checkIfCallerOnProject = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM UsersProjectsRelation WHERE UserId = '{Int32.Parse(callerId)}' AND ProjectId = '{chechIfTicketExists}'");
        if (checkIfCallerOnProject is null)
        {
            throw new UserNotOnProjectException("Unauthorized");
        }
        
        //VALIDATION STUFF
        if (!TicketConstants.TicketPriority.Contains(updateTicketDto.TicketPriority!))
        {
            throw new TicketPriorityDoesNotExistException();

        }

        if (!TicketConstants.TicketType.Contains(updateTicketDto.TicketType!))
        {
            throw new TicketTaskDoesNotExistException();
        }
        

        var updatedTicket = await connection.ExecuteAsync($"UPDATE Tickets SET TicketName = '{updateTicketDto.TicketName}', TicketDescription = '{updateTicketDto.TicketDescription}', TicketPriority = '{updateTicketDto.TicketPriority}', TicketType = '{updateTicketDto.TicketType}', TicketReporter = '{updateTicketDto.TicketReporter}', UserId = '{updateTicketDto.UserId}' WHERE Id = '{ticketId}'");

        return updatedTicket;

    }

    public async Task<int> DeleteTicketAsync(int ticketId, string callerId)
    {
        using var connection = CreateSqlConnection();
        
        var chechIfTicketExists = await connection.QueryFirstOrDefaultAsync<int?>($"SELECT Tickets.ProjectId FROM Tickets WHERE Tickets.Id = '{ticketId}'");

        if (chechIfTicketExists is null)
        {
            throw new TicketNotFoundException("Ticket does not exist");
        }
        
        //CHECK IF THE CALLER IS ON PROJECT
        var checkIfCallerOnProject = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM UsersProjectsRelation WHERE UserId = '{Int32.Parse(callerId)}' AND ProjectId = '{chechIfTicketExists}'");
        if (checkIfCallerOnProject is null)
        {
            throw new UserNotOnProjectException("Unauthorized");
        }

        var deletedProject = await connection.ExecuteAsync($"DELETE FROM Tickets WHERE Id = '{ticketId}'");

        return deletedProject;

    }

    public Task<dynamic> GetTicketByIdAsync(int ticketId)
    {
        throw new NotImplementedException();
    }

    private SqlConnection CreateSqlConnection()
    {
        return new SqlConnection(_options.DefaultConnection);
    }
}