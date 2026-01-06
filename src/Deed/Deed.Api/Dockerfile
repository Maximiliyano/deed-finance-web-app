FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8000
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/Deed

COPY ["Deed.Api/Deed.Api.csproj", "Deed.Api/"]
COPY ["Deed.Domain/Deed.Domain.csproj", "Deed.Domain/"]
COPY ["Deed.Application/Deed.Application.csproj", "Deed.Application/"]
COPY ["Deed.Infrastructure/Deed.Infrastructure.csproj", "Deed.Infrastructure/"]

RUN dotnet restore "Deed.Api/Deed.Api.csproj"

COPY . .

RUN dotnet build "Deed.Api/Deed.Api.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Deed.Api/Deed.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Deed.Api.dll"]