FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/MyCarApp.Api/MyCarApp.Api.csproj", "src/MyCarApp.Api/"]
RUN dotnet restore "src/MyCarApp.Api/MyCarApp.Api.csproj"
COPY . .
WORKDIR "/src/src/MyCarApp.Api"
RUN dotnet build "MyCarApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyCarApp.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyCarApp.Api.dll"]
