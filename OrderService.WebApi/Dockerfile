FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["OrderService.WebApi/OrderService.WebApi.csproj", "OrderService.WebApi/"]
COPY ["OrderService.DataAccess/OrderService.DataAccess.csproj", "OrderService.DataAccess/"]
RUN dotnet restore "OrderService.WebApi/OrderService.WebApi.csproj"

COPY . .
WORKDIR "/src/OrderService.WebApi"
RUN dotnet build "OrderService.WebApi.csproj" -c Release -o /app/build
RUN dotnet publish "OrderService.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderService.WebApi.dll"]
