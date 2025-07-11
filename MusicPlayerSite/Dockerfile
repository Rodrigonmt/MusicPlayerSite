# Etapa 1: build do app .NET
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

# Copia csproj e restaura pacotes
COPY MusicPlayerSite/MusicPlayerSite.csproj ./MusicPlayerSite/
WORKDIR /app/MusicPlayerSite
RUN dotnet restore

# Copia todo código e publica app em Release
COPY . /app/
RUN dotnet publish -c Release -o /app/out

# Etapa 2: runtime com Python e ffmpeg
FROM mcr.microsoft.com/dotnet/aspnet:6.0

# Instala Python e ffmpeg separado (dividido para evitar estourar memória)
RUN apt-get update && apt-get install -y --no-install-recommends python3 python3-pip && rm -rf /var/lib/apt/lists/*
RUN apt-get update && apt-get install -y --no-install-recommends ffmpeg libsndfile1 git && rm -rf /var/lib/apt/lists/*

# Copia requirements.txt e instala dependências Python
COPY MusicPlayerSite/requirements.txt /app/requirements.txt
RUN python3 -m pip install --upgrade pip setuptools wheel
RUN pip install --no-cache-dir -r /app/requirements.txt

# Copia app publicado do estágio build
COPY --from=build /app/out /app

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:3000
EXPOSE 3000

ENTRYPOINT ["dotnet", "MusicPlayerSite.dll"]
