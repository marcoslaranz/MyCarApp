using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyCarApp.Api.Data;
using CloudinaryDotNet;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Required for older timestamp behavior
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// -------------------------------
// DATABASE CONNECTION (Render + Local)
// -------------------------------
string connectionString = GetConnectionString(builder);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// -------------------------------
// FORM OPTIONS
// -------------------------------
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

// -------------------------------
// ASP.NET Identity
// -------------------------------
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// -------------------------------
// JWT Authentication
// -------------------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// -------------------------------
// Controllers + JSON
// -------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------------
// CORS
// -------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// -------------------------------
// CLOUDINARY
// -------------------------------
var cloudinarySettings = builder.Configuration.GetSection("Cloudinary");
var cloudinary = new Cloudinary(new Account(
    cloudinarySettings["CloudName"],
    cloudinarySettings["ApiKey"],
    cloudinarySettings["ApiSecret"]
));
cloudinary.Api.Secure = true;

builder.Services.AddSingleton(cloudinary);

// -------------------------------
// BUILD APP
// -------------------------------
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


// ======================================================
// HELPER: Parse Render DATABASE_URL into Npgsql format
// ======================================================
static string GetConnectionString(WebApplicationBuilder builder)
{
    var rawUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (string.IsNullOrWhiteSpace(rawUrl))
        return builder.Configuration.GetConnectionString("DefaultConnection")!;

    if (!rawUrl.StartsWith("postgres://") && !rawUrl.StartsWith("postgresql://"))
        return rawUrl;

    // Remove scheme
    var noScheme = rawUrl.Replace("postgres://", "").Replace("postgresql://", "");

    // Split into parts
    var parts = noScheme.Split('@');
    var userPass = parts[0];
    var hostDb = parts[1];

    var user = Uri.UnescapeDataString(userPass.Split(':')[0]);
    var pass = Uri.UnescapeDataString(userPass.Split(':')[1]);

    var hostPort = hostDb.Split('/')[0];
    var database = hostDb.Split('/')[1];

    var host = hostPort.Split(':')[0];
    var port = hostPort.Split(':')[1];

    var csb = new NpgsqlConnectionStringBuilder
    {
        Host = host,
        Port = int.Parse(port),
        Username = user,
        Password = pass,
        Database = database,
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    };

    return csb.ConnectionString;
}
