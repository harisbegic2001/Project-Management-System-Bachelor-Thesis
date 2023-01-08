using JWT_Implementation.Constants;
using JWT_Implementation.DTOs;
using JWT_Implementation.EnvironmentSettings;
using JWT_Implementation.Services.Interfaces;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;

namespace JWT_Implementation.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _options;
    
    public EmailService(IOptions<EmailSettings> options)
    {
        _options = options.Value;
    }
    
    

    public async Task<string> SendLinkEmailAsync(string receiverAddress)
    {
        var loginLink = _options.LoginLink;
        var body = $"<div>Dear,</div> <br/>" +
                   "<div>Welcome to our app in order to use our app please follow the link: </div>" +
                   $"<div>{loginLink}</div> <br/>" +
                   "<div>See you soon,</div>" +
                   "<div>Admin Team</div>";

        var sendEmailRequest = new SendEmailRequestDto
        {
            Receiver = receiverAddress,
            Subject = EmailServiceConstants.RegistrationLinkSubject,
            Body = body,
        };

        return await SendEmailAsync(sendEmailRequest);
    }
    
    
    
    
    
    public async Task<string> SendEmailAsync(SendEmailRequestDto sendEmailRequestDto)
    {
        // Setting the email message
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_options.EmailUsername));
        email.To.Add(MailboxAddress.Parse(sendEmailRequestDto.Receiver));
        email.Subject = sendEmailRequestDto.Subject;
        email.Body = new TextPart(TextFormat.Html) { Text = sendEmailRequestDto.Body };

        // Configuring the SMTP Client and sending the email
        using var smtpClient = new SmtpClient();

        
        await smtpClient.ConnectAsync(
            EmailServiceConstants.Host,
            EmailServiceConstants.SmtpDefaultPort,
            MailKit.Security.SecureSocketOptions.StartTls);

        await smtpClient.AuthenticateAsync(_options.EmailUsername, _options.EmailPassword);

        var response = await smtpClient.SendAsync(email);
        await smtpClient.DisconnectAsync(true);

        return response;
    }
}