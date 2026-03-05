using AspNetCoreRateLimit;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Required when running behind a reverse proxy (e.g. Render, nginx)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
}));


builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Haven Safety Platform API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    // If you have a SecurityRequirementsOperationFilter, add it here
    // c.OperationFilter<SecurityRequirementsOperationFilter>();
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header
            },
            new System.Collections.Generic.List<string>()
        }
    });
});
builder.Services.AddSignalR();
var rabbitConnStr = builder.Configuration.GetConnectionString("RabbitMQ") ?? string.Empty;
builder.Services.AddSingleton<Haven.Infrastructure.IRabbitMQService>(sp => new Haven.Infrastructure.RabbitMQService(rabbitConnStr));
var mongoConnStr = builder.Configuration.GetConnectionString("MongoLocations") ?? string.Empty;
builder.Services.AddSingleton<Haven.Infrastructure.ILocationRepository>(sp => new Haven.Infrastructure.MongoLocationRepository(mongoConnStr));
builder.Services.AddSingleton<Haven.Infrastructure.INotificationService, Haven.Infrastructure.NotificationService>();
var groupConnStr = builder.Configuration.GetConnectionString("PostgresGroups") ?? string.Empty;
builder.Services.AddSingleton<Haven.Infrastructure.IGroupRepository>(sp => new Haven.Infrastructure.PostgresGroupRepository(groupConnStr));

// Emergency/SOS storage: use Postgres (same DB as groups) if configured, else MongoDB
var emergencyStorage = builder.Configuration.GetValue<string>("EmergencyStorage") ?? "Postgres";
if (emergencyStorage.Equals("Postgres", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(groupConnStr))
{
    builder.Services.AddSingleton<Haven.Infrastructure.IEmergencyRepository>(sp => new Haven.Infrastructure.PostgresEmergencyRepository(groupConnStr));
}
else
{
    builder.Services.AddSingleton<Haven.Infrastructure.IEmergencyRepository>(sp => new Haven.Infrastructure.EmergencyRepository(mongoConnStr));
}
var userConnStr = builder.Configuration.GetConnectionString("PostgresUsers") ?? string.Empty;
var roleConnStr = builder.Configuration.GetConnectionString("PostgresRoles") ?? string.Empty;
builder.Services.AddSingleton<Haven.Infrastructure.IUserRepository>(sp => new Haven.Infrastructure.PostgresUserRepository(userConnStr));
builder.Services.AddSingleton<Haven.Application.IGroupService, Haven.Application.GroupService>();
builder.Services.AddSingleton<Haven.Application.IUserService>(sp =>
    new Haven.Application.UserService(
        sp.GetRequiredService<Haven.Infrastructure.IUserRepository>()
    )
);

var jwtSigningKey = builder.Configuration.GetValue<string>("Jwt:Key");
if (string.IsNullOrWhiteSpace(jwtSigningKey))
    throw new InvalidOperationException("JWT signing key is not configured. Please set 'Jwt:Key' in your configuration.");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSigningKey)),
        ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience")
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            Console.WriteLine("Message received. Checking for token in Authorization header.");
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
            {
                Console.WriteLine("No Authorization header found.");
            }
            else
            {
                Console.WriteLine($"Authorization header found: {authHeader}");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("Authentication failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully. Claims: ");
            foreach (var claim in context.Principal.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("Authentication challenge issued. Reason: " + context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddControllers();


var app = builder.Build();

app.UseForwardedHeaders();

app.UseCors("corsapp");
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

// Swagger in all environments for API discovery (restrict in production if needed)
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Haven API v1"));

// Root and health endpoints
app.MapGet("/", () => Results.Ok(new { service = "Haven API", status = "running", docs = "/swagger" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapHub<Haven.API.Hubs.LocationHub>("/locationHub");
app.MapControllers();

app.Run();