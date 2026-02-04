# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY AquaHub.MVC.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish AquaHub.MVC.csproj -c Release -o /app --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./

# Expose port 8080 (Railway will map this)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "AquaHub.MVC.dll"]
