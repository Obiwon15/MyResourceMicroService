#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Authentication/src/Resourceedge.Authentication.API/Resourceedge.Authentication.API.csproj", "Authentication/src/Resourceedge.Authentication.API/"]
COPY ["Email/src/Resourceedge.Email.Api/Resourceedge.Email.Api.csproj", "Email/src/Resourceedge.Email.Api/"]
COPY ["Authentication/src/Resourceedge.Authentication.Domain/Resourceedge.Authentication.Domain.csproj", "Authentication/src/Resourceedge.Authentication.Domain/"]
RUN dotnet restore "Authentication/src/Resourceedge.Authentication.API/Resourceedge.Authentication.API.csproj"
COPY . .
WORKDIR "/src/Authentication/src/Resourceedge.Authentication.API"
RUN dotnet build "Resourceedge.Authentication.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Resourceedge.Authentication.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Resourceedge.Authentication.API.dll"]
