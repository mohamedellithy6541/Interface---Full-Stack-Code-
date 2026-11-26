# Build stage: restore and publish the API.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first, using only the project files, so the restore layer is cached until dependencies change.
COPY Interface.sln ./
COPY src/Interface.Domain/Interface.Domain.csproj src/Interface.Domain/
COPY src/Interface.Application/Interface.Application.csproj src/Interface.Application/
COPY src/Interface.Infrastructure/Interface.Infrastructure.csproj src/Interface.Infrastructure/
COPY src/Interface.Api/Interface.Api.csproj src/Interface.Api/
RUN dotnet restore src/Interface.Api/Interface.Api.csproj

COPY src/ src/
RUN dotnet publish src/Interface.Api/Interface.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage: only the ASP.NET Core runtime and the published output.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# The base image ships a non-root "app" user.
USER app
ENTRYPOINT ["dotnet", "Interface.Api.dll"]
