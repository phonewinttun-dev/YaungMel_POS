# Yaung Mel POS System

## Migration commands

Run this in YaungMel_POS\YaungMel_POS.WebApi.csproj:

Migration script:

```cmd
dotnet ef migrations add InitialCreate --project YaungMel_POS.Database --startup-project YaungMel_POS.WebApi
```

```cmd
dotnet ef database update
```

Please use quotation when viewing tables in PgAdmin or DataGrip:

```sql
SELECT * FROM "PRODUCTS";
```

## Docker commands

Run this in the root directory (where docker-compose.yml exists):

```cmd
docker compose up -d --build
```

apply database migration (only run this if you are initializing the database) :

```cmd
dotnet ef database update --project YaungMel_POS.Database --startup-project YaungMel_POS.WebApi
```

to run docker:

```cmd
docker compose up
```

to stop docker:

```cmd
docker compose down
```

To check if docker is running:

```cmd
docker compose ps
```

Database drop command (run this only when needed): 
```cmd
dotnet ef database drop --project YaungMel_POS.Database --startup-project YaungMel_POS.WebApi
```