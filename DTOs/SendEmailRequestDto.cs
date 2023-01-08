namespace JWT_Implementation.DTOs;

public class SendEmailRequestDto
{

    public string Receiver { get; set; } = string.Empty;


    public string Subject { get; set; } = string.Empty;


    public string Body { get; set; } = string.Empty;
}