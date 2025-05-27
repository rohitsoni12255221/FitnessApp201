using UserAPI;
using Microsoft.OpenApi.Models;
using UserAPI.IRepository;
using Shared.SharedMiddleware;
using Shared.Helpers;
using Serilog;
using UserAPI.AzureServices;
using UserAPI.Services;
using UserAPI.Models;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using UserAPI.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


//string keyVaultUrl = "https://fitnesskeuvault.vault.azure.net/";
//var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential(includeInteractiveCredentials: true));

//string secretName = "ConnectionString";
//KeyVaultSecret secret = await client.GetSecretAsync(secretName);

//Console.WriteLine($"Secret: {secret.Value}");
//if (builder.Environment.IsDevelopment())
//{
//    var KeyVaultUrl = builder.Configuration.GetSection("KeyVault:KeyVaultUrl");
//    var KeyVaultClientId = builder.Configuration.GetSection("KeyVault:ClientId");
//    var KeyVaultClientSecret = builder.Configuration.GetSection("KeyVault:ClientSecret");
//    var KeyVaultDirectoryId = builder.Configuration.GetSection("KeyVault:DirectoryId");

//    var credentials = new ClientSecretCredential(KeyVaultDirectoryId.Value!.ToString(), KeyVaultClientId.Value!.ToString(), KeyVaultClientSecret.Value!.ToString());
//    builder.Configuration.AddAzureKeyVault(KeyVaultUrl.Value!.ToString(), KeyVaultClientId.Value!.ToString(), KeyVaultClientSecret.Value!.ToString(), new DefaultKeyVaultSecretManager());
//    var client = new SecretClient(new Uri(KeyVaultUrl.Value!.ToString()), credentials);

//    builder.Services.AddDbContext<UserDbContext>(O =>
//    {
//        O.UseSqlServer(client.GetSecret("DevConnection").Value.Value.ToString());
//    });
//}
//;
//if (builder.Environment.IsDevelopment())
//{ 
//    var kv = builder.Configuration.GetSection("KeyVault");
//    string vaultUrl = kv["VaultUri"]!;
//    string clientId = kv["ClientId"]!;
//    string clientSecret = kv["ClientSecret"]!;

//    // This overload is in Microsoft.Extensions.Configuration.AzureKeyVault
//    builder.Configuration.AddAzureKeyVault(
//        vaultUrl,
//        clientId,
//        clientSecret,
//        new DefaultKeyVaultSecretManager()
//    );

//    builder.Services.AddDbContext<UserDbContext>(opts =>
//        opts.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection"))
//    );

//}


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWorkoutService", policy =>
    {
        policy.WithOrigins("http://localhost:4201" , "http://localhost:4200", "http://localhost:3000", "http://localhost:5074") 
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "UserAPI", Version = "v1" });

    // Configure JWT Authentication with auto "Bearer " prefix
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter your JWT token below:",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lowercase "bearer"
        BearerFormat = "JWT"
    });

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
    options.SupportNonNullableReferenceTypes();
});

builder.Services.AddScoped<IUserRepository, UserAPI.AuthRepo.UserRepository>();

builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();

builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton<IBlobService,BlobService>();
builder.Services.AddScoped<IQueueService, AzureQueueService>();
builder.Services.AddScoped<EmailService>();
builder.Services.Configure<SmtpSetting>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User", "Admin"));
});

//Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("C:\\Users\\rohit.soni\\Desktop\\FitnessTracking\\BackendApi\\FitnessTrackingApp\\UserAPI\\Logs\\logs.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();
//Log.Logger = new LoggerConfiguration().
//    WriteTo.Console()
//    .WriteTo.Debug()
//    .MinimumLevel.Information()
//    .Enrich.FromLogContext()
//    .CreateBootstrapLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowWorkoutService");

app.UseMiddleware<Shared.Helpers.ExceptionHandlingMiddleware>();
app.UseRouting();

app.UseHttpsRedirection();

//app.UseAuthentication();
app.UseMiddleware<JwtValidationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
