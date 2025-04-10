FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj và restore dependencies
COPY WorkFinder.Web/*.csproj ./WorkFinder.Web/
COPY *.sln ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Biến môi trường
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port (for documentation, Render ignores this)
EXPOSE ${PORT:-8080}

# Start command
ENTRYPOINT ["dotnet", "WorkFinder.Web.dll"] 