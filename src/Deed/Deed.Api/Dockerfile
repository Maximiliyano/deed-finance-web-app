FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
WORKDIR /src

# Copy csproj files first for layer caching
COPY src/Deed/Deed.Domain/Deed.Domain.csproj src/Deed/Deed.Domain/
COPY src/Deed/Deed.Application/Deed.Application.csproj src/Deed/Deed.Application/
COPY src/Deed/Deed.Infrastructure/Deed.Infrastructure.csproj src/Deed/Deed.Infrastructure/
COPY src/Deed/Deed.Api/Deed.Api.csproj src/Deed/Deed.Api/
RUN dotnet restore src/Deed/Deed.Api/Deed.Api.csproj

# Copy source and publish
COPY src/Deed/ src/Deed/
RUN dotnet publish src/Deed/Deed.Api/Deed.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Deed.Api.dll"]
