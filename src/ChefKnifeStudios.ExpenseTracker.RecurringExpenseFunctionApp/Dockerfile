FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["./src", "/src"]

WORKDIR /src/ChefKnifeStudios.ExpenseTracker.RecurringExpenseFunctionApp

RUN dotnet build -c Release

FROM build AS publish

WORKDIR /src/ChefKnifeStudios.ExpenseTracker.RecurringExpenseFunctionApp

RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=publish ["/app/publish", "/home/site/wwwroot"]