FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ApiGateway/ApiGateway.csproj ApiGateway/
RUN dotnet restore ApiGateway/ApiGateway.csproj
COPY ApiGateway/ ApiGateway/
RUN dotnet publish ApiGateway/ApiGateway.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
