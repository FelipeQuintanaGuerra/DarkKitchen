# Guía para levantar deploy en otra PC

## Requisitos

```text
Docker Desktop
.NET 8 SDK
Repositorio clonado
```

---

## 1. Clonar repo

```powershell
git clone <url-del-repo>
cd DarkKitchen
```

---

## 2. Verificar imágenes

Deben estar en:

```text
Backend/WebApi/wwwroot/images/products
```

Con estos nombres:

```text
pizza-muzzarella.jpg
pizza-pepperoni.jpg
empanada-carne.jpg
empanada-jamon-queso.jpg
hamburguesa-clasica.jpg
hamburguesa-doble.jpg
```

---

## 3. Limpiar contenedores anteriores

```powershell
docker rm -f frontend api sql 2>$null
docker network rm da2-network 2>$null
```

---

## 4. Crear red Docker

```powershell
docker network create da2-network
```

---

## 5. Levantar SQL

```powershell
docker run --network da2-network `
  -e "ACCEPT_EULA=Y" `
  -e "MSSQL_SA_PASSWORD=La_Contrasenia123" `
  -e "MSSQL_PID=Developer" `
  -p 1433:1433 `
  -d --name=sql `
  mcr.microsoft.com/mssql/server:2022-latest
```

Esperar aproximadamente 30 segundos para que SQL Server termine de iniciar.

---

# 6. Crear base y cargar datos

## Caso A: migraciones + script por consola

### Configurar connection string

Archivo:

```text
Backend/WebApi/appsettings.Development.json
```

Contenido:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DarkKitchen;User Id=SA;Password=La_Contrasenia123;TrustServerCertificate=True;Encrypt=False"
  }
}
```

### Ejecutar migraciones

```powershell
cd Backend

dotnet ef database update `
  -p Repository\Repository.csproj `
  -s WebApi\WebApi.csproj

cd ..
```

### Copiar script al contenedor

```powershell
docker cp Datos/database-with-test-data.sql sql:/tmp/database-with-test-data.sql
```

### Ejecutar script

```powershell
docker exec -it sql /opt/mssql-tools18/bin/sqlcmd `
  -S localhost `
  -U SA `
  -P "La_Contrasenia123" `
  -C `
  -i /tmp/database-with-test-data.sql
```

---

## Caso B: migraciones + script desde SQL Server Management Studio

### Configurar connection string

Archivo:

```text
Backend/WebApi/appsettings.Development.json
```

Contenido:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DarkKitchen;User Id=SA;Password=La_Contrasenia123;TrustServerCertificate=True;Encrypt=False"
  }
}
```

### Ejecutar migraciones

```powershell
cd Backend

dotnet ef database update `
  -p Repository\Repository.csproj `
  -s WebApi\WebApi.csproj

cd ..
```

### Abrir SQL Server Management Studio

Conectarse con:

```text
Server: localhost,1433
Authentication: SQL Server Authentication
User: SA
Password: La_Contrasenia123
Trust Server Certificate: True
```

### Ejecutar script

Abrir:

```text
Datos/database-with-test-data.sql
```

y ejecutarlo.

---

## 7. Build de la API

Desde la raíz del repositorio:

```powershell
docker build --rm -f Backend/WebApi/Dockerfile -t deploy/api:latest .
```

---

## 8. Levantar API

```powershell
docker run --network da2-network `
  -e "ConnectionStrings__DefaultConnection=Server=sql,1433;Database=DarkKitchen;User Id=SA;Password=La_Contrasenia123;TrustServerCertificate=True;Encrypt=False" `
  -p 5222:5222 `
  -d --name=api `
  deploy/api:latest
```

### Verificar API

```powershell
docker logs api
```

Debe aparecer algo similar a:

```text
Now listening on: http://[::]:5222
```

---

## 9. Build frontend

```powershell
cd Frontend

docker build . -t deploy/frontend:latest
```

---

## 10. Levantar frontend

```powershell
docker run --network da2-network `
  --name frontend `
  -d `
  -p 8080:80 `
  deploy/frontend:latest
```

---

## 11. Verificar contenedores

```powershell
docker ps
```

Deben aparecer:

```text
sql
api
frontend
```

Opcionalmente:

```powershell
docker network inspect da2-network
```

---

## 12. Abrir aplicación

```text
http://localhost:8080
```

### Usuario administrador de prueba

```text
Email:
admin@darkkitchen.com

Contraseña:
AdminPass2026!X
```

---

# Arquitectura final

```text
Frontend (localhost:8080)
            │
            ▼
API (localhost:5222)
            │
            ▼
SQL Server (localhost:1433)
```

Dentro de Docker:

```text
frontend
   │
   ▼
api
   │
   ▼
sql
```

La API se conecta a SQL utilizando:

```text
Server=sql,1433
```

porque ambos contenedores están dentro de la red:

```text
da2-network
```