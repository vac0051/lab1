# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Lab5.LibraryApi/Library.Api/Library.Api.csproj", "Lab5.LibraryApi/Library.Api/"]
COPY ["Lab5.LibraryApi/Library.Data/Library.Data.csproj", "Lab5.LibraryApi/Library.Data/"]
RUN dotnet restore "Lab5.LibraryApi/Library.Api/Library.Api.csproj"

COPY Lab5.LibraryApi/ Lab5.LibraryApi/
WORKDIR /src/Lab5.LibraryApi/Library.Api
RUN dotnet publish "Library.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Library.Api.dll"]
