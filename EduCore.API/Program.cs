using EduCore.API;
using EduCore.API.Extensions;
using EduCore.API.Middleware;
using EduCore.Application.DependencyInjection;
using EduCore.Application.Features.Enrollments.Validators;
using EduCore.Domain.Identity;
using EduCore.Infrastructure.DependencyInjection;
using EduCore.Infrastructure.Identity;
using EduCore.Persistence.DependencyInjection;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Enums;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();

 
builder.Services.AddValidatorsFromAssemblyContaining<CreateEnrollmentRequestValidator>();


builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});


builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.ValidationStrategy = ValidationStrategy.All;

    options.OverrideDefaultResultFactoryWith<CustomValidationResultFactory>();
});


builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
}); 

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed Roles & Admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        await IdentitySeeder.SeedAsync(roleManager, userManager, configuration, logger);
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Application failed to start due to a seeding error.");

        // stop the application with a non-zero exit code to indicate failure 
        Environment.Exit(1);
    }
}

app.UseHttpsRedirection();

app.UseAuthentication();   

app.UseAuthorization();

app.MapControllers();

app.Run();