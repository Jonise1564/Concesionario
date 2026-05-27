# 1. Etapa de compilación usando el SDK de .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copiar el archivo de proyecto y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar todo el resto del código y compilar en modo Release
COPY . ./
RUN dotnet publish -c Release -o out

# 2. Etapa de ejecución usando el Runtime de .NET 10 (más liviano)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

# Configurar la variable de entorno para que escuche en el puerto de Render
ENV ASPNETCORE_URLS=http://+:10000

# Comando para arrancar la aplicación
ENTRYPOINT ["dotnet", "Concesionario.dll"]