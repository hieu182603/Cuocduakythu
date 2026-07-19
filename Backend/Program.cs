using Backend.Hubs;
using Backend.Repositories;
using Backend.Services;
using MongoDB.Driver;
using MongoDB.Bson;
DotNetEnv.Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// ── Kestrel: Bind to 0.0.0.0 on PORT env var (default 5089) ──
var port = Environment.GetEnvironmentVariable("PORT") ?? "5089";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ── Database ──
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(defaultConnection))
    throw new InvalidOperationException("ConnectionStrings__DefaultConnection is required. Configure it in .env locally or in the hosting environment.");

var mongoUrl = new MongoUrl(defaultConnection);
var mongoClient = new MongoClient(mongoUrl);
builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton<IMongoDatabase>(mongoClient.GetDatabase(mongoUrl.DatabaseName ?? "CuocDuaKyThuDb"));

// ── Repositories (DI) ──
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddSingleton<IRoomRepository, RoomRepository>();
builder.Services.AddSingleton<IPlayerRepository, PlayerRepository>();

// ── Services (DI) ──
builder.Services.AddSingleton<IQuestionBank, QuestionBank>();
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<RoomStateStore>();
builder.Services.AddSingleton<IRoomStateStore>(provider => provider.GetRequiredService<RoomStateStore>());
builder.Services.AddHostedService(provider => provider.GetRequiredService<RoomStateStore>());
builder.Services.AddSingleton<RoomTimerService>();
builder.Services.AddSingleton<IRoomTimerService>(provider => provider.GetRequiredService<RoomTimerService>());
builder.Services.AddHostedService(provider => provider.GetRequiredService<RoomTimerService>());

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
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/ready", async (IMongoDatabase db, CancellationToken cancellationToken) =>
{
    try
    {
        var isAlive = await db.RunCommandAsync((Command<BsonDocument>)"{ping:1}", cancellationToken: cancellationToken);
        return isAlive != null 
            ? Results.Ok(new { status = "ready" }) 
            : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();
