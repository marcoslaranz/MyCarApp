using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyCarApp.Api.Data;
using CloudinaryDotNet;
using Npgsql; // add at top
using System.Security.Cryptography;



var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

//var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
//    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
//    ?? builder.Configuration.GetConnectionString("DefaultConnection");



//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Temporary debug
//Console.WriteLine($"DEBUG - Using connection: Host={connectionString?.Split(';').FirstOrDefault(s => s.StartsWith("Host"))}; Username={connectionString?.Split(';').FirstOrDefault(s => s.StartsWith("Username"))}");

// Convert postgres:// URL format to Npgsql format
//if (connectionString!.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
//{
//    var uri = new Uri(connectionString.Split('?')[0]); // Remove query string
//    var userInfo = uri.UserInfo.Split(':');
//    var username = Uri.UnescapeDataString(userInfo[0]);
//    var password = Uri.UnescapeDataString(userInfo[1]);
//    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
//}

 

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(connectionString));

/*

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

// ASP.NET Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
*/









var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL") ??
    builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString!.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
{
    var csb = new NpgsqlConnectionStringBuilder(connectionString);
    csb.SslMode = SslMode.Require; // Supabase needs TLS
    connectionString = csb.ConnectionString;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


var csb2 = new NpgsqlConnectionStringBuilder(connectionString);
Console.WriteLine($"DB: host={csb2.Host} port={csb2.Port} db={csb2.Database} user={csb2.Username} ssl={csb2.SslMode}");


var raw = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrWhiteSpace(raw))
{
    throw new Exception("DATABASE_URL is missing in Render env vars.");
}

var csb = new NpgsqlConnectionStringBuilder(raw);
Console.WriteLine($"DB connect: host={csb.Host} port={csb.Port} db={csb.Database} user={csb.Username} ssl={csb.SslMode}");





static string Sha256Hex(string s)
{
    using var sha = SHA256.Create();
    return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(s)));
}

var raw = Environment.GetEnvironmentVariable("DATABASE_URL")!;
var csb = new NpgsqlConnectionStringBuilder(raw);

// csb.Password exists only after parsing:
Console.WriteLine($"DB password hash: {Sha256Hex(csb.Password)}");





// JWT Authentication
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
//builder.Services.AddControllers();
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
var cloudinary = new CloudinaryDotNet.Cloudinary(new CloudinaryDotNet.Account(
    cloudinarySettings["CloudName"],
    cloudinarySettings["ApiKey"],
    cloudinarySettings["ApiSecret"]
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