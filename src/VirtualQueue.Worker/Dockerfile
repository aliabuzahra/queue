FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/VirtualQueue.Worker/VirtualQueue.Worker.csproj", "src/VirtualQueue.Worker/"]
COPY ["src/VirtualQueue.Application/VirtualQueue.Application.csproj", "src/VirtualQueue.Application/"]
COPY ["src/VirtualQueue.Domain/VirtualQueue.Domain.csproj", "src/VirtualQueue.Domain/"]
COPY ["src/VirtualQueue.Infrastructure/VirtualQueue.Infrastructure.csproj", "src/VirtualQueue.Infrastructure/"]
RUN dotnet restore "src/VirtualQueue.Worker/VirtualQueue.Worker.csproj"
COPY . .
WORKDIR "/src/src/VirtualQueue.Worker"
RUN dotnet build "VirtualQueue.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VirtualQueue.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VirtualQueue.Worker.dll"]
