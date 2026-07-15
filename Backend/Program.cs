using Backend.Data;
using Backend.Hubs;
using Backend.Repositories;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories (DI) ──
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddSingleton<IRoomRepository, RoomRepository>();
builder.Services.AddSingleton<IPlayerRepository, PlayerRepository>();

// ── Services (DI) ──
builder.Services.AddSingleton<IGameService, GameService>();

// ── API Controllers ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("GameCorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials();
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

app.MapGet("/", () => "Cuộc Đua Kỳ Thú - Backend Server Running! (ASP.NET Core 9)");

app.Run();
