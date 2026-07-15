# ============================================
# Dockerfile cho Backend (ASP.NET Core 8)
# Build: docker build -t cuocduakythu-backend ./Backend
# Run:   docker run -d -p 5089:5089 \
#          -e PORT=5089 \
#          -e CORS_ORIGINS=https://cuocduakythu.vercel.app \
#          -e ConnectionStrings__DefaultConnection="Host=...;..." \
#          --name game-backend cuocduakythu-backend
# ============================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Backend.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=5089

EXPOSE 5089

ENTRYPOINT ["dotnet", "Backend.dll"]
