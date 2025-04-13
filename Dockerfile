FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj và restore dependencies
COPY ["WorkFinder.Web/WorkFinder.Web.csproj", "WorkFinder.Web/"]
RUN dotnet restore "WorkFinder.Web/WorkFinder.Web.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "WorkFinder.Web/WorkFinder.Web.csproj" -c Release
RUN dotnet publish "WorkFinder.Web/WorkFinder.Web.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Set biến db connection
ENV PGHOST=dpg-cvnbinpr0fns73eqerg0-a
ENV PGPORT=5432
ENV PGDATABASE=workfinder
ENV PGUSER=workfinder_user
ENV PGPASSWORD=e0bZbW1EBBAoGGoUFJDJPByrzUURyKHJ

# Hiển thị các biến môi trường khi container khởi chạy
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE ${PORT:-8080}

# Start command
ENTRYPOINT ["dotnet", "WorkFinder.Web.dll"] 