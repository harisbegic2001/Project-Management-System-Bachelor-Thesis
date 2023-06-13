using System.Data.SqlClient;
using Dapper;
using JWT_Implementation.DTOs;
using JWT_Implementation.Entities;
using JWT_Implementation.EnvironmentSettings;
using JWT_Implementation.Exceptions;
using JWT_Implementation.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace JWT_Implementation.Services;

public class TicketStageService : ITicketStageService
{
    private readonly ConnectionStrings _options;

    public TicketStageService(IOptions<ConnectionStrings> options)
    {
        _options = options.Value;
    }
    
    
    public async Task DeleteTicekStageAsync(int ticketStageId)
    {
        using var connection = CreateSqlConnection();

        //Provjerava da li taj TicketStage uopšte postoji --> provjera po Id-u

        var ticketStageToDelete = await connection.QueryFirstOrDefaultAsync<ReadTicekStageDto>($"SELECT * FROM TicketStage WHERE Id = '{ticketStageId}'");
        if (ticketStageToDelete is null)
        {
            throw new TicketStageNotFoundException();
        }
        
        //Check if the Ticket Stage is the last stage for the specified project
        var numberOfTicketStagesInproject = await connection.QueryFirstOrDefaultAsync<int>($"SELECT COUNT(*) FROM TicketStage WHERE ProjectId = '{ticketStageToDelete.ProjectId}'");

        if (numberOfTicketStagesInproject < 2)
        {
            throw new NotEnoughStagesException();
        }


        var getTicketsonStageToBeDeleted = await connection.QueryAsync<Ticket>($"SELECT * FROM Tickets WHERE TicketStageId = '{ticketStageId}'");


        var possibleStagesToInheritTickets = await connection.QueryFirstOrDefaultAsync<TicketStage>($"SELECT * FROM TicketStage WHERE TicketStage.ProjectId ='{ticketStageToDelete.ProjectId}' AND TicketStage.Id != '{ticketStageId}'");
        
        
        foreach (var ticket in getTicketsonStageToBeDeleted)
        {
            var ticketStageToBeUpdated = await connection.ExecuteAsync(
                $"UPDATE Tickets SET TicketStageId = '{possibleStagesToInheritTickets.Id}' WHERE Id = '{ticket.Id}'");
        }

        var deleteTicketStage = await connection.ExecuteAsync($"DELETE FROM TicketStage WHERE Id = '{ticketStageId}'");

    }

    public async Task<ReadTicekStageDto> UpdateTicketStageAsync(UpdateTicketStageDto updateTicketStageDto, int ticketStageId, string callerId)
    {
        using var connection = CreateSqlConnection();

        //Provjerava da li taj TicketStage uopšte postoji --> provjera po Id-u

        var ticketStageToUpdate = await connection.QueryFirstOrDefaultAsync<ReadTicekStageDto>($"SELECT * FROM TicketStage WHERE Id = '{ticketStageId}'");
        if (ticketStageToUpdate is null)
        {
            throw new TicketStageNotFoundException();
        }
        
        
        //Provjera da li je korisnik na tom projektu
        var checkIfCallerOnTheProject = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM UsersProjectsRelation WHERE ProjectId = '{ticketStageToUpdate.ProjectId}' AND UserId = '{callerId}'");
        if (checkIfCallerOnTheProject is null)
        {
            throw new UserNotOnProjectException();
        }

        var updatingTicekStage =
            await connection.ExecuteAsync($"UPDATE TicketStage SET StageName = '{updateTicketStageDto.StageName}' WHERE Id = '{ticketStageId}'");

        return new ReadTicekStageDto
        {
            StageName = updateTicketStageDto.StageName,
            ProjectId = ticketStageToUpdate.ProjectId
        };
    }

    public async Task CreateTicketStageAsync(CreateTicketStageDto createTicketStageDto, int projectId, string callerId)
    {
        using var connection = CreateSqlConnection();

        //Provjera da li je korisnik na tom projektu
        var checkIfCallerOnTheProject = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM UsersProjectsRelation WHERE ProjectId = '{projectId}' AND UserId = '{callerId}'");
        if (checkIfCallerOnTheProject is null)
        {
            throw new UserNotOnProjectException();
        }

        var creatingProject = await connection.ExecuteAsync($"INSERT INTO TicketStage (StageName, ProjectId) VALUES (@StageName, @ProjectId)",
        new {
            @StageName = createTicketStageDto.StageName,
            @ProjectId = projectId
        });



    }

    public async Task<IEnumerable<TicketStage>> GetTicketStagesOnProjectAsync(int projectId)
    {
        using var connection = CreateSqlConnection();

        var ticketStagesOnSingleProject =
            await connection.QueryAsync<TicketStage>($"SELECT TicketStage.Id, TicketStage.StageName FROM TicketStage WHERE TicketStage.ProjectId = '{projectId}' ");

        return ticketStagesOnSingleProject;
    }

    public async Task UpdateTicketCurrentStageAsync(int ticketId,  UpdateTicketCurrentStageDto stageName)
    {
        using var connection = CreateSqlConnection();

        var projectIdOfTheTicketToBeUpdated = await connection.QueryFirstOrDefaultAsync<int>($"SELECT Tickets.ProjectId FROM Tickets  WHERE Tickets.Id='{ticketId}'");

        var ticketStageNamesOfTheProject = await connection.QueryAsync<string>($"SELECT TicketStage.StageName FROM TicketStage WHERE TicketStage.ProjectId = '{projectIdOfTheTicketToBeUpdated}'");

        if (!ticketStageNamesOfTheProject.Contains(stageName.stageName))
        {
            throw new TicketStageNotFoundException();
        }

        var newTicketStageId = await connection.QueryFirstOrDefaultAsync<int>($"SELECT TicketStage.Id FROM TicketStage WHERE TicketStage.StageName ='{stageName.stageName}' AND TicketStage.ProjectId='{projectIdOfTheTicketToBeUpdated}'");

        var updatingStage = await connection.ExecuteAsync($"UPDATE Tickets SET TicketStageId = '{newTicketStageId}' WHERE Id = '{ticketId}'");
    }

    private SqlConnection CreateSqlConnection()
    {
        return new SqlConnection(_options.DefaultConnection);
    }
}