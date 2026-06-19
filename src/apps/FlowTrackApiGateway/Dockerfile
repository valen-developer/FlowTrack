FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/apps/FlowTrackApiGateway/FlowTrackApiGateway.csproj
RUN dotnet publish src/apps/FlowTrackApiGateway/FlowTrackApiGateway.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:5001
EXPOSE 5001
COPY --from=build /app .
ENTRYPOINT ["dotnet", "FlowTrackApiGateway.dll"]
