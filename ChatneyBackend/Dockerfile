# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of your app and build
COPY . ./
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published app from build image
COPY --from=build /app/publish ./

RUN ls -lah

# Expose port 8080 (the default for ASP.NET Core 9)
EXPOSE 8080

# Set environment variable for ASP.NET Core (optional but good practice)
ENV ASPNETCORE_URLS=http://+:8080

# Run the app
ENTRYPOINT ["dotnet", "ChatneyBackend.dll"]
