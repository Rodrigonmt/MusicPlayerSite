# ----------------------------
# Etapa 1: Build com SDK .NET, Python, ffmpeg etc
# ----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Instala dependências
RUN apt-get update && apt-get install -y \
    python3 python3-pip ffmpeg libsndfile1 git && \
    rm -rf /var/lib/apt/lists/*

# Define pasta de trabalho
WORKDIR /app

# Copia apenas o .csproj e requirements.txt
COPY MusicPlayerSite/MusicPlayerSite.csproj ./MusicPlayerSite/
COPY MusicPlayerSite/requirements.txt ./requirements.txt

# Restaura as dependências .NET
WORKDIR /app/MusicPlayerSite
RUN dotnet restore

# Copia todo o código após o restore
WORKDIR /app
COPY . .

# Publica a aplicação
WORKDIR /app/MusicPlayerSite
RUN dotnet publish -c Release -o /app/out

# ----------------------------
# Etapa 2: Runtime leve
# ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Instala dependências runtime
RUN apt-get update && apt-get install -y \
    python3 python3-pip ffmpeg libsndfile1 git && \
    rm -rf /var/lib/apt/lists/*

# Copia requirements.txt e instala libs Python
COPY MusicPlayerSite/requirements.txt ./requirements.txt
RUN python3 -m pip install --upgrade pip setuptools wheel --break-system-packages && \
    pip install --no-cache-dir --retries 10 --timeout 100 --break-system-packages -r requirements.txt

# Copia app publicada
WORKDIR /app
COPY --from=build /app/out .

# Define porta via Railway
ENV ASPNETCORE_URLS=http://+:${PORT}
EXPOSE 8080

ENTRYPOINT ["dotnet", "MusicPlayerSite.dll"]
