FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ECommerce.Contracts/ECommerce.Contracts.csproj ECommerce.Contracts/
COPY InventoryService/InventoryService.Api/InventoryService.Api.csproj InventoryService/InventoryService.Api/
COPY InventoryService/InventoryService.Application/InventoryService.Application.csproj InventoryService/InventoryService.Application/
COPY InventoryService/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj InventoryService/InventoryService.Infrastructure/
RUN dotnet restore InventoryService/InventoryService.Api/InventoryService.Api.csproj

COPY ECommerce.Contracts/ ECommerce.Contracts/
COPY InventoryService/ InventoryService/
RUN dotnet publish InventoryService/InventoryService.Api/InventoryService.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "InventoryService.Api.dll"]
