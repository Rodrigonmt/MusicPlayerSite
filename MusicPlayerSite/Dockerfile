# ---------------------------------------------
# Etapa 1: Imagem de build (compila o app e instala Python + ffmpeg)
# ---------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Instala Python, pip, ffmpeg
RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    ffmpeg && \
    rm -rf /var/lib/apt/lists/*

# Copia apenas o requirements.txt (para cache de dependências Python)
COPY MusicPlayerSite/requirements.txt .

# Atualiza pip e instala dependências Python
RUN python3 -m pip install --upgrade pip setuptools wheel
RUN pip install --no-cache-dir --retries 10 --timeout 100 -r requirements.txt

# Define a pasta de trabalho para o build do app
WORKDIR /app

# Copia o restante do código para build
COPY . .

# Publica o app em modo Release
RUN dotnet publish -c Release -o out


# ---------------------------------------------
# Etapa 2: Imagem final (runtime leve + dependências para execução)
# ---------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Instala Python e ffmpeg no ambiente final
RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    ffmpeg && \
    rm -rf /var/lib/apt/lists/*

# Copia requirements.txt novamente para reinstalar dependências no runtime
COPY MusicPlayerSite/requirements.txt .

# Atualiza pip e instala dependências Python no runtime
RUN python3 -m pip install --upgrade pip setuptools wheel
RUN pip install --no-cache-dir --retries 10 --timeout 100 -r requirements.txt

# Define pasta de trabalho para a aplicação publicada
WORKDIR /app

# Copia arquivos da build (binários .NET publicados)
COPY --from=build /app/out .

# Define o comando de entrada (inicia a aplicação ASP.NET)
ENTRYPOINT ["dotnet", "MusicPlayerSite.dll"]
