using System.Text;
using ApiPermissionBasedAuthorization.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ApiPermissionBasedAuthorization.Services;
using ApiPermissionBasedAuthorization.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

//To map JWT section from appsetting to JWT class
builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

// For API, Identity with roles, but without UI
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();


//To register the Authentication service
builder.Services.AddScoped<IAuthService, AuthService>();

//To add JWT Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})/*to define the JWT key place and what are the things that it will validate on them*/.AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
    };
});


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

//becasue we use the identity we should use the authentication before the Authorization
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
