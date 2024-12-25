FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the .csproj file to the container
COPY "WebCrawler/WebCrawler.csproj" ./WebCrawler/

# Restore project dependencies
WORKDIR /app/WebCrawler
RUN dotnet restore

# Copy the entire project source to the container
WORKDIR /app
COPY . .

# Build the project in Release mode
WORKDIR /app/WebCrawler
RUN dotnet publish -c Release -o /out

# Verify that the publish output exists
RUN ls /out

# Use a runtime image for the final container
FROM mcr.microsoft.com/dotnet/runtime:9.0

# Set the working directory for the runtime container
WORKDIR /app

# Copy the published output from the build stage to the runtime stage
COPY --from=build /out .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "WebCrawler.dll"]