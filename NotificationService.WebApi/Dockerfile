FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["NotificationService.WebApi/NotificationService.WebApi.csproj", "NotificationService.WebApi/"]
RUN dotnet restore "NotificationService.WebApi/NotificationService.WebApi.csproj"

COPY . .
WORKDIR "/src/NotificationService.WebApi"
RUN dotnet build "NotificationService.WebApi.csproj" -c Release -o /app/build
RUN dotnet publish "NotificationService.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.WebApi.dll"]
