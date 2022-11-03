#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Appraisal/src/Resourceedge.Appraisal.API/Resourceedge.Appraisal.API.csproj", "Appraisal/src/Resourceedge.Appraisal.API/"]
COPY ["Appraisal/src/Resourceedge.Appraisal.Domain/Resourceedge.Appraisal.Domain.csproj", "Appraisal/src/Resourceedge.Appraisal.Domain/"]
COPY ["Common/Resourceedge.Common/Resourceedge.Common.csproj", "Common/Resourceedge.Common/"]
COPY ["Email/src/Resourceedge.Email.Api/Resourceedge.Email.Api.csproj", "Email/src/Resourceedge.Email.Api/"]
COPY ["Workers/src/Resourceedge.Worker.Auth/Resourceedge.Worker.Auth.csproj", "Workers/src/Resourceedge.Worker.Auth/"]
COPY ["Workers/src/Resourceedge.Worker.Common/Resourceedge.Worker.Common.csproj", "Workers/src/Resourceedge.Worker.Common/"]
RUN dotnet restore "Appraisal/src/Resourceedge.Appraisal.API/Resourceedge.Appraisal.API.csproj"
COPY . .
WORKDIR "/src/Appraisal/src/Resourceedge.Appraisal.API"
RUN dotnet build "Resourceedge.Appraisal.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Resourceedge.Appraisal.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Resourceedge.Appraisal.API.dll"]
