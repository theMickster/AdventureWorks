# Stage 1: Restore dependencies
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src
COPY AdventureWorks.sln .
COPY src/AdventureWorks.API/AdventureWorks.API.csproj src/AdventureWorks.API/
COPY src/AdventureWorks.Application/AdventureWorks.Application.csproj src/AdventureWorks.Application/
COPY src/AdventureWorks.Common/AdventureWorks.Common.csproj src/AdventureWorks.Common/
COPY src/AdventureWorks.Domain/AdventureWorks.Domain.csproj src/AdventureWorks.Domain/
COPY src/AdventureWorks.Infrastructure.Persistence/AdventureWorks.Infrastructure.Persistence.csproj src/AdventureWorks.Infrastructure.Persistence/
COPY src/AdventureWorks.Models/AdventureWorks.Models.csproj src/AdventureWorks.Models/
COPY tests/AdventureWorks.Test.Common/AdventureWorks.Test.Common.csproj tests/AdventureWorks.Test.Common/
COPY tests/AdventureWorks.UnitTests/AdventureWorks.UnitTests.csproj tests/AdventureWorks.UnitTests/
RUN dotnet restore AdventureWorks.sln

# Stage 2: Build
FROM restore AS build
COPY . .
WORKDIR /src/src/AdventureWorks.API
RUN dotnet build -c Release --no-restore

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish -c Release --no-build -o /app/publish

# Stage 4: Runtime (non-root, curl for healthcheck)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Docker
EXPOSE 8080
ENTRYPOINT ["dotnet", "AdventureWorks.API.dll"]
