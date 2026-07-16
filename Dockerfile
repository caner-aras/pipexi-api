FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for better Docker layer caching on restore.
COPY Pipexi.sln ./
COPY src/Pipexi.Api/Pipexi.Api.csproj src/Pipexi.Api/
COPY src/Pipexi.Application/Pipexi.Application.csproj src/Pipexi.Application/
COPY src/Pipexi.Contracts/Pipexi.Contracts.csproj src/Pipexi.Contracts/
COPY src/Pipexi.Domain/Pipexi.Domain.csproj src/Pipexi.Domain/
COPY src/Pipexi.Infrastructure/Pipexi.Infrastructure.csproj src/Pipexi.Infrastructure/
COPY src/Pipexi.Persistence/Pipexi.Persistence.csproj src/Pipexi.Persistence/
COPY src/Pipexi.Shared/Pipexi.Shared.csproj src/Pipexi.Shared/

RUN dotnet restore src/Pipexi.Api/Pipexi.Api.csproj

# Copy the rest of the source and publish.
COPY . .
RUN dotnet publish src/Pipexi.Api/Pipexi.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render provides PORT at runtime. Bind Kestrel to it (fallback: 10000).
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-10000} dotnet Pipexi.Api.dll"]