using Backend.Data;
using Backend.Hubs;
using Backend.Repositories;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Kestrel: Bind to 0.0.0.0 on PORT env var (default 5089) ──
var port = Environment.GetEnvironmentVariable("PORT") ?? "5089";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ── Database ──
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql
            .CommandTimeout(30)
            .EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(3),
                errorCodesToAdd: null)));

// ── Repositories (DI) ──
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddSingleton<IRoomRepository, RoomRepository>();
builder.Services.AddSingleton<IPlayerRepository, PlayerRepository>();

// ── Services (DI) ──
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IRoomQuestionLoader, RoomQuestionLoader>();

// ── API Controllers ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── CORS ──
// Đọc danh sách origins cho phép từ biến môi trường CORS_ORIGINS
// Ví dụ: CORS_ORIGINS=https://cuocduakythu.vercel.app,https://your-custom-domain.com
var corsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS");
builder.Services.AddCors(options =>
{
    options.AddPolicy("GameCorsPolicy", policy =>
    {
        if (!string.IsNullOrEmpty(corsOrigins))
        {
            var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // Development fallback: allow all origins
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .SetIsOriginAllowed(origin => true)
                  .AllowCredentials();
        }
    });
});

// ── SignalR ──
builder.Services.AddSignalR();

var app = builder.Build();

// ── Pipeline ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("GameCorsPolicy");

// Map Controllers (REST API)
app.MapControllers();

// Map SignalR Hub
app.MapHub<GameHub>("/gameHub");

app.MapGet("/", () => "Cuộc Đua Kỳ Thú - Backend Server Running! (ASP.NET Core 8)");

app.Run();
