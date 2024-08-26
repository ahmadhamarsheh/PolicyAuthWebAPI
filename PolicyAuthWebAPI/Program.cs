using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PolicyAuthWebAPI.CustomRequirement;
using PolicyAuthWebAPI.Data;
using PolicyAuthWebAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddRoles<IdentityRole>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
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

builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new OpenApiInfo
    { Version = "v1" });

    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminManagerUserPolicy", o =>
    {
        o.RequireAuthenticatedUser();
        o.RequireRole("admin", "manager", "user");
    })
    .AddPolicy("AdminManagerPolicy", o =>
    {
        o.RequireAuthenticatedUser();
        o.RequireRole("admin", "manager");
    })
    .AddPolicy("AdminUserPolicy", o =>
    {
        o.RequireAuthenticatedUser();
        o.RequireRole("admin", "user");
        o.Requirements.Add(new MinimunAgeRequirement(18));
    });
builder.Services.AddSingleton<IAuthorizationHandler, MinimunAgeHandler>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/account/create",
    async (RegisterModel model, UserManager<AppUser> userManager) =>
    {
        // Check if user already exists
        AppUser existingUser = await userManager.FindByEmailAsync(model.Email);
        if (existingUser != null) return Results.BadRequest("User already exists.");

        // Create new user
        AppUser user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth
        };
        IdentityResult result = await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return Results.BadRequest(result.Errors.Select(e => e.Description));

        // Add claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, model.Email),
            new Claim(ClaimTypes.Role, model.Role),
            new Claim(ClaimTypes.DateOfBirth, model.DateOfBirth.ToString("yyyy-MM-dd")) // Ensure date format
        };
        await userManager.AddClaimsAsync(user, claims);

        return Results.Ok(true);
    });

app.MapPost("/account/login", async (string email, string password, UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager, IConfiguration config) =>
{
    AppUser User = await userManager.FindByEmailAsync(email);
    if (User == null) return Results.NotFound();

    SignInResult result = await signInManager.CheckPasswordSignInAsync(User!, password, false);
    if (!result.Succeeded) return Results.BadRequest(false);

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
    var credentails = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: await userManager.GetClaimsAsync(User),
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentails
                );
    return Results.Ok(new JwtSecurityTokenHandler().WriteToken(token));
});

app.MapGet("/list",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminManagerPolicy")]
() =>
    {
        return Results.Ok("Admin and Manager only can have access");
    });

app.MapGet("/single",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminUserPolicy")]
() =>
    {
        return Results.Ok("Admin and User only");
    });

app.MapGet("/home",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminManagerUserPolicy")]
() =>
    {
        return Results.Ok("Hello, welcome home everyone");
    });

app.Run();
