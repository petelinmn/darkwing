# See https://aka.ms/customizecontainer to learn how to customize your debug container.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "./"]
COPY ["src/Darkwing.Api/Darkwing.Api.csproj", "src/Darkwing.Api/"]
RUN dotnet restore "./src/Darkwing.Api/Darkwing.Api.csproj"
COPY ["src/Darkwing.Api/", "src/Darkwing.Api/"]
WORKDIR "/src/src/Darkwing.Api"
RUN dotnet build "./Darkwing.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Darkwing.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Darkwing.Api.dll"]
