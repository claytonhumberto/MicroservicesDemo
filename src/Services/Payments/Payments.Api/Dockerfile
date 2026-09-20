FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Services/Payments/Payments.Api/Payments.Api.csproj", "Services/Payments/Payments.Api/"]
COPY ["Services/Payments/Payments.Application/Payments.Application.csproj", "Services/Payments/Payments.Application/"]
COPY ["Services/Payments/Payments.Domain/Payments.Domain.csproj", "Services/Payments/Payments.Domain/"]
COPY ["Services/Payments/Payments.Infrastructure/Payments.Infrastructure.csproj", "Services/Payments/Payments.Infrastructure/"]
COPY ["BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj", "BuildingBlocks/BuildingBlocks.Contracts/"]
COPY ["BuildingBlocks/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj", "BuildingBlocks/BuildingBlocks.Observability/"]
RUN dotnet restore "Services/Payments/Payments.Api/Payments.Api.csproj"
COPY . .
RUN dotnet publish "Services/Payments/Payments.Api/Payments.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Payments.Api.dll"]
