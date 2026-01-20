using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MusicPlayerAPI.Configurations;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Middleware;
using MusicPlayerAPI.Repositories;
using MusicPlayerAPI.Repositories.Interfaces;
using MusicPlayerAPI.Services;
using MusicPlayerAPI.Services.Interfaces;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration
                .AddJsonFile(
                    "local.settings.json",
                    optional: true,
                    reloadOnChange: true
                );
        }

        ConfigureServices(builder);
        ConfigureAuthentication(builder);
        ConfigureCors(builder);

        var app = builder.Build();
        ConfigurePipeline(app);

        app.Run();
    }

    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new NullReferenceException("Database connection string is not configured.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddSingleton(sp => {
            var configuration = sp.GetRequiredService<IConfiguration>();

            var settings = new Settings
            {
                UploadAudioFolderPath = configuration["FilePaths:AudioFolder"] ?? throw new NullReferenceException($"No {nameof(configuration)} found in configuration!"),
                UploadImageFolderPath = configuration["FilePaths:ImageFolder"] ?? throw new NullReferenceException($"No {nameof(configuration)} found in configuration!"),
                AllowedAudioExtensions = configuration.GetSection("FileTypes:AudioExtensions").Get<string[]>() ?? throw new NullReferenceException($"No {nameof(configuration)} found in configuration!"),
                AllowedImageExtensions = configuration.GetSection("FileTypes:ImageExtensions").Get<string[]>() ?? throw new NullReferenceException($"No {nameof(configuration)} found in configuration!"),
                JwtSecret = configuration["JwtSettings:Secret"] ?? throw new NullReferenceException($"No {nameof(configuration)} found in configuration!"),
                JwtExpiryHours = int.TryParse(configuration["JwtSettings:ExpiryHours"], out var expiry) ? expiry : throw new NullReferenceException($"No {nameof(configuration)} found in configuration!")
            };

            return settings;
        });

        builder.Services.AddScoped<ISongService, SongService>();
        builder.Services.AddScoped<IPlaylistService, PlaylistService>();
        builder.Services.AddScoped<ISearchService, SearchService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ISongRepository, SongRepository>();
        builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
        builder.Services.AddScoped<ISearchRepository, SearchRepository>();
    }

    public static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? throw new NullReferenceException("JWT Secret is not configured.");

        var key = Encoding.ASCII.GetBytes(jwtSecret);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });
    }

    public static void ConfigureCors(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseHsts();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("AllowAllOrigins");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapControllers();
    }
}