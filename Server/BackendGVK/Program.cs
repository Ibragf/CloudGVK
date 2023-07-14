using BackendGVK.Db;
using BackendGVK.Extensions;
using BackendGVK.Models;
using BackendGVK.Policy.isAllowed;
using BackendGVK.Services;
using BackendGVK.Services.CloudService;
using BackendGVK.Services.Configs;
using BackendGVK.Services.EmailSender;
using BackendGVK.Services.SpaceService;
using BackendGVK.Services.TokenManagerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neo4jClient;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddUserSecrets<Program>();

string contentRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "cloudGVK", "storage");
if (!Directory.Exists(contentRootPath)) Directory.CreateDirectory(contentRootPath);
builder.Environment.ContentRootPath = contentRootPath;

var issuer = builder.Configuration.GetSection("JwtSettings:Issuer").Value;
var audience = builder.Configuration.GetSection("JwtSettings:Audience").Value;
var secretKey = builder.Configuration.GetSection("JwtSettings:SecretKey").Value;

var graphClient = new BoltGraphClient(
        new Uri(builder.Configuration.GetConnectionString("Neo4j")),
        builder.Configuration.GetSection("Neo4jSettings:Login").Value,
        builder.Configuration.GetSection("Neo4jSettings:Password").Value);
await graphClient.ConnectAsync();
await graphClient.CreateConstraintsIfNotExistsAsync();

// Add services to the container.
builder.Services.Configure<SmtpServerSettings>(builder.Configuration.GetSection("SmtpServerAccess"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<GoogleCaptchaSettings>(builder.Configuration.GetSection("GoogleCaptchaConfig"));

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));
builder.Services.AddControllers();
builder.Services.AddAuthorization(options =>
    options.AddPolicy("isAllowed", policy =>
        policy.AddRequirements(new isAllowedRequirement())));
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddPolicy("AllowAnyOrigin", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddSignalR();
builder.Services.AddHostedService<FileShredderHostedService>();

builder.Services.AddTransient(typeof(GoogleCaptcha));

builder.Services.AddScoped<ITokenManager, TokenManager>();

builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddSingleton<ICloud, CloudManager>();
builder.Services.AddSingleton<IGraphClient>(graphClient);
builder.Services.AddSingleton<IAuthorizationHandler, isOwnerHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, HasAccessHandler>();
builder.Services.AddScoped<SpaceManager>();
builder.Services.AddScoped<FileLoader>();
builder.Services.AddSingleton<IDateProvider, DateProvider>();


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
    endpoints.MapHub<ProgressLoadingHub>("api/progress");
});

app.Run();
