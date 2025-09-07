FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8000 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Deed/Deed.Api/Deed.Api.csproj", "Deed.Api/"]
COPY ["src/Deed/Deed.Domain/Deed.Domain.csproj", "Deed.Domain/"]
COPY ["src/Deed/Deed.Application/Deed.Application.csproj", "Deed.Application/"]
COPY ["src/Deed/Deed.Infrastructure/Deed.Infrastructure.csproj", "Deed.Infrastructure/"]
RUN dotnet restore "Deed.Api/Deed.Api.csproj"
COPY . .
WORKDIR "src/Deed/Deed.Api"
RUN dotnet build "Deed.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "Deed.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Deed.Api.dll"]