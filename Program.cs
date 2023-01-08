using System.Text;
using JWT_Implementation.EnvironmentSettings;
using JWT_Implementation.GlobalErrorHandling;
using JWT_Implementation.Services;
using JWT_Implementation.Services.Interfaces;
using JWT_Implementation.TokenService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization Header Using the Bearer Scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
        
    }
    );
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
// IOPTIONS CONFIGURATION
builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection(nameof(ConnectionStrings)));
builder.Services.Configure<SecretConfiguration>(builder.Configuration.GetSection(nameof(SecretConfiguration)));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));
builder.Services.AddOptions();

//DependencyInjection
builder.Services.AddTransient<IProjectService, ProjectService>();
builder.Services.AddTransient<ITicketService, TicketService>();
builder.Services.AddTransient<ICommentService, CommentService>();
builder.Services.AddTransient<IEmailService, EmailService>();



// Adding Authentication Middleware
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new TokenValidationParameters
        {
            //What will be validated
            ValidateIssuerSigningKey = true, //This and line under means that it our SymmetricSecurity key will be validated
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["SecretConfiguration:SecretKey"])),
            ValidateIssuer = false, //Issuer of token is our server no need for that¸
            ValidateAudience = false
        };
    });


builder.Services.AddScoped<ITokenService, TokenService>();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Custom exception Middleware (GLOBAL ERROR HANDLING)
app.ConfigureExceptionMiddleware();
app.UseHttpsRedirection();
app.UseAuthentication(); //Do you have a Valid Token?
app.UseAuthorization(); //After It is known that we have a valid token what are we allowed to do with this token
app.MapControllers();

app.Run();