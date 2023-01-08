using JWT_Implementation.DTOs;

namespace JWT_Implementation.Services.Interfaces;

public interface IEmailService
{
    Task<string> SendEmailAsync(SendEmailRequestDto sendEmailRequestDto);

    Task<string> SendLinkEmailAsync(string receiverAddress);
}