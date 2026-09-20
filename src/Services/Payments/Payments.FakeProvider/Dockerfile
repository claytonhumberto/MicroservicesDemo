FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Services/Payments/Payments.FakeProvider/Payments.FakeProvider.csproj", "Services/Payments/Payments.FakeProvider/"]
RUN dotnet restore "Services/Payments/Payments.FakeProvider/Payments.FakeProvider.csproj"
COPY . .
RUN dotnet publish "Services/Payments/Payments.FakeProvider/Payments.FakeProvider.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Payments.FakeProvider.dll"]
