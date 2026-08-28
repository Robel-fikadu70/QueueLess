using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QueueLess.Domain.Entities;
using QueueLess.Domain.Enums;
using QueueLess.Infrastructure.Identity;


namespace QueueLess.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(QlDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Ensure the database matches the current migration schema
        await context.Database.EnsureCreatedAsync();

        // 2. Seed System Roles
        string[] roles = ["Admin", "Staff", "Customer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 3. Seed Default Admin User
        var adminEmail = "admin@queueless.com";
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // 4. Seed Default Staff User (Required to demonstrate assignment data)
        var staffEmail = "staff@queueless.com";
        var existingStaff = await userManager.FindByEmailAsync(staffEmail);
        ApplicationUser? seededStaff = existingStaff;

        if (seededStaff == null)
        {
            var staffUser = new ApplicationUser
            {
                UserName = staffEmail,
                Email = staffEmail,
                FirstName = "Jane",
                LastName = "Smith",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(staffUser, "Staff123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(staffUser, "Staff");
                seededStaff = staffUser;
            }
        }

        // 5. Seed Facilities and Services
        if (!await context.Facilities.AnyAsync())
        {
            // Facility A: City Hospital
            var hospital = new Facility
            {
                Name = "City Hospital (Bole Branch)",
                Description = "Emergency, primary care and diagnostics center",
                Location = "Bole Road, Near Airport",
                OperatingHours = "08:00 - 20:00",
                Status = QueueStatus.Open
            };

            // Facility B: Metro Dental Clinic
            var clinic = new Facility
            {
                Name = "Metro Clinic (Downtown)",
                Description = "General practice, family dentistry, and pediatrics",
                Location = "Down Town, Plaza Area",
                OperatingHours = "09:00 - 18:00",
                Status = QueueStatus.Open
            };

            await context.Facilities.AddRangeAsync(hospital, clinic);
            await context.SaveChangesAsync(); // Generates Facility IDs required for foreign key mapping

            // Seed 3 Services for City Hospital
            var hospitalServices = new[]
            {
                new Service { FacilityId = hospital.Id, Name = "Registration", Description = "Front desk administrative check-in", EstimatedDurationMinutes = 5, IsActive = true },
                new Service { FacilityId = hospital.Id, Name = "Laboratory", Description = "Blood tests, urine samples and diagnostics", EstimatedDurationMinutes = 15, IsActive = true },
                new Service { FacilityId = hospital.Id, Name = "Pharmacy", Description = "Prescription collection and billing", EstimatedDurationMinutes = 10, IsActive = true }
            };

            // Seed 3 Services for Metro Dental Clinic
            var clinicServices = new[]
            {
                new Service { FacilityId = clinic.Id, Name = "General Consultation", Description = "Standard check-up with general practitioner", EstimatedDurationMinutes = 20, IsActive = true },
                new Service { FacilityId = clinic.Id, Name = "Pediatrics", Description = "Specialist care for child candidates", EstimatedDurationMinutes = 15, IsActive = true },
                new Service { FacilityId = clinic.Id, Name = "Dental Care", Description = "Root canal, cleanings and dental check-ups", EstimatedDurationMinutes = 30, IsActive = true }
            };

            await context.Services.AddRangeAsync(hospitalServices);
            await context.Services.AddRangeAsync(clinicServices);
            await context.SaveChangesAsync(); // Generates Service IDs required for assignments

            // 6. Seed Staff Assignment (Assign our single staff user to the Hospital Laboratory service)
            if (seededStaff != null)
            {
                var laboratoryService = hospitalServices.First(s => s.Name == "Laboratory");
                var assignment = new StaffAssignment
                {
                    StaffId = seededStaff.Id,
                    ServiceId = laboratoryService.Id,
                    CounterNumber = 1,
                    IsActive = true
                };

                await context.StaffAssignments.AddAsync(assignment);
                await context.SaveChangesAsync();
            }
        }
    }
}