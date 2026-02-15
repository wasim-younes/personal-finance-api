# STAGE 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the solution/project files first to leverage Docker cache
COPY ["thepiapi/thepiapi.csproj", "thepiapi/"]
RUN dotnet restore "thepiapi/thepiapi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/thepiapi"
RUN dotnet publish "thepiapi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# STAGE 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy only the compiled app from the build stage
COPY --from=build /app/publish .

# Create a folder for the SQLite database so it persists
RUN mkdir -p /app/data
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/thepiapi.db"

ENTRYPOINT ["dotnet", "thepiapi.dll"]