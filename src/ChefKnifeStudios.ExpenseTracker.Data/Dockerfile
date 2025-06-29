FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["./src", "/src"]

WORKDIR /src/ChefKnifeStudios.ExpenseTracker.Data
RUN dotnet build -c Release

FROM build AS publish
WORKDIR /src/ChefKnifeStudios.ExpenseTracker.Data

# Install EF tools
RUN dotnet tool install dotnet-ef -g --version 9.0.5
ENV PATH="$PATH:/root/.dotnet/tools"

# Create the migration bundle
RUN dotnet ef migrations bundle --self-contained -r linux-x64 --output /app/publish/efbundle

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Make executable
RUN chmod +x ./efbundle

# Simple entrypoint that accepts connection string as environment variable or argument
ENTRYPOINT ["sh", "-c", "./efbundle --connection \"${CONNECTIONSTRINGS__EXPENSETRACKERDB:-${ConnectionStrings__ExpenseTrackerDB:-$1}}\""]