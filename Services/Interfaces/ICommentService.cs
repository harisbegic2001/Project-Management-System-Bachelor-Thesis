using JWT_Implementation.DTOs;
using JWT_Implementation.Entities;

namespace JWT_Implementation.Services.Interfaces;

public interface ICommentService
{
    Task<ReadCommentDto> CreateCommentAsync(CreateCommentDto createCommentDto, string callerId);

    Task<IEnumerable<dynamic>> GetCommentsByTicketAsync(int ticketId);

    Task<int> UpdateCommentAsync(string callerId, int commentId, UpdateCommentDto updateCommentDto);

    Task<int> DeleteCommentAsync(string callerId, int commentId);
}