using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyCarApp.Api.Data;
using CloudinaryDotNet;
using Npgsql; // add at top

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Convert PostgreSQL URI to standard connection string if needed
if (connectionString!.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    var csb = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port == -1 ? 5432 : uri.Port,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = uri.UserInfo.Split(':')[0],
        Password = uri.UserInfo.Split(':')[1],
        SslMode = SslMode.Require,
        Timeout = 60,
        CommandTimeout = 60
    };
    connectionString = csb.ConnectionString;
}
else
{
    var csb = new NpgsqlConnectionStringBuilder(connectionString)
    {
        SslMode = SslMode.Require,
        Timeout = 60,
        CommandTimeout = 60
    };
    connectionString = csb.ConnectionString;
}

Console.WriteLine($"DEBUG: Final connection string = {connectionString}");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

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