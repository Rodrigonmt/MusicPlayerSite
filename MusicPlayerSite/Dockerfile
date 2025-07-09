# Etapa 1 - build do app .NET
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


# Etapa 2 - runtime com ASP.NET + Python + Demucs
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    ffmpeg \
    git \
    && rm -rf /var/lib/apt/lists/*

COPY MusicPlayerSite/requirements.txt /app/requirements.txt

RUN pip install --upgrade pip && \
    pip install -r /app/requirements.txt

WORKDIR /app
COPY --from=build /app/out .

# Porta configurável para Render
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 10000

ENTRYPOINT ["dotnet",]()