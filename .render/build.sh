#!/usr/bin/env bash
# Cài đặt .NET SDK nếu cần
# exit on error
set -e

# Build ứng dụng ASP.NET Core
dotnet restore
dotnet build --configuration Release
dotnet publish -c Release -o publish

# Copy file .env vào thư mục publish nếu có
if [ -f .env ]; then
  cp .env publish/
fi 