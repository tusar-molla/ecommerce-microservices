FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY IdentityService/IdentityService.Api/IdentityService.Api.csproj IdentityService/IdentityService.Api/
COPY IdentityService/IdentityService.Application/IdentityService.Application.csproj IdentityService/IdentityService.Application/
COPY IdentityService/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj IdentityService/IdentityService.Infrastructure/
RUN dotnet restore IdentityService/IdentityService.Api/IdentityService.Api.csproj

COPY IdentityService/ IdentityService/
RUN dotnet publish IdentityService/IdentityService.Api/IdentityService.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "IdentityService.Api.dll"]
