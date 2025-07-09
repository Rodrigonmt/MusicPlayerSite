# Etapa 1 - build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia o csproj e faz restore
COPY MusicPlayerSite/*.csproj ./MusicPlayerSite/
WORKDIR /app/MusicPlayerSite
RUN dotnet restore

# Copia o restante do projeto
WORKDIR /app
COPY . .

# Publica
WORKDIR /app/MusicPlayerSite
RUN dotnet publish -c Release -o /app/out

# Etapa 2 - runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 10000

ENTRYPOINT ["dotnet", "MusicPlayerSite.dll"]
