using System.Data.SqlClient;
using Dapper;
using JWT_Implementation.DTOs;
using JWT_Implementation.Entities;
using JWT_Implementation.EnvironmentSettings;
using JWT_Implementation.Exceptions;
using JWT_Implementation.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace JWT_Implementation.Services;

public class CommentService : ICommentService
{

    private readonly ConnectionStrings _options;
    
    public CommentService(IOptions<ConnectionStrings> options)
    {
        _options = options.Value;
    }
    
    
    
    public async Task<ReadCommentDto> CreateCommentAsync(CreateCommentDto createCommentDto, string callerId)
    {
        using var connection = CreateSqlConnection();

        var checkIfTicketExists = await connection.QueryFirstOrDefaultAsync($"SELECT Tickets.Id FROM Tickets WHERE Id = '{createCommentDto.TicketId}'");
        if (checkIfTicketExists is null)
        {
            throw new TicketNotFoundException();
        }
        
        var newComment = new Comment
        {
            CommentSource = createCommentDto.CommentSource,
            DateOfCreation = DateTime.Now,
            TicketId = createCommentDto.TicketId,
            CreatorId = Int32.Parse(callerId)
        };

        var createdComment = await connection.ExecuteAsync("INSERT INTO Comments (CommentSource, DateOfCreation, TicketId, CreatorId) values (@CommentSource, @DateOfCreation, @TicketId, @CreatorId)", newComment);

        var readComment = new ReadCommentDto
        {
            CommentSource = newComment.CommentSource,
            DateOfCreation = newComment.DateOfCreation
        };

        return readComment;
    }

    public async Task<IEnumerable<dynamic>> GetCommentsByTicketAsync(int ticketId)
    {
        using var connection = CreateSqlConnection();
        
        var checkIfTicketExists = await connection.QueryFirstOrDefaultAsync($"SELECT Tickets.Id FROM Tickets WHERE Id = '{ticketId}'");
        if (checkIfTicketExists is null)
        {
            throw new TicketNotFoundException();
        }

        var ticketComments = await connection.QueryAsync($"SELECT Users.Firstname, Users.Lastname, Comments.Id, Comments.CommentSource, Comments.DateOfCreation FROM Users JOIN Comments ON Users.Id = Comments.CreatorId WHERE Comments.TicketId = '{ticketId}'");

        return ticketComments;
    }

    public async Task<int> UpdateCommentAsync(string callerId, int commentId, UpdateCommentDto updateCommentDto)
    {
        using var connection = CreateSqlConnection();

        var existingCommentCreator = await connection.QueryFirstOrDefaultAsync<int>($"SELECT Comments.CreatorId FROM Comments WHERE Id = '{commentId}'");

        if (Int32.Parse(callerId) != existingCommentCreator)
        {
            throw new UnauthorizedAccessException();
        }

        var updatedComment = await connection.ExecuteAsync($"UPDATE Comments SET CommentSource = '{updateCommentDto.CommentSource}' WHERE Id = '{commentId}'");

        return updatedComment;
    }

    public async Task<int> DeleteCommentAsync(string callerId, int commentId)
    {
        using var connection = CreateSqlConnection();
        
        var existingCommentCreator = await connection.QueryFirstOrDefaultAsync<int>($"SELECT Comments.CreatorId FROM Comments WHERE Id = '{commentId}'");

        if (Int32.Parse(callerId) != existingCommentCreator)
        {
            throw new UnauthorizedAccessException();
        }

        var deletedComment = await connection.ExecuteAsync($"DELETE FROM Comments WHERE Id = '{commentId}'");

        return deletedComment;
    }
    
    private SqlConnection CreateSqlConnection()
    {
        return new SqlConnection(_options.DefaultConnection);
    }
}