using ApiPermissionBasedAuthorization.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// For API, Identity with roles, but without UI
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//To Seed the database
using var scope = app.Services.CreateScope();
var loggerFactory = app.Services.GetService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("app");
try
{

    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await ApiPermissionBasedAuthorization.Seeds.DefaultRoles.SeedAsync(roleManager);
    await ApiPermissionBasedAuthorization.Seeds.DefaultUsers.SeedBasicUserAsync(userManager);
    await ApiPermissionBasedAuthorization.Seeds.DefaultUsers.SeedSuperAdminUserAsync(userManager, roleManager);

    logger.LogInformation("Data Seeded.");
    logger.LogInformation("Application Started.");
}
catch (Exception e)
{
    logger.LogWarning(e, "An Error occured while seeding data!");
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
