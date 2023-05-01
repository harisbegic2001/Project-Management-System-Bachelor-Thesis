using JWT_Implementation.DTOs;

namespace JWT_Implementation.Services.Interfaces;

public interface ITicketStageService
{
    
    //Razmisliti da li treba, provjeravati za autorizaciju
    Task DeleteTicekStageAsync(int ticketStageId);

    Task<ReadTicekStageDto> UpdateTicketStageAsync(UpdateTicketStageDto updateTicketStageDto, int ticketStageId, string callerId);

    Task CreateTicketStageAsync(CreateTicketStageDto createTicketStageDto, string callerId);
}