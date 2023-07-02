# Start with a base image that includes the .NET 6 runtime
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

# Copy the published application to the container
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
WORKDIR /src
COPY . .
RUN dotnet publish "JWT_Implementation.csproj" -c Release -o /app/publish

# Create a final image with the published application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JWT_Implementation.dll"]
