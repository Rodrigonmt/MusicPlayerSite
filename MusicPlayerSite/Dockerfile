# Etapa 1 - build .NET
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

# Etapa 2 - runtime + Python + Demucs
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Instala dependências do sistema
RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    ffmpeg \
    git \
    && rm -rf /var/lib/apt/lists/*

# Instala Demucs
RUN pip3 install --break-system-packages demucs diffq

# Define diretório de trabalho
WORKDIR /app

# Copia app publicado
COPY --from=build /app/out .

# Define variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 10000

# Executa a aplicação
ENTRYPOINT ["dotnet", "MusicPlayerSite.dll"]