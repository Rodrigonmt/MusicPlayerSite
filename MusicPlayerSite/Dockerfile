# ----------------------------
# Etapa 1: Build com .NET SDK, Python, pip, FFmpeg e dependências Python
# ----------------------------
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Instala Python, pip e ffmpeg
RUN apt-get update && apt-get install -y \
    python3 python3-pip ffmpeg && \
    rm -rf /var/lib/apt/lists/*

# Copia apenas o requirements.txt para cache eficiente
COPY MusicPlayerSite/requirements.txt .

# Atualiza pip e instala dependências com segurança
RUN python3 -m pip install --upgrade pip setuptools wheel && \
    pip install --no-cache-dir --retries 10 --timeout 100 -r requirements.txt

# Define pasta de trabalho e copia o restante do código
WORKDIR /app
COPY . .

# Publica a aplicação .NET
RUN dotnet publish -c Release -o out

# ----------------------------
# Etapa 2: Runtime final (imagem leve)
# ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# Instala Python + FFmpeg apenas para runtime
RUN apt-get update && apt-get install -y \
    python3 python3-pip ffmpeg && \
    rm -rf /var/lib/apt/lists/*

# Copia novamente o requirements.txt
COPY MusicPlayerSite/requirements.txt .

# Instala pacotes Python no runtime
RUN python3 -m pip install --upgrade pip setuptools wheel && \
    pip install --no-cache-dir --retries 10 --timeout 100 -r requirements.txt

# Define pasta da aplicação
WORKDIR /app
COPY --from=build /app/out .

# Inicia a aplicação
ENTRYPOINT ["dotnet", "MusicPlayerSite.dll"]
