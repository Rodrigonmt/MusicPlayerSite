# Etapa 1: Build com .NET + Python + ffmpeg
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

# Copia apenas arquivos essenciais para restaurar pacotes
COPY MusicPlayerSite/MusicPlayerSite.csproj ./MusicPlayerSite/
COPY MusicPlayerSite/requirements.txt .

# Restaura dependências .NET
WORKDIR /app/MusicPlayerSite
RUN dotnet restore

# Copia todo o código fonte
WORKDIR /app
COPY . .

# Publica o app .NET
WORKDIR /app/MusicPlayerSite
RUN dotnet publish -c Release -o /app/out


# Etapa 2: Runtime com .NET ASP.NET + Python + ffmpeg
FROM mcr.microsoft.com/dotnet/aspnet:6.0

# Atualiza pacotes
RUN apt-get update

# Instala Python e pip
RUN apt-get install -y --no-install-recommends python3 python3-pip

# Instala ffmpeg e dependências de áudio
RUN apt-get install -y --no-install-recommends ffmpeg libsndfile1 git

# Limpa cache do apt para economizar espaço
RUN rm -rf /var/lib/apt/lists/*

# Copia e instala dependências Python
COPY MusicPlayerSite/requirements.txt ./requirements.txt
RUN python3 -m pip install --upgrade pip setuptools wheel && \
    pip install --no-cache-dir -r requirements.txt

# Copia a aplicação publicada da imagem de build
WORKDIR /app
COPY --from=build /app/out .

# Define porta usada pela aplicação (para Railway)
ENV ASPNETCORE_URLS=http://+:3000
EXPOSE 3000

# Comando de entrada
ENTRYPOINT ["dotnet", "MusicPlayerSite.dll"]
