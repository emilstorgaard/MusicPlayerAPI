using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext with error handling for connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register services
builder.Services.AddScoped<SongService>();
builder.Services.AddScoped<PlaylistService>();
builder.Services.AddScoped<SearchService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://musik.emilstorgaard.dk")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Ensure secure headers and HTTPS in production
    app.UseHttpsRedirection();
    app.UseHsts();
}

// Use CORS middleware
app.UseCors("AllowAllOrigins");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
