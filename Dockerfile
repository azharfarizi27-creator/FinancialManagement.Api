# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["FinancialManagement.Api/FinancialManagement.Api.csproj", "FinancialManagement.Api/"]
RUN dotnet restore "FinancialManagement.Api/FinancialManagement.Api.csproj"

# Copy everything and build release
COPY . .
WORKDIR "/src/FinancialManagement.Api"
RUN dotnet publish "FinancialManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FinancialManagement.Api.dll"]
