#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Employee/src/Resourceedge.Employee.API/Resourceedge.Employee.API.csproj", "Employee/src/Resourceedge.Employee.API/"]
COPY ["Employee/src/Resourceedge.Employee.Application/Resourceedge.Employee.Application.csproj", "Employee/src/Resourceedge.Employee.Application/"]
COPY ["Employee/src/Resourceedge.Employee.Infrastructure/Resourceedge.Employee.Infrastructure.csproj", "Employee/src/Resourceedge.Employee.Infrastructure/"]
COPY ["Employee/src/Resourceedge.Employee.Domain/Resourceedge.Employee.Domain.csproj", "Employee/src/Resourceedge.Employee.Domain/"]
COPY ["Common/Resourceedge.Common/Resourceedge.Common.csproj", "Common/Resourceedge.Common/"]
RUN dotnet restore "Employee/src/Resourceedge.Employee.API/Resourceedge.Employee.API.csproj"
COPY . .
WORKDIR "/src/Employee/src/Resourceedge.Employee.API"
RUN dotnet build "Resourceedge.Employee.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Resourceedge.Employee.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Resourceedge.Employee.API.dll"]
