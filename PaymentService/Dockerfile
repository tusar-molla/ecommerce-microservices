FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ECommerce.Contracts/ECommerce.Contracts.csproj ECommerce.Contracts/
COPY PaymentService/PaymentService.Api/PaymentService.Api.csproj PaymentService/PaymentService.Api/
COPY PaymentService/PaymentService.Application/PaymentService.Application.csproj PaymentService/PaymentService.Application/
COPY PaymentService/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj PaymentService/PaymentService.Infrastructure/
RUN dotnet restore PaymentService/PaymentService.Api/PaymentService.Api.csproj

COPY ECommerce.Contracts/ ECommerce.Contracts/
COPY PaymentService/ PaymentService/
RUN dotnet publish PaymentService/PaymentService.Api/PaymentService.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PaymentService.Api.dll"]
