FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ECommerce.Contracts/ECommerce.Contracts.csproj ECommerce.Contracts/
COPY OrderService/OrderService.Api/OrderService.Api.csproj OrderService/OrderService.Api/
COPY OrderService/OrderService.Application/OrderService.Application.csproj OrderService/OrderService.Application/
COPY OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj OrderService/OrderService.Infrastructure/
RUN dotnet restore OrderService/OrderService.Api/OrderService.Api.csproj

COPY ECommerce.Contracts/ ECommerce.Contracts/
COPY OrderService/ OrderService/
RUN dotnet publish OrderService/OrderService.Api/OrderService.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]
