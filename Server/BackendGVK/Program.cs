using BackendGVK.Db;
using BackendGVK.Extensions;
using BackendGVK.Models;
using BackendGVK.Services;
using BackendGVK.Services.Configs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var issuer = builder.Configuration.GetSection("JwtSettings:Issuer").Value;
var audience = builder.Configuration.GetSection("JwtSettings:Audience").Value;
var secretKey = builder.Configuration.GetSection("JwtSettings:SecretKey").Value;
// Add services to the container.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));
builder.Services.AddControllers();
builder.Services.AddJwtAuthentication(issuer, audience, secretKey);
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 15;
    options.Lockout.DefaultLockoutTimeSpan = new TimeSpan(0, 10, 0);
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 10;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
}).AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.InstanceName = builder.Configuration.GetConnectionString("Redis:Instance");
    options.Configuration = builder.Configuration.GetConnectionString("Redis:Configuration");
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ITokenManager, TokenManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
