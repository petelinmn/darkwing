# -----------------------------
# Base stage: runtime
# -----------------------------
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble AS base
WORKDIR /app

RUN apt-get update \
    && apt-get install -y libc6 \
    && rm -rf /var/lib/apt/lists/*

# -----------------------------
# Build stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /

COPY . .

RUN dotnet restore src/PriceAnalyzer/PriceAnalyzer.csproj --runtime linux-x64

RUN dotnet build src/PriceAnalyzer/PriceAnalyzer.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/build

# -----------------------------
# Publish stage
# -----------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish src/PriceAnalyzer/PriceAnalyzer.csproj \
    -c $BUILD_CONFIGURATION \
    -r linux-x64 \
    -o /app/publish \
    /p:UseAppHost=true \
    /p:PublishSingleFile=false

# -----------------------------
# Final stage
# -----------------------------
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PriceAnalyzer.dll"]
