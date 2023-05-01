using JWT_Implementation.DTOs;

namespace JWT_Implementation.Services.Interfaces;

public interface IEmailService
{
    string SendEmailAsync(SendEmailRequestDto sendEmailRequestDto);

    string SendLinkEmailAsync(string receiverAddress);
}