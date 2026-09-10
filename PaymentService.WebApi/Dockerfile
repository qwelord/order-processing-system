FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PaymentService.WebApi/PaymentService.WebApi.csproj", "PaymentService.WebApi/"]
COPY ["PaymentService.DataAccess/PaymentService.DataAccess.csproj", "PaymentService.DataAccess/"]
RUN dotnet restore "PaymentService.WebApi/PaymentService.WebApi.csproj"

COPY . .
WORKDIR "/src/PaymentService.WebApi"
RUN dotnet build "PaymentService.WebApi.csproj" -c Release -o /app/build
RUN dotnet publish "PaymentService.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.WebApi.dll"]
