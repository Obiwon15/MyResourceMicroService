#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["ApiGateway/src/Resourceedge.ApiGateway/Resourceedge.ApiGateway.csproj", "ApiGateway/src/Resourceedge.ApiGateway/"]
COPY ["Common/Resourceedge.Common/Resourceedge.Common.csproj", "Common/Resourceedge.Common/"]
RUN dotnet restore "ApiGateway/src/Resourceedge.ApiGateway/Resourceedge.ApiGateway.csproj"
COPY . .
WORKDIR "/src/ApiGateway/src/Resourceedge.ApiGateway"
RUN dotnet build "Resourceedge.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Resourceedge.ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Resourceedge.ApiGateway.dll"]
