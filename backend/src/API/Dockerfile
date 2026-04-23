FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching
COPY API/API.csproj API/
COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Domain/Domain.csproj Domain/
COPY veldgenerated/VeldGenerated.csproj veldgenerated/
RUN dotnet restore API/API.csproj

# Copy everything else and publish
COPY . .
WORKDIR /src/API
RUN dotnet publish API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Cloud Run provides PORT=8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "API.dll"]
