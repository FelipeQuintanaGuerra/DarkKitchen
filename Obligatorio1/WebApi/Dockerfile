# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Obligatorio1/DarkKitchen.sln Obligatorio1/

COPY Obligatorio1/BusinessLogic/BusinessLogic.csproj Obligatorio1/BusinessLogic/
COPY Obligatorio1/Domain/Domain.csproj Obligatorio1/Domain/
COPY Obligatorio1/DTOs/DTOs.csproj Obligatorio1/DTOs/
COPY Obligatorio1/IBussinesLogic/IBussinesLogic.csproj Obligatorio1/IBussinesLogic/
COPY Obligatorio1/IImporters/IImporters.csproj Obligatorio1/IImporters/
COPY Obligatorio1/IRepository/IRepository.csproj Obligatorio1/IRepository/
COPY Obligatorio1/Repository/Repository.csproj Obligatorio1/Repository/
COPY Obligatorio1/ServiceFactory/ServiceFactory.csproj Obligatorio1/ServiceFactory/
COPY Obligatorio1/WebApi/WebApi.csproj Obligatorio1/WebApi/

COPY JsonImporter/JsonImporter/JsonImporter.csproj JsonImporter/JsonImporter/
COPY XmlImporter/XmlImporter/XmlImporter.csproj XmlImporter/XmlImporter/

RUN dotnet restore Obligatorio1/WebApi/WebApi.csproj

COPY Obligatorio1/ Obligatorio1/
COPY JsonImporter/ JsonImporter/
COPY XmlImporter/ XmlImporter/

RUN dotnet build JsonImporter/JsonImporter/JsonImporter.csproj -c Release
RUN dotnet build XmlImporter/XmlImporter/XmlImporter.csproj -c Release

RUN dotnet publish Obligatorio1/WebApi/WebApi.csproj \
    -c Release \
    -o /app/publish

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:5222
EXPOSE 5222

ENTRYPOINT ["dotnet", "WebApi.dll"]