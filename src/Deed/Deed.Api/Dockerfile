FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY src/Deed/Deed.Api/Deed.Api.csproj Deed.Api/
COPY src/Deed/Deed.Domain/Deed.Domain.csproj Deed.Domain/
COPY src/Deed/Deed.Application/Deed.Application.csproj Deed.Application/
COPY src/Deed/Deed.Infrastructure/Deed.Infrastructure.csproj Deed.Infrastructure/

RUN dotnet restore Deed.Api/Deed.Api.csproj

COPY src/ .

WORKDIR /src/Deed/Deed.Api
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Deed.Api.dll"]