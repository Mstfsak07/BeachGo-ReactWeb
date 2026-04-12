FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BeachRehberi.API/BeachRehberi.API.csproj", "BeachRehberi.API/"]
COPY ["BeachRehberi.Application/BeachRehberi.Application.csproj", "BeachRehberi.Application/"]
COPY ["BeachRehberi.Domain/BeachRehberi.Domain.csproj", "BeachRehberi.Domain/"]

RUN dotnet restore "BeachRehberi.API/BeachRehberi.API.csproj"

COPY . .

WORKDIR /src/BeachRehberi.API
RUN dotnet publish "BeachRehberi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "BeachRehberi.API.dll"]