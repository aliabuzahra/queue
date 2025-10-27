FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/VirtualQueue.Api/VirtualQueue.Api.csproj", "src/VirtualQueue.Api/"]
COPY ["src/VirtualQueue.Application/VirtualQueue.Application.csproj", "src/VirtualQueue.Application/"]
COPY ["src/VirtualQueue.Domain/VirtualQueue.Domain.csproj", "src/VirtualQueue.Domain/"]
COPY ["src/VirtualQueue.Infrastructure/VirtualQueue.Infrastructure.csproj", "src/VirtualQueue.Infrastructure/"]
RUN dotnet restore "src/VirtualQueue.Api/VirtualQueue.Api.csproj"
COPY . .
WORKDIR "/src/src/VirtualQueue.Api"
RUN dotnet build "VirtualQueue.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VirtualQueue.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VirtualQueue.Api.dll"]
