# See https://aka.ms/customizecontainer to learn how to customize your debug container.

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "./"]
COPY ["src/Darkwing.Worker/Darkwing.Worker.csproj", "src/Darkwing.Worker/"]
RUN dotnet restore "./src/Darkwing.Worker/Darkwing.Worker.csproj"
COPY ["src/Darkwing.Worker/", "src/Darkwing.Worker/"]
WORKDIR "/src/src/Darkwing.Worker"
RUN dotnet build "./Darkwing.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Darkwing.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Darkwing.Worker.dll"]
