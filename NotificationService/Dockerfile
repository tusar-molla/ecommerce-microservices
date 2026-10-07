FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ECommerce.Contracts/ECommerce.Contracts.csproj ECommerce.Contracts/
COPY NotificationService/NotificationService.Api/NotificationService.Api.csproj NotificationService/NotificationService.Api/
COPY NotificationService/NotificationService.Application/NotificationService.Application.csproj NotificationService/NotificationService.Application/
COPY NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj NotificationService/NotificationService.Infrastructure/
RUN dotnet restore NotificationService/NotificationService.Api/NotificationService.Api.csproj

COPY ECommerce.Contracts/ ECommerce.Contracts/
COPY NotificationService/ NotificationService/
RUN dotnet publish NotificationService/NotificationService.Api/NotificationService.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
