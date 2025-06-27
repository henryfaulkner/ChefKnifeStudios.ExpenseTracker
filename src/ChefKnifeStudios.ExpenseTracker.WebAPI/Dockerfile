FROM mcr.microsoft.com/dotnet/sdk:9.0 AS publish
WORKDIR /src
COPY ["./src", "/src"]

WORKDIR /src/ChefKnifeStudios.ExpenseTracker.WebAPI

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80

# .NET 8 switches the default port to 8080, setting it back to 80 to work with our deployments
ENV ASPNETCORE_HTTP_PORTS=80

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "/app/publish/ChefKnifeStudios.ExpenseTracker.WebAPI.dll"]