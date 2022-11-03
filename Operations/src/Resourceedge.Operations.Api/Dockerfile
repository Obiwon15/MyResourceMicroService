#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Resourceedge.Operations.Api/Resourceedge.Operations.Api.csproj", "Resourceedge.Operations.Api/"]
COPY ["Common/Resourceedge.Common/Resourceedge.Common.csproj", "Common/Resourceedge.Common/"]

RUN dotnet restore "Resourceedge.Operations.Api/Resourceedge.Operations.Api.csproj"
COPY . .
WORKDIR "/src/Resourceedge.Operations.Api"
RUN dotnet build "Resourceedge.Operations.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Resourceedge.Operations.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Resourceedge.Operations.Api.dll"]