#!/usr/bin/env bash
# exit on error
set -e

cd publish

# Export biến môi trường từ file .env nếu có
if [ -f .env ]; then
  export $(cat .env | xargs)
fi

# Thực hiện migrations
dotnet WorkFinder.Web.dll --environment=Production 