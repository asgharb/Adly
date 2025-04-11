FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Ui/Adly.Api/Adly.Api.csproj", "src/Ui/Adly.Api/"]
COPY ["src/Infrastructure/Adly.Infrastructure.Persistence/Adly.Infrastructure.Persistence.csproj", "src/Infrastructure/Adly.Infrastructure.Persistence/"]
COPY ["src/Core/Adly.Application/Adly.Application.csproj", "src/Core/Adly.Application/"]
COPY ["src/Core/Adly.Domain/Adly.Domain.csproj", "src/Core/Adly.Domain/"]
COPY ["src/Infrastructure/Adly.Infrastructure.Identity/Adly.Infrastructure.Identity.csproj", "src/Infrastructure/Adly.Infrastructure.Identity/"]
COPY ["src/Infrastructure/Adly.Infrastructure.CrossCutting/Adly.Infrastructure.CrossCutting.csproj", "src/Infrastructure/Adly.Infrastructure.CrossCutting/"]
COPY ["src/Ui/Adly.WebFramework/Adly.WebFramework.csproj", "src/Ui/Adly.WebFramework/"]
RUN dotnet restore "src/Ui/Adly.Api/Adly.Api.csproj"
COPY . .
WORKDIR "/src/src/Ui/Adly.Api"
RUN dotnet build "Adly.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Adly.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Adly.Api.dll"]
