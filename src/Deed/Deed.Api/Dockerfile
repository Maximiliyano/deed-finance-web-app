FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8000
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Deed/Deed.Api/Deed.Api.csproj", "src/Deed/Deed.Api/"]
COPY ["src/Deed/Deed.Domain/Deed.Domain.csproj", "src/Deed/Deed.Domain/"]
COPY ["src/Deed/Deed.Application/Deed.Application.csproj", "src/Deed/Deed.Application/"]
COPY ["src/Deed/Deed.Infrastructure/Deed.Infrastructure.csproj", "src/Deed/Deed.Infrastructure/"]

RUN dotnet restore "src/Deed/Deed.Api/Deed.Api.csproj"

COPY . .

RUN dotnet build "src/Deed/Deed.Api/Deed.Api.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/Deed/Deed.Api/Deed.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Deed.Api.dll"]