# ----------------------------
# Etapa 1: Build com .NET SDK, Python, pip, FFmpeg e dependências Python
# ----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Instala Python, pip, ffmpeg, libsndfile (para soundfile) e git
RUN apt-get update && apt-get install -y \
    python3 python3-pip ffmpeg libsndfile1 git && \
    rm -rf /var/lib/apt/lists/*

# Define pasta de trabalho
WORKDIR /app

# Copia apenas o .csproj e o requirements.txt (para melhor uso de cache)
COPY MusicPlayerSite/*.csproj ./MusicPlayerSite/
COPY MusicPlayerSite/requirements.txt ./

# Restaura os pacotes NuGet
WORKDIR /app/MusicPlayerSite
RUN dotnet restore

# Volta para a raiz e copia o restante do código
WORKDIR /app
COPY . .

# Publica a aplicação
WORKDIR /app/MusicPlayerSite
RUN dotnet publish -c Release -o /app/out

# ----------------------------
# Etapa 2: Runtime final
# ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Instala Python, pip, ffmpeg, libsndfile e git
RUN apt-get update && apt-get install -y \
    python3 python3-pip ffmpeg libsndfile1 git && \
    rm -rf /var/lib/apt/lists/*

# Copia requirements.txt
COPY MusicPlayerSite/requirements.txt ./

# Instala dependências Python no runtime
RUN python3 -m pip
