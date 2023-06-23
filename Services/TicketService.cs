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

    private readonly IEmailService _emailService;
    
    public TicketService(IOptions<ConnectionStrings> options, IEmailService emailService)
    {
        _options = options.Value;
        _emailService = emailService;
    }
    
    
    public async Task<ReadTicketDto> CreateTicketAsync(CreateTicketDto createTicketDto, string callerId, int projectId)
    {
        using var connection = CreateSqlConnection();
        
        //CHECKS IF PROJECT EXISTS
        var checkIfProjectExists = await connection.QueryFirstOrDefaultAsync<string>($"SELECT Projects.ProjectName FROM Projects WHERE Id = '{projectId}'");

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
        
        //ASSIGN TICKET STAGE TO TICKET --> BY DEFAULT LET IT BE THE FIRST AVAILABLE FOR THE PROJECT
        var firstTicketStageId = await connection.QueryFirstOrDefaultAsync<int>($"SELECT TOP 1 Id FROM TicketStage WHERE ProjectId = '{projectId}'");
        
        
        // Dodijeljivanje ticketa Asignee-u 1) Provjeriti da li je korisnik uopšte na tom projektu, 
        
        var userIdToBeAssgined = await connection.QueryFirstOrDefaultAsync<int>($"SELECT Users.Id FROM Users WHERE Email = '{createTicketDto.AsigneeEmail}'");
        
        //Check if user that is to be assigned is on the project
        var checkIfAsigneeOnTheProject = await connection.QueryFirstOrDefaultAsync($"SELECT UserId FROM UsersProjectsRelation WHERE UserId ='{userIdToBeAssgined}' AND ProjectId='{projectId}'");
       
        if (checkIfAsigneeOnTheProject is null)
        {
            throw new UserNotOnProjectException();
        }
        
        var newTicket = new Ticket
        {
            TicketName = createTicketDto.TicketName,
            TicketKey = ticketName,
            TicketDescription = createTicketDto.TicketDescription,
            TicketPriority = createTicketDto.TicketPriority,
            TicketType = createTicketDto.TicketType,
            TicketReporter = creatorUserName,
            UserId = userIdToBeAssgined,
            ProjectId = projectId,
            TicketStageId = firstTicketStageId,
            IsValid = true
        };


        var createTicket = await connection.ExecuteAsync("INSERT INTO Tickets (TicketName, TicketKey, TicketDescription, TicketPriority, TicketType, TicketReporter, UserId, ProjectId, TicketStageId, IsValid) VALUES (@TicketName, @TicketKey, @TicketDescription, @TicketPriority, @TicketType, @TicketReporter, @UserId, @ProjectId, @TicketStageId, @IsValid)", newTicket);

        var readTicket = new ReadTicketDto
        {
            TicketName = newTicket.TicketName,
            TicketDescription = newTicket.TicketDescription,
            TicketPriority = newTicket.TicketPriority,
            TicketType = newTicket.TicketType,
            TicketReporter = newTicket.TicketReporter,
            TicketKey = newTicket.TicketKey,
            TicketProject = projectName
        };
        
        
        //EMAIL LOGIC
        var asignee = await connection.QueryFirstOrDefaultAsync<ReadAsigneeDto>($"SELECT Users.FirstName, Users.Email FROM Users WHERE Id = '{userIdToBeAssgined}'");
        
        var projectReporter = await connection.QueryFirstOrDefaultAsync<string>($"SELECT Users.Email FROM Users WHERE Id = '{callerId}'");

        var emailForCreatingTicket = new AssignedToTicketEmailDto(asignee.Email, asignee.FirstName, checkIfProjectExists, projectReporter, createTicketDto.TicketName);

        _emailService.AssignedToTicketEmailAsync(emailForCreatingTicket);
        
        return readTicket;

    }

    public async Task<IEnumerable<dynamic>> GetAllProjectTicketsAsync(int projectId, string callerId)
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

        var ticketsWithStages = await connection.QueryAsync($"SELECT TicketStage.StageName, Tickets.Id, Tickets.TicketName, Tickets.TicketKey, Tickets.TicketDescription, Tickets.TicketPriority, Tickets.TicketType, Tickets.TicketReporter FROM TicketStage INNER JOIN Tickets ON TicketStage.Id = Tickets.TicketStageId WHERE Tickets.ProjectId = '{projectId}' AND Tickets.IsValid = '1'");


        return ticketsWithStages;
    }

    public async Task<IEnumerable<dynamic>> GetAllTicketsAssignedToUserAsync(string callerId)
    {
        using var connection = CreateSqlConnection();
        
        var ticketsOnProject = await connection.QueryAsync($"SELECT Tickets.Id, Tickets.TicketName, Tickets.TicketKey, Tickets.TicketDescription, Tickets.TicketType, Projects.ProjectName  FROM Tickets JOIN Projects ON Tickets.ProjectId = Projects.Id WHERE Tickets.UserId = '{Int32.Parse(callerId)}'");

        return ticketsOnProject;
    }

    public async Task<int> UpdateTicketAsync(UpdateTicketDto updateTicketDto, int ticketId, string callerId)
    {
        using var connection = CreateSqlConnection();

        var checkIfTicketExists = await connection.QueryFirstOrDefaultAsync<int?>($"SELECT Tickets.ProjectId FROM Tickets WHERE Tickets.Id = '{ticketId}'");

        if (checkIfTicketExists is null)
        {
            throw new TicketNotFoundException("Ticket does not exist");
        }
        
        //CHECK IF THE CALLER IS ON PROJECT
        var checkIfCallerOnProject = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM UsersProjectsRelation WHERE UserId = '{Int32.Parse(callerId)}' AND ProjectId = '{checkIfTicketExists}'");
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

        // Validating if the updating to a ticket stage is possible
        var availableTicketStages = await connection.QueryAsync<int>($"SELECT Id FROM TicketStage WHERE ProjectId = '{checkIfTicketExists}'");
        if (!availableTicketStages.Contains(updateTicketDto.TicketStageId))
        {
            throw new TicketStageNotFoundException();
        }
        

        var updatedTicket = await connection.ExecuteAsync($"UPDATE Tickets SET TicketName = '{updateTicketDto.TicketName}', TicketDescription = '{updateTicketDto.TicketDescription}', TicketPriority = '{updateTicketDto.TicketPriority}', TicketType = '{updateTicketDto.TicketType}', UserId = '{updateTicketDto.TicketReporterId}', TicketStageId = '{updateTicketDto.TicketStageId}' WHERE Id = '{ticketId}'");

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

        var ticketTODeactivate = await connection.ExecuteAsync($"UPDATE Tickets SET IsValid = '0' WHERE Id = '{ticketId}'");

        return ticketTODeactivate;

    }

    public  async Task<ReadTicketDto> GetTicketByIdAsync(int ticketId, string callerId)
    {
        using var connection = CreateSqlConnection();

        var getSpecifiedTicket = await connection.QueryFirstOrDefaultAsync<ReadTicketDto>($"SELECT Tickets.Id, Tickets.TicketName, Tickets.TicketKey, Tickets.TicketDescription, Tickets.TicketPriority, Tickets.TicketType, Tickets.TicketReporter, Tickets.UserId FROM Tickets WHERE Tickets.Id = '{ticketId}'");

        if (getSpecifiedTicket.UserId != Int32.Parse(callerId) )
        {
            throw new UnauthorizedAccessException();
        }
        return getSpecifiedTicket;
    }

    private SqlConnection CreateSqlConnection()
    {
        return new SqlConnection(_options.DefaultConnection);
    }
}