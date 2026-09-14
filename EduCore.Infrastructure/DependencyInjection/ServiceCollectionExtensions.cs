using EduCore.API.Middleware;
using EduCore.Application.Common;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Features.Attendances.Interfaces;
using EduCore.Application.Features.Attendances.Services;
using EduCore.Application.Features.Authentication.Interfaces;
using EduCore.Application.Features.Enrollments.Interfaces;
using EduCore.Application.Features.Enrollments.Services;
using EduCore.Application.Features.Groups.Interfaces;
using EduCore.Application.Features.Groups.Services;
using EduCore.Application.Features.Payments.Interfaces;
using EduCore.Application.Features.Payments.Services;
using EduCore.Application.Features.Profiles.Interfaces;
using EduCore.Application.Features.Profiles.Services;
using EduCore.Application.Features.StudentFees.Interfaces;
using EduCore.Application.Features.StudentFees.Services;
using EduCore.Application.Features.StudentManagement.DTOs.Student;
using EduCore.Application.Features.StudentManagement.Interfaces;
using EduCore.Application.Features.StudentManagement.Services;
using EduCore.Application.Features.Teachers.Interfaces;
using EduCore.Application.Features.Teachers.Services;
using EduCore.Domain.Identity;
using EduCore.Infrastructure.Identity;
using EduCore.Infrastructure.Jwt;
using EduCore.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EduCore.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                options.User.RequireUniqueEmail = false;

                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<ApplicationRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


        services.Configure<JwtSettings>(
        configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IAuthService, AuthService>();

        var jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are missing in appsettings.json.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Key)),

                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    // 1. no token or expired
                    OnChallenge = async context =>
                    {
                        // stop default handling to write a custom response
                        context.HandleResponse();

                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        var response = new ErrorResponse
                        {
                            StatusCode = StatusCodes.Status401Unauthorized,
                            Message = "Unauthorized access.",
                            Errors = new List<string> { ErrorCodes.User.Unauthorized } 
                        };

                        await context.Response.WriteAsJsonAsync(response);
                    },

                    // 2. when user tries to access a forbidden resource
                    OnForbidden = async context =>
                    {
                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;

                        var response = new ErrorResponse
                        {
                            StatusCode = StatusCodes.Status403Forbidden,
                            Message = "Forbidden access.",
                            Errors = new List<string> { ErrorCodes.User.Forbidden }
                        };

                        await context.Response.WriteAsJsonAsync(response);
                    }
                };


            });

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IProfileService, ProfileService>();


        services.Configure<StudentImportSettings>(configuration.GetSection(StudentImportSettings.SectionName));

        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IStudentFeeService, StudentFeeService>();
        services.AddScoped<IGroupService, GroupService>();



        return services;
    }
}