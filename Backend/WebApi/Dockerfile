# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Backend/DarkKitchen.sln Backend/

COPY Backend/BusinessLogic/BusinessLogic.csproj Backend/BusinessLogic/
COPY Backend/Domain/Domain.csproj Backend/Domain/
COPY Backend/DTOs/DTOs.csproj Backend/DTOs/
COPY Backend/IBussinesLogic/IBussinesLogic.csproj Backend/IBussinesLogic/
COPY Backend/IImporters/IImporters.csproj Backend/IImporters/
COPY Backend/IRepository/IRepository.csproj Backend/IRepository/
COPY Backend/Repository/Repository.csproj Backend/Repository/
COPY Backend/ServiceFactory/ServiceFactory.csproj Backend/ServiceFactory/
COPY Backend/WebApi/WebApi.csproj Backend/WebApi/

COPY JsonImporter/JsonImporter/JsonImporter.csproj JsonImporter/JsonImporter/
COPY XmlImporter/XmlImporter/XmlImporter.csproj XmlImporter/XmlImporter/

RUN dotnet restore Backend/WebApi/WebApi.csproj

COPY Backend/ Backend/
COPY JsonImporter/ JsonImporter/
COPY XmlImporter/ XmlImporter/

RUN dotnet build JsonImporter/JsonImporter/JsonImporter.csproj -c Release
RUN dotnet build XmlImporter/XmlImporter/XmlImporter.csproj -c Release

RUN dotnet publish Backend/WebApi/WebApi.csproj \
    -c Release \
    -o /app/publish

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:5222
EXPOSE 5222

ENTRYPOINT ["dotnet", "WebApi.dll"]