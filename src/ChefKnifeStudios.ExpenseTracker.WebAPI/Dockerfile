FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["./src", "/src"]

WORKDIR /src/ChefKnifeStudios.ExpenseTracker.WebAPI

RUN dotnet build -c Release

FROM build AS publish

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ChefKnifeStudios.ExpenseTracker.WebAPI.dll"]

# Build image with: docker build -f src/ChefKnifeStudios.ExpenseTracker.WebAPI/Dockerfile -t expense-tracker-api-image .
# Run image with: docker run -d -p 5268:8080 --name expense-tracker-api-container expense-tracker-api-image