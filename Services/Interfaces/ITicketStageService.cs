using JWT_Implementation.DTOs;
using JWT_Implementation.Entities;

namespace JWT_Implementation.Services.Interfaces;

public interface ITicketStageService
{
    
    //Razmisliti da li treba, provjeravati za autorizaciju
    Task DeleteTicekStageAsync(int ticketStageId);

    Task<ReadTicekStageDto> UpdateTicketStageAsync(UpdateTicketStageDto updateTicketStageDto, int ticketStageId, string callerId);

    Task CreateTicketStageAsync(CreateTicketStageDto createTicketStageDto, int projectId, string callerId);

    Task<IEnumerable<TicketStage>> GetTicketStagesOnProjectAsync(int projectId);

    Task UpdateTicketCurrentStageAsync(int ticketId,  UpdateTicketCurrentStageDto stageName);
}