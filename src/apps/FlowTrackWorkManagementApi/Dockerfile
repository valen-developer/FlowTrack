FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/apps/FlowTrackWorkManagementApi/FlowTrackWorkManagementApi.csproj
RUN dotnet publish src/apps/FlowTrackWorkManagementApi/FlowTrackWorkManagementApi.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:5003
EXPOSE 5003
COPY --from=build /app .
ENTRYPOINT ["dotnet", "FlowTrackWorkManagementApi.dll"]
