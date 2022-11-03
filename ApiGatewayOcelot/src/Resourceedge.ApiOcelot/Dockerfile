#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["ApiGatewayOcelot/src/Resourceedge.ApiOcelot/Resourceedge.ApiOcelot.csproj", "ApiGatewayOcelot/src/Resourceedge.ApiOcelot/"]
RUN dotnet restore "ApiGatewayOcelot/src/Resourceedge.ApiOcelot/Resourceedge.ApiOcelot.csproj"
COPY . .
WORKDIR "/src/ApiGatewayOcelot/src/Resourceedge.ApiOcelot"
RUN dotnet build "Resourceedge.ApiOcelot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Resourceedge.ApiOcelot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Resourceedge.ApiOcelot.dll"]
