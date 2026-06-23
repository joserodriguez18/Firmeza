FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Firmeza.slnx ./
COPY src/Firmeza.Core/Firmeza.Core.csproj src/Firmeza.Core/
COPY src/Firmeza.Infrastructure/Firmeza.Infrastructure.csproj src/Firmeza.Infrastructure/
COPY src/Firmeza.Api/Firmeza.Api.csproj src/Firmeza.Api/
RUN dotnet restore src/Firmeza.Api/Firmeza.Api.csproj

COPY . ./
RUN dotnet publish src/Firmeza.Api/Firmeza.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8081
ENTRYPOINT ["dotnet", "Firmeza.Api.dll"]
