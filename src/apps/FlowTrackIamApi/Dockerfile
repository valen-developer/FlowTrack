FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/apps/FlowTrackIamApi/FlowTrackIamApi.csproj
RUN dotnet publish src/apps/FlowTrackIamApi/FlowTrackIamApi.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:5002
EXPOSE 5002
COPY --from=build /app .
ENTRYPOINT ["dotnet", "FlowTrackIamApi.dll"]
