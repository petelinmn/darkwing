FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DarkWing.PriceSource/DarkWing.PriceSource.csproj", "DarkWing.PriceSource/"]
RUN dotnet restore "DarkWing.PriceSource/DarkWing.PriceSource.csproj"
COPY . .
WORKDIR "/src/DarkWing.PriceSource"
RUN dotnet build "DarkWing.PriceSource.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DarkWing.PriceSource.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DarkWing.PriceSource.dll"]
