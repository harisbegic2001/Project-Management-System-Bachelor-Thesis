using JWT_Implementation.DTOs;

namespace JWT_Implementation.Services.Interfaces;

public interface ITicketService
{
    Task<ReadTicketDto> CreateTicketAsync(CreateTicketDto createTicketDto, string callerId, int projectId);

    Task<IEnumerable<ReadTicketDto>> GetAllProjectTicketsAsync(int projectId, string callerId);

    Task<IEnumerable<dynamic>> GetAllTicketsAssignedToUserAsync(string callerId);

    Task<int> UpdateTicketAsync(UpdateTicketDto updateTicketDto, int ticketId, string callerId);

    Task<int> DeleteTicketAsync(int ticketId, string callerId);
    
    Task<dynamic> GetTicketByIdAsync(int ticketId);

}