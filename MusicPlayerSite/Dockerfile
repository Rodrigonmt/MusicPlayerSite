# Etapa 1 - build .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY MusicPlayerSite/*.csproj ./MusicPlayerSite/
WORKDIR /app/MusicPlayerSite
RUN dotnet restore

WORKDIR /app
COPY . .

WORKDIR /app/MusicPlayerSite
RUN dotnet publish -c Release -o /app/out

# Etapa 2 - runtime com Python + Demucs
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    ffmpeg \
    git \
    && rm -rf /var/lib/apt/lists/*

RUN pip3 install --break-system-packages demucs diffq

WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 80

ENTRYPOINT ["dotnet", "MusicPlayerSite.dll"]
