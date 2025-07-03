FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

WORKDIR /src/ChefKnifeStudios.ExpenseTracker.RecurringExpenseFunctionApp
RUN dotnet build ChefKnifeStudios.ExpenseTracker.RecurringExpenseFunctionApp.csproj -c Release

FROM build AS publish
WORKDIR /src/ChefKnifeStudios.ExpenseTracker.RecurringExpenseFunctionApp
RUN dotnet publish ChefKnifeStudios.ExpenseTracker.RecurringExpenseFunctionApp.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated9.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=publish /app/publish /home/site/wwwroot