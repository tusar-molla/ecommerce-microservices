FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ECommerce.Contracts/ECommerce.Contracts.csproj ECommerce.Contracts/
COPY CatalogService/CatalogService.Api/CatalogService.Api.csproj CatalogService/CatalogService.Api/
COPY CatalogService/CatalogService.Application/CatalogService.Application.csproj CatalogService/CatalogService.Application/
COPY CatalogService/CatalogService.Infrastructure/CatalogService.Infrastructure.csproj CatalogService/CatalogService.Infrastructure/
RUN dotnet restore CatalogService/CatalogService.Api/CatalogService.Api.csproj

COPY ECommerce.Contracts/ ECommerce.Contracts/
COPY CatalogService/ CatalogService/
RUN dotnet publish CatalogService/CatalogService.Api/CatalogService.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CatalogService.Api.dll"]
