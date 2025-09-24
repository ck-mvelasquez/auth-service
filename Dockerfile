
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Auth/Api/Auth.Api.csproj", "src/Auth/Api/"]
# Copy other project files
COPY ["src/Auth/Application/Auth.Application.csproj", "src/Auth/Application/"]
COPY ["src/Auth/Domain/Auth.Domain.csproj", "src/Auth/Domain/"]
COPY ["src/Auth/Infrastructure/Auth.Infrastructure.csproj", "src/Auth/Infrastructure/"]
RUN dotnet restore "src/Auth/Api/Auth.Api.csproj"

COPY . .
WORKDIR "/src/src/Auth/Api"
RUN dotnet build "Auth.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auth.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.Api.dll"]
