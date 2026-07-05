using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyCarApp.Api.Data;
using CloudinaryDotNet;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var rawConn = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(rawConn))
    throw new InvalidOperationException("Database connection string is not configured. Set DATABASE_URL or ConnectionStrings:DefaultConnection.");

string connectionString;
// Support DATABASE_URL style URIs (postgres://user:pass@host:port/db)
if (rawConn.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
    rawConn.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    var uri = new Uri(rawConn);
    var userInfo = uri.UserInfo.Split(':');
    var csb = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    };

    // Supabase pooler (port 6543) runs PgBouncer in transaction mode, which doesn't
    // preserve session state (like prepared statements) between transactions on the
    // same physical connection. Disable statement caching to be compatible with that —
    // but KEEP Npgsql's own client-side connection pooling enabled (Pooling=true is the
    // default), since disabling it forces a brand new TCP+TLS connection for every single
    // command, which is slow and can queue for minutes under any backpressure.
    if (csb.Port == 6543)
    {
        csb.MaxAutoPrepare = 0;
    }

    // Increase command timeout to reduce transient read timeouts
    csb.CommandTimeout = 60; // seconds

    connectionString = csb.ConnectionString;
}
else
{
    // Use NpgsqlConnectionStringBuilder to ensure SSL and sensible defaults
    var csb = new NpgsqlConnectionStringBuilder(rawConn)
    {
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    };

    if (csb.Port == 6543)
        csb.MaxAutoPrepare = 0;

    if (csb.CommandTimeout < 30)
        csb.CommandTimeout = 60;

    connectionString = csb.ConnectionString;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Optional: enable retry on failure for transient errors
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
    }));


// Identity
builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddScoped<UserManager<IdentityUser>>();
builder.Services.AddScoped<SignInManager<IdentityUser>>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT secret is not configured. Set JWT_SECRET or JwtSettings:SecretKey.");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? jwtSettings["Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is not configured. Set JWT_ISSUER or JwtSettings:Issuer.");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? jwtSettings["Audience"]
    ?? throw new InvalidOperationException("JWT audience is not configured. Set JWT_AUDIENCE or JwtSettings:Audience.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT AUTH FAILED: {context.Exception.GetType().Name} - {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"JWT CHALLENGE: error={context.Error}, description={context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS — allow Blazor client
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Cloudinary
var cloudinarySettings = builder.Configuration.GetSection("Cloudinary");
var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")
    ?? cloudinarySettings["CloudName"]
    ?? throw new InvalidOperationException("Cloudinary cloud name is not configured. Set CLOUDINARY_CLOUD_NAME or Cloudinary:CloudName.");
var cloudinaryApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")
    ?? cloudinarySettings["ApiKey"]
    ?? throw new InvalidOperationException("Cloudinary API key is not configured. Set CLOUDINARY_API_KEY or Cloudinary:ApiKey.");
var cloudinaryApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
    ?? cloudinarySettings["ApiSecret"]
    ?? throw new InvalidOperationException("Cloudinary API secret is not configured. Set CLOUDINARY_API_SECRET or Cloudinary:ApiSecret.");

var cloudinary = new CloudinaryDotNet.Cloudinary(new CloudinaryDotNet.Account(
    cloudName,
    cloudinaryApiKey,
    cloudinaryApiSecret
));
cloudinary.Api.Secure = true;
builder.Services.AddSingleton(cloudinary);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("BlazorClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();