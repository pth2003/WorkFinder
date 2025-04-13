FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy và restore packages
COPY *.sln .
COPY WorkFinder.Web/*.csproj ./WorkFinder.Web/
RUN dotnet restore

# Copy và build app
COPY . .
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Hiển thị các biến môi trường khi container khởi chạy
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

# Set biến db connection
ENV PGHOST=dpg-cvnbinpr0fns73eqerg0-a
ENV PGPORT=5432
ENV PGDATABASE=workfinder
ENV PGUSER=workfinder_user
ENV PGPASSWORD=e0bZbW1EBBAoGGoUFJDJPByrzUURyKHJ

# Expose port (for documentation, Render ignores this)
EXPOSE ${PORT:-8080}

# Start command
ENTRYPOINT ["dotnet", "WorkFinder.Web.dll"] 