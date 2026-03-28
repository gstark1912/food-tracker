FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/App.Api/App.Api.csproj", "App.Api/"]
RUN dotnet restore "App.Api/App.Api.csproj"
COPY src/App.Api/ App.Api/
WORKDIR "/src/App.Api"
RUN dotnet build "App.Api.csproj" -c Release -o /app/build
RUN dotnet publish "App.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "App.Api.dll"]
