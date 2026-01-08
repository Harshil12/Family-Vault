# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FamilyVault.API/FamilyVault.API.csproj", "FamilyVault.API/"]
RUN dotnet restore "FamilyVault.API/FamilyVault.API.csproj"

COPY . .
WORKDIR "/src/FamilyVault.API"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FamilyVault.API.dll"]
