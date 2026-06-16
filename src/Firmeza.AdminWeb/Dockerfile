# Etapa 1: Compilación de la aplicación
FROM ://microsoft.com AS build
WORKDIR /app

# Copiar archivos de proyectos y restaurar dependencias
COPY Firmeza.sln ./
COPY src/Firmeza.Core/Firmeza.Core.csproj src/Firmeza.Core/
COPY src/Firmeza.Infrastructure/Firmeza.Infrastructure.csproj src/Firmeza.Infrastructure/
COPY src/Firmeza.AdminWeb/Firmeza.AdminWeb.csproj src/Firmeza.AdminWeb/
COPY tests/Firmeza.Tests/Firmeza.Tests.csproj tests/Firmeza.Tests/
RUN dotnet restore

# Copiar el código completo y compilar en modo Release
COPY . ./
RUN dotnet publish src/Firmeza.AdminWeb/Firmeza.AdminWeb.csproj -c Release -o /app/publish

# Etapa 2: Entorno de ejecución ligero
FROM ://microsoft.com AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Firmeza.AdminWeb.dll"]
