FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["Server/AndNetwork.Server.csproj", "Server/"]
COPY ["Client/AndNetwork.Client.csproj", "Client/"]
COPY ["Shared/AndNetwork.Shared.csproj", "Shared/"]
RUN dotnet restore "Server/AndNetwork.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "AndNetwork.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AndNetwork.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AndNetwork.Server.dll"]
