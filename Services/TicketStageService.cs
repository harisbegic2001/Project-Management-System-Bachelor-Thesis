using System.Data.SqlClient;
using Dapper;
using JWT_Implementation.DTOs;
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

    public async Task CreateTicketStageAsync(CreateTicketStageDto createTicketStageDto, string callerId)
    {
        using var connection = CreateSqlConnection();

        //Provjera da li je korisnik na tom projektu
        var checkIfCallerOnTheProject = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM UsersProjectsRelation WHERE ProjectId = '{createTicketStageDto.ProjectId}' AND UserId = '{callerId}'");
        if (checkIfCallerOnTheProject is null)
        {
            throw new UserNotOnProjectException();
        }

        var creatingProject = await connection.ExecuteAsync($"INSERT INTO TicketStage (StageName, ProjectId) VALUES (@StageName, @ProjectId)",
        new {
            @StageName = createTicketStageDto.StageName,
            @ProjectId = createTicketStageDto.ProjectId
        });



    }
    
    private SqlConnection CreateSqlConnection()
    {
        return new SqlConnection(_options.DefaultConnection);
    }
}