FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BeachRehberi.API/BeachRehberi.API.csproj", "BeachRehberi.API/"]
COPY ["BeachRehberi.Application/BeachRehberi.Application.csproj", "BeachRehberi.Application/"]
COPY ["BeachRehberi.Domain/BeachRehberi.Domain.csproj", "BeachRehberi.Domain/"]

RUN dotnet restore "BeachRehberi.API/BeachRehberi.API.csproj" --nologo

COPY . .

RUN dotnet publish "BeachRehberi.API/BeachRehberi.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BeachRehberi.API.dll"]
