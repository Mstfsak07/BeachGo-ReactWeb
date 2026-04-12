FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BeachRehberi.API/BeachRehberi.API.csproj", "BeachRehberi.API/"]
COPY ["BeachRehberi.Application/BeachRehberi.Application.csproj", "BeachRehberi.Application/"]
COPY ["BeachRehberi.Domain/BeachRehberi.Domain.csproj", "BeachRehberi.Domain/"]

RUN dotnet restore "BeachRehberi.API/BeachRehberi.API.csproj" --nologo

COPY . .

WORKDIR /src/BeachRehberi.API
RUN dotnet publish "BeachRehberi.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --nologo \
    /p:UseAppHost=false \
    /p:ContinuousIntegrationBuild=true \
    /p:DebugType=None \
    /p:DebugSymbols=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV PORT=8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENV DOTNET_EnableDiagnostics=0

EXPOSE 8080

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet BeachRehberi.API.dll"]
