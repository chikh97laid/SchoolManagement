
using EduCore.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EduCore.Shared.Constants;

namespace EduCore.Infrastructure.Identity;

public static class IdentitySeeder
{

    public static async Task SeedAsync(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {

        try
        {
            await SeedRolesAsync(roleManager, logger);
            await SeedAdminAsync(userManager, configuration, logger);

            logger.LogInformation("Identity database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the Identity database.");
            throw; 
        }
    }

    private static async Task SeedRolesAsync(
        RoleManager<ApplicationRole> roleManager,
        ILogger logger)
    {
        string[] roles =
        {
            Roles.Admin,
            Roles.Student,
            Roles.Teacher
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                logger.LogInformation("Creating role: {RoleName}", role);

               var result = await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role
                });

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                    logger.LogError("Failed to create role '{RoleName}'. Errors: {Errors}", role, errors);
                    
                    throw new InvalidOperationException($"Failed to create role '{role}': {errors}");
                }

            }
            else
            {
                logger.LogDebug("Role '{RoleName}' already exists. Skipping creation.", role);
            }

        }
    }

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var adminSettings = configuration.GetSection(SeedAdminSettings.SectionName).Get<SeedAdminSettings>();
        if (adminSettings is null)
        {

            logger.LogError("Configuration section '{SectionName}' is missing or invalid.", SeedAdminSettings.SectionName);
           
            throw new InvalidOperationException($"Section '{SeedAdminSettings.SectionName}' is not found in the configuration.");
        
        }

        var admin = await userManager.FindByNameAsync(adminSettings.UserName);

        if (admin is not null)
        {
            logger.LogInformation("Admin user '{UserName}' already exists. Skipping creation.", adminSettings.UserName);
            return;
        }

        logger.LogInformation("Admin user not found. Creating default Admin account: {UserName}", adminSettings.UserName);

        admin = new ApplicationUser
        {
            UserName = adminSettings.UserName,
            Email = adminSettings.Email,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(admin, adminSettings.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            logger.LogError("Failed to create Admin user '{UserName}'. Errors: {Errors}", adminSettings.UserName, errors);
            throw new InvalidOperationException($"Failed to create Admin user: {errors}");
        }

        logger.LogInformation("Admin user '{UserName}' created successfully. Assigning Admin role...", adminSettings.UserName);

        var roleResult = await userManager.AddToRoleAsync(admin, Roles.Admin);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(x => x.Description));
            logger.LogError("Failed to assign '{Role}' role to Admin user. Errors: {Errors}", Roles.Admin, errors);
            throw new InvalidOperationException($"Failed to assign '{Roles.Admin}' role to Admin user: {errors}");
        }

        logger.LogInformation("Admin user successfully assigned to role '{Role}'.", Roles.Admin);
    }
}