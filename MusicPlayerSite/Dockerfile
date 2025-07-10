# ----------------------------
# Etapa 1: Build com .NET SDK, Python, pip, FFmpeg e dependências Python
# ----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Instala Python, pip, ffmpeg, libsndfile (para soundfile) e git (necessário para alguns pacotes Python)
RUN apt-get update && apt-get install -y \
    python3 python3-pip ffmpeg libsndfile1 git && \
    rm -rf /var/lib/apt/lists/*

# Copia apenas o requirements.txt (para cache eficiente)
COPY MusicPlayerSite/requirements.txt .

# Atualiza pip e instala as dependências Python (incluindo demucs, diffq, soundfile)
RUN python3 -m pip install --upgrade pip setuptools wheel --break-system-packages && \
    pip install --no-cache-dir --retries 10 --timeout 100 --break-system-packages -r requirements.txt

# Define pasta de trabalho e copia o código fonte
WORKDIR /app
COPY . .

# Publica a aplicação .NET
RUN dotnet publish -c Release -o out

# ----------------------------
# Etapa 2: Runtime final (imagem leve)
# ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Instala Python, pip, ffmpeg, libsndfile e git para runtime
RUN apt-get update && apt-get install -y \
    python3 python3-pip ffmpeg libsndfile1 git && \
    rm -rf /var/lib/apt/lists/*

# Copia requirements.txt
COPY MusicPlayerSite/requirements.txt .

# Atualiza pip e instala dependências Python no runtime
RUN python3 -m pip install --upgrade pip setuptools wheel --break-system-packages && \
    pip install --no-cache-dir --retries 10 --timeout 100 --break-system-packages -r requirements.txt

# Define pasta da aplicação
WORKDIR /app
COPY --from=build /app/out .

# Inicia a aplicação .NET
ENTRYPOINT ["dotnet", "MusicPlayerSite.dll"]
