using BackendGVK.Db;
using BackendGVK.Extensions;
using BackendGVK.Models;
using BackendGVK.Services;
using BackendGVK.Services.Configs;
using BackendGVK.Services.EmailSender;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();
var issuer = builder.Configuration.GetSection("JwtSettings:Issuer").Value;
var audience = builder.Configuration.GetSection("JwtSettings:Audience").Value;
var secretKey = builder.Configuration.GetSection("JwtSettings:SecretKey").Value;
// Add services to the container.
builder.Services.Configure<SmtpServerSettings>(builder.Configuration.GetSection("SmtpServerAccess"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<GoogleCaptchaSettings>(builder.Configuration.GetSection("GoogleCaptchaConfig"));
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
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
}).AddEntityFrameworkStores<AppDbContext>().AddTokenProvider<ConfirmTokenProvider>("Default");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.InstanceName = builder.Configuration.GetConnectionString("Redis:Instance");
    options.Configuration = builder.Configuration.GetConnectionString("Redis:Configuration");
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ITokenManager, TokenManager>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddTransient(typeof(GoogleCaptcha));
builder.Services.AddCors(options => options.AddPolicy("AllowAnyOrigin", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowAnyOrigin");
app.UseAuthentication();
app.UseTokenManager();  
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
