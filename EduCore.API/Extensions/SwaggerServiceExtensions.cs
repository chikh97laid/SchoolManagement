using Microsoft.OpenApi.Models;

namespace EduCore.API.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Education Social Services API",
                Version = "v1",
                Description = "API Documentation for Education Social Services System"
            });

            // 1. تعريف خيار إضافة التوكن (JWT Bearer Security Definition)
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "ضع الـ JWT Token مباشرة هنا دون كتابة كلمة Bearer"
            });

            // 2. تطبيق الحماية وإظهار الأقفال على الـ Endpoints
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
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}