FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
# Instala dependencias necessarias no Alpine para .NET (Globalizacao e Timezone)
RUN apk add --no-cache \
    icu-data-full \
    icu-libs \
    tzdata \
    curl

# Configurações de Globalização e Timezone
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=America/Sao_Paulo \
    ASPNETCORE_URLS=http://+:80 \
    ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ["src/AgroSolutions.Service/AgroSolutions.Service.csproj", "AgroSolutions.Service/"]
COPY ["src/AgroSolutions.Business/AgroSolutions.Business.csproj", "AgroSolutions.Business/"]
COPY ["src/AgroSolutions.Domain/AgroSolutions.Domain.csproj", "AgroSolutions.Domain/"]
COPY ["src/AgroSolutions.Infra/AgroSolutions.Infra.csproj", "AgroSolutions.Infra/"]

RUN dotnet restore "AgroSolutions.Service/AgroSolutions.Service.csproj"
COPY . .
WORKDIR "src/AgroSolutions.Service"
RUN dotnet build "AgroSolutions.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AgroSolutions.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AgroSolutions.Service.dll"]