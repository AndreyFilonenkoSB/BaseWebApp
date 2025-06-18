using System.Text;
using BaseWebApp.Api.Middleware;
using BaseWebApp.Bll.Interfaces;
using BaseWebApp.Bll.Services;
using BaseWebApp.Dal;
using BaseWebApp.Dal.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// FOR DEBUGGING ONLY: Define a key that we know is correct.
//const string SHARED_SECRET_KEY = "This-Is-My-Super-Secure-And-Extra-Long-Dummy-Key-For-Testing-And-Debugging-1234567890!";

Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();
Log.Information("Starting up BaseWebApp");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ================== START DIAGNOSTIC ==================
    // This line will print the exact ApplicationName that the test host is using.
    Console.WriteLine($"---> [DIAGNOSTIC] ApplicationName = '{builder.Environment.ApplicationName}'");
    // =================== END DIAGNOSTIC ===================

    // This checks if the app is being run by our test project.
    if (!builder.Environment.ApplicationName.Contains("IntegrationTests"))
    {
        // This line will tell us if the IF block is being entered.
        Console.WriteLine("---> [DIAGNOSTIC] Registering PostgreSQL provider because the condition was TRUE.");
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
    }

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<ITokenService, TokenService>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // === THIS IS THE CORRECTED SWAGGER CONFIGURATION ===
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[]{}
            }
        });
    });

    var app = builder.Build();

    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception during startup");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

public partial class Program { }