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

var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL") ??
    builder.Configuration.GetConnectionString("DefaultConnection");


/*
if (connectionString!.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
{
    var csb = new NpgsqlConnectionStringBuilder(connectionString);
    csb.SslMode = SslMode.Require;
    connectionString = csb.ConnectionString;
}
*/
if (connectionString!.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}




builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


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