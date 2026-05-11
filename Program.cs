using Microsoft.EntityFrameworkCore;
using FordEnterRPG.Data;
using Scalar.AspNetCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FordEnterRPG.Repositories;
using FordEnterRPG.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure EF Core (MySQL)
// Connection string derived from: mysql://root:cimatec@127.0.0.1:3306/rpg
var connectionString = "Server=127.0.0.1;Port=3306;Database=rpg;User=root;Password=cimatec;";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

// Add Razor Pages and Views
builder.Services.AddControllersWithViews();

// JWT Authentication

// Add Cookie Authentication for MVC and JWT for APIs
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/SignIn";
    options.AccessDeniedPath = "/SignIn";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "supersecretkey"))
    };
});

// Add repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Serve OpenAPI documents at /openapi/{documentName}.json for Scalar
    app.MapSwagger("/openapi/{documentName}.json");

    // Serve Scalar UI (default at /scalar)
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SignInPage}/{action=Index}/{id?}");

app.Run();