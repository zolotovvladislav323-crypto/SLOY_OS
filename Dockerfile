FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet workload install maui
RUN dotnet restore
RUN dotnet publish SLOY.App.Unified/SLOY.App.Unified.csproj -f net8.0-linux -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "SLOY.App.Unified.dll"]