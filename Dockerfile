# Haven API - .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Haven.sln ./
COPY API/API.csproj API/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/

# Restore
RUN dotnet restore

# Copy source (exclude obj/bin via .dockerignore - they contain host-specific NuGet paths)
COPY . .
# Remove any obj/bin that may have been copied (Windows NuGet cache references break Linux build)
RUN rm -rf API/obj API/bin Application/obj Application/bin Domain/obj Domain/bin Infrastructure/obj Infrastructure/bin
# Restore and publish (full restore to avoid stale asset resolution)
RUN dotnet publish API/API.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose HTTP port (ASP.NET Core default)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "API.dll"]
