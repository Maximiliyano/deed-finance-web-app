FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8000
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG API_PATH="/src/Deed/Deed.Api"
WORKDIR /src

COPY ["src/Deed/Deed.Api/Deed.Api.csproj", "Deed.Api/"]
COPY ["src/Deed/Deed.Domain/Deed.Domain.csproj", "Deed.Domain/"]
COPY ["src/Deed/Deed.Application/Deed.Application.csproj", "Deed.Application/"]
COPY ["src/Deed/Deed.Infrastructure/Deed.Infrastructure.csproj", "Deed.Infrastructure/"]

COPY . .
RUN dotnet restore "src/Deed/Deed.Api/Deed.Api.csproj"
RUN dotnet build "src/Deed/Deed.Api/Deed.Api.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/Deed/Deed.Api/Deed.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false



FROM base AS final
WORKDIR /app