# Etapa de build com .NET SDK e Python + FFmpeg + Demucs
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Instala Python, pip e ffmpeg no ambiente Linux
RUN apt-get update && apt-get install -y \
    python3 python3-pip ffmpeg && \
    rm -rf /var/lib/apt/lists/*

# Instala dependências Python
COPY requirements.txt .
RUN python3 -m pip install --upgrade pip
RUN python3 -m pip install -r requirements.txt

# Define pasta de trabalho e copia o código-fonte
WORKDIR /app
COPY . .

# Publica a aplicação .NET (gera binários otimizados)
RUN dotnet publish -c Release -o out

# Imagem final (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# Instala ffmpeg no ambiente de runtime
RUN apt-get update && apt-get install -y \
    ffmpeg python3 python3-pip && \
    rm -rf /var/lib/apt/lists/*

# Instala os pacotes Python de novo no ambiente final
COPY requirements.txt .
RUN python3 -m pip install --upgrade pip
RUN python3 -m pip install -r requirements.txt

WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "MusicPlayerSite.dll"]