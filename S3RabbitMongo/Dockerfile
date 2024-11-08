FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["S3RabbitMongo/S3RabbitMongo.csproj", "S3RabbitMongo/"]
RUN dotnet restore "S3RabbitMongo/S3RabbitMongo.csproj"
COPY . .
WORKDIR "/src/S3RabbitMongo"
RUN dotnet build "S3RabbitMongo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "S3RabbitMongo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "S3RabbitMongo.dll"]
