# Etapa 1: Build com .NET + Python + ffmpeg
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

# Copia somente arquivos essenciais para restaurar pacotes primeiro
COPY MusicPlayerSite/MusicPlayerSite.csproj ./MusicPlayerSite/
COPY MusicPlayerSite/requirements.txt .

# Restaura pacotes .NET
WORKDIR /app/MusicPlayerSite
RUN dotnet restore

# Copia o restante do código
WORKDIR /app
COPY . .

# Publica app .NET
WORKDIR /app/MusicPlayerSite
RUN dotnet publish -c Release -o /app/out

# Etapa 2: Runtime enxuto baseado no ASP.NET com Python e ffmpeg
FROM mcr.microsoft.com/dotnet/aspnet:6.0

# Instala pacotes necessários no runtime
RUN apt-get update && apt-get install -y \
    python3 python3-pip ffmpeg libsndfile1 git && \
    rm -rf /var/lib/apt/lists/*

# Copia requirements.txt e instala dependências Python
COPY MusicPlayerSite/requirements.txt ./requirements.txt
RUN python3 -m pip install --upgrade pip setuptools wheel && \
    pip install --no-cache-dir -r requirements.txt

# Define pasta de execução do app
WORKDIR /app
COPY --from=build /app/out .

# Define porta padrão para Railway
ENV ASPNETCORE_URLS=http://+:3000
EXPOSE 3000

ENTRYPOINT ["dotnet", "MusicPlayerSite.dll"]
