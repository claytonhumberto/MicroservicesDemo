FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Services/Notifications/Notifications.Worker/Notifications.Worker.csproj", "Services/Notifications/Notifications.Worker/"]
COPY ["Services/Notifications/Notifications.Domain/Notifications.Domain.csproj", "Services/Notifications/Notifications.Domain/"]
COPY ["BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj", "BuildingBlocks/BuildingBlocks.Contracts/"]
COPY ["BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj", "BuildingBlocks/BuildingBlocks.Observability/"]
RUN dotnet restore "Services/Notifications/Notifications.Worker/Notifications.Worker.csproj"
COPY . .
RUN dotnet publish "Services/Notifications/Notifications.Worker/Notifications.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notifications.Worker.dll"]
