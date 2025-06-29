FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["./src", "/src"]

WORKDIR /src

RUN dotnet build -c Release

FROM build AS publish

WORKDIR /src/ChefKnifeStudios.ExpenseTracker.Data

RUN dotnet tool install dotnet-ef -g --version 9.0.1 
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations bundle --self-contained -r linux-x64 --output /app/publish/chefknifestudios.expensetracker.efbundle

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

RUN chmod +x ./chefknifestudios.expensetracker.efbundle
ENTRYPOINT ./chefknifestudios.expensetracker.efbundle --connection "$CONNECTIONSTRINGS__ExpenseTrackerDB"