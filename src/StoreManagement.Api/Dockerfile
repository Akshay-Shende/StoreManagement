FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["StoreManagement.sln", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/StoreManagement.Api/StoreManagement.Api.csproj", "src/StoreManagement.Api/"]
COPY ["src/StoreManagement.Application/StoreManagement.Application.csproj", "src/StoreManagement.Application/"]
COPY ["src/StoreManagement.Domain/StoreManagement.Domain.csproj", "src/StoreManagement.Domain/"]
COPY ["src/StoreManagement.Infrastructure/StoreManagement.Infrastructure.csproj", "src/StoreManagement.Infrastructure/"]
RUN dotnet restore "src/StoreManagement.Api/StoreManagement.Api.csproj"
COPY . .
RUN dotnet publish "src/StoreManagement.Api/StoreManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StoreManagement.Api.dll"]
