# Etapa 1 - build do app .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY MusicPlayerSite/*.csproj ./MusicPlayerSite/
WORKDIR /app/MusicPlayerSite
RUN dotnet restore

WORKDIR /app
COPY . .

WORKDIR /app/MusicPlayerSite
RUN dotnet publish -c Release -o /app/out


# Etapa 2 - runtime com Python + .NET
FROM debian:bullseye-slim

# Instala dependências do sistema e do .NET + Python + pip + ffmpeg
RUN apt-get update && apt-get install -y \
    wget \
    curl \
    ca-certificates \
    python3 \
    python3-pip \
    ffmpeg \
    git \
    libc6 \
    libgcc1 \
    libgssapi-krb5-2 \
    libssl1.1 \
    libstdc++6 \
    zlib1g \
    && rm -rf /var/lib/apt/lists/*

# Instala o runtime do .NET 8
RUN wget https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --version 8.0.0 --runtime aspnet --install-dir /usr/share/dotnet && \
    ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

ENV DOTNET_ROOT=/usr/share/dotnet

# Copia o app publicado
WORKDIR /app
COPY --from=build /app/out .

# Copia o requirements.txt
COPY MusicPlayerSite/requirements.txt /app/requirements.txt

# Instala Demucs e Torch via pip (sem erro PEP 668)
RUN pip3 install --no-cache-dir --upgrade pip && \
    pip3 install --no-cache-dir -r /app/requirements.txt

# Configura porta do ASP.NET Core
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 10000

ENTRYPOINT ["dotnet", "MusicPlayerSite.dll"]