#Requires -Version 5.1
<#
.SYNOPSIS
  Backup PostgreSQL + MongoDB - xuat file doc duoc (SQL + JSON) hoac dinh dang restore (dump/bson).

.EXAMPLE
  .\scripts\backup\backup-databases.ps1
  .\scripts\backup\backup-databases.ps1 -PostgresFormat Dump -MongoFormat Bson
#>
param(
    [string]$OutputDir = "",
    [string]$EnvFile = ".env.docker",
    [ValidateSet("Sql", "Dump")]
    [string]$PostgresFormat = "Sql",
    [ValidateSet("Json", "Bson")]
    [string]$MongoFormat = "Json",
    [switch]$PostgresOnly,
    [switch]$MongoOnly
)

$ErrorActionPreference = "Stop"
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
Set-Location $RepoRoot

function Read-DotEnv([string]$path) {
    $map = @{}
    if (-not (Test-Path $path)) { return $map }
    Get-Content $path | ForEach-Object {
        $line = $_.Trim()
        if ($line -eq "" -or $line.StartsWith("#")) { return }
        $eq = $line.IndexOf("=")
        if ($eq -lt 1) { return }
        $k = $line.Substring(0, $eq).Trim()
        $v = $line.Substring($eq + 1).Trim().Trim('"').Trim("'")
        $map[$k] = $v
    }
    return $map
}

function Get-EnvOrDefault($map, [string]$key, [string]$default) {
    if ($map.ContainsKey($key) -and $map[$key]) { return $map[$key] }
    return $default
}

$envMap = Read-DotEnv (Join-Path $RepoRoot $EnvFile)

$pgHost = Get-EnvOrDefault $envMap "DB_BIND_HOST" "127.0.0.1"
$pgPort = Get-EnvOrDefault $envMap "POSTGRES_PORT" "5433"
$pgUser = Get-EnvOrDefault $envMap "POSTGRES_USER" "unihub"
$pgPass = Get-EnvOrDefault $envMap "POSTGRES_PASSWORD" "unihub_dev_2026"
$pgDb   = Get-EnvOrDefault $envMap "POSTGRES_DB" "unihub"

$mongoHost = Get-EnvOrDefault $envMap "DB_BIND_HOST" "127.0.0.1"
$mongoPort = Get-EnvOrDefault $envMap "MONGODB_PORT" "27018"
$mongoUser = Get-EnvOrDefault $envMap "MONGO_ROOT_USER" "unihub"
$mongoPass = Get-EnvOrDefault $envMap "MONGO_ROOT_PASSWORD" "unihub_dev_2026"

$mongoDb = Get-EnvOrDefault $envMap "MONGO_BACKUP_DB" ""
if (-not $mongoDb) {
    $appDevJson = Join-Path $RepoRoot "src\UniHub.API\appsettings.Development.json"
    if (Test-Path $appDevJson) {
        try {
            $dev = Get-Content $appDevJson -Raw | ConvertFrom-Json
            if ($dev.MongoDbSettings.DatabaseName) {
                $mongoDb = [string]$dev.MongoDbSettings.DatabaseName
            }
        } catch { }
    }
}
if (-not $mongoDb) {
    $mongoDb = Get-EnvOrDefault $envMap "MONGO_DB" "unihub_db_dev"
}

if (-not $OutputDir) {
    $stamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $OutputDir = Join-Path $RepoRoot "backups\export_$stamp"
}
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$pgContainer = "unihub-postgres"
$mongoContainer = "unihub-mongodb"
$mongoUriDocker = "mongodb://${mongoUser}:${mongoPass}@localhost:27017/?authSource=admin"
$mongoUriHost = "mongodb://${mongoUser}:$([uri]::EscapeDataString($mongoPass))@${mongoHost}:${mongoPort}/?authSource=admin"

function Test-DockerContainerRunning([string]$name) {
    try {
        $state = docker inspect -f "{{.State.Running}}" $name 2>$null
        return $state -eq "true"
    } catch {
        return $false
    }
}

function Invoke-PgBackup {
    $ext = if ($PostgresFormat -eq "Sql") { "sql" } else { "dump" }
    $outFile = Join-Path $OutputDir "postgres_$pgDb.$ext"
    $pgRunning = Test-DockerContainerRunning $pgContainer
    if ($pgRunning) {
        Write-Host "[postgres] Docker -> .$ext (SQL text = mo bang editor, restore: psql) ..."
        $tmpDump = "/tmp/unihub_pg_backup.$ext"
        if ($PostgresFormat -eq "Sql") {
            docker exec -e "PGPASSWORD=$pgPass" $pgContainer pg_dump -U $pgUser -d $pgDb -Fp --no-owner --no-acl -f $tmpDump
        } else {
            docker exec -e "PGPASSWORD=$pgPass" $pgContainer pg_dump -U $pgUser -d $pgDb -Fc --no-owner --no-acl -f $tmpDump
        }
        if ($LASTEXITCODE -ne 0) { throw "pg_dump via docker failed (exit $LASTEXITCODE)" }
        docker cp "${pgContainer}:${tmpDump}" $outFile
        if ($LASTEXITCODE -ne 0) { throw "docker cp postgres backup failed" }
        docker exec $pgContainer rm -f $tmpDump | Out-Null
        return $outFile
    }

    $pgDump = Get-Command pg_dump -ErrorAction SilentlyContinue
    if (-not $pgDump) {
        throw "pg_dump not found. Start Docker ($pgContainer) or install PostgreSQL client tools."
    }

    Write-Host "[postgres] Host $pgHost`:$pgPort -> .$ext ..."
    $env:PGPASSWORD = $pgPass
    if ($PostgresFormat -eq "Sql") {
        & pg_dump -h $pgHost -p $pgPort -U $pgUser -d $pgDb -Fp --no-owner --no-acl -f $outFile
    } else {
        & pg_dump -h $pgHost -p $pgPort -U $pgUser -d $pgDb -Fc --no-owner --no-acl -f $outFile
    }
    if ($LASTEXITCODE -ne 0) { throw "pg_dump failed (exit $LASTEXITCODE)" }
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    return $outFile
}

function Get-MongoCollectionNames([bool]$useDocker) {
    $js = "JSON.stringify(db.getSiblingDB('$mongoDb').getCollectionNames())"
    if ($useDocker) {
        $raw = docker exec $mongoContainer mongosh $mongoUriDocker --quiet --eval $js 2>&1
    } else {
        $raw = mongosh $mongoUriHost --quiet --eval $js 2>&1
    }
    if ($LASTEXITCODE -ne 0) { throw "mongosh list collections failed: $raw" }
    $names = $raw | ConvertFrom-Json
    if ($names -is [string]) { return @($names) }
    return @($names)
}

function Invoke-MongoJsonExport {
    $outDir = Join-Path $OutputDir "mongodb_${mongoDb}_json"
    $jsonDir = Join-Path $outDir "collections"
    New-Item -ItemType Directory -Force -Path $jsonDir | Out-Null
    $mongoRunning = Test-DockerContainerRunning $mongoContainer

    Write-Host "[mongodb] Export JSON (mongoexport) - moi collection mot file .json ..."
    $collections = Get-MongoCollectionNames $mongoRunning
    if ($collections.Count -eq 0) {
        Write-Warning "No collections in database '$mongoDb'."
        return $outDir
    }

    foreach ($coll in $collections) {
        $safeName = $coll -replace '[^\w\-.]', '_'
        $hostFile = Join-Path $jsonDir "$safeName.json"
        Write-Host "  - $coll -> $safeName.json"

        if ($mongoRunning) {
            $tmpOut = "/tmp/mongoexp_$safeName.json"
            docker exec $mongoContainer mongoexport `
                --uri=$mongoUriDocker `
                --db=$mongoDb `
                --collection=$coll `
                --jsonArray `
                --out=$tmpOut
            if ($LASTEXITCODE -ne 0) { throw "mongoexport failed for $coll" }
            docker cp "${mongoContainer}:${tmpOut}" $hostFile
            if ($LASTEXITCODE -ne 0) { throw "docker cp failed for $coll" }
            docker exec $mongoContainer rm -f $tmpOut | Out-Null
        } else {
            $mongoexport = Get-Command mongoexport -ErrorAction SilentlyContinue
            if (-not $mongoexport) {
                throw "mongoexport not found. Start Docker ($mongoContainer) or install MongoDB Database Tools."
            }
            & mongoexport --uri=$mongoUriHost --db=$mongoDb --collection=$coll --jsonArray --out=$hostFile
            if ($LASTEXITCODE -ne 0) { throw "mongoexport failed for $coll" }
        }
    }

    @{
        database    = $mongoDb
        format      = "json"
        exportedAt  = (Get-Date).ToUniversalTime().ToString("o")
        collections = $collections
    } | ConvertTo-Json | Set-Content -Path (Join-Path $outDir "_index.json") -Encoding UTF8

    return $outDir
}

function Invoke-MongoBsonDump {
    $outDir = Join-Path $OutputDir "mongodb_${mongoDb}_bson"
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null
    $mongoRunning = Test-DockerContainerRunning $mongoContainer

    if ($mongoRunning) {
        Write-Host "[mongodb] mongodump BSON (.bson.gz) - dung mongorestore, khong phai JSON ..."
        $tmpInContainer = "/tmp/unihub_mongodump"
        docker exec $mongoContainer mongodump --uri=$mongoUriDocker --db=$mongoDb --gzip --out=$tmpInContainer
        if ($LASTEXITCODE -ne 0) { throw "mongodump via docker failed (exit $LASTEXITCODE)" }
        docker cp "${mongoContainer}:${tmpInContainer}/." $outDir
        if ($LASTEXITCODE -ne 0) { throw "docker cp mongodump failed" }
        docker exec $mongoContainer rm -rf $tmpInContainer | Out-Null
        return $outDir
    }

    $mongodump = Get-Command mongodump -ErrorAction SilentlyContinue
    if (-not $mongodump) {
        throw "mongodump not found. Start Docker ($mongoContainer) or install MongoDB Database Tools."
    }

    Write-Host "[mongodb] Host mongodump ..."
    & mongodump --uri=$mongoUriHost --db=$mongoDb --gzip --out=$outDir
    if ($LASTEXITCODE -ne 0) { throw "mongodump failed (exit $LASTEXITCODE)" }
    return $outDir
}

$pgNote = if ($PostgresFormat -eq "Sql") {
    "postgres_*.sql = plain SQL (psql -f file.sql). Đọc/sửa được bằng text editor."
} else {
    "postgres_*.dump = pg_restore format. Nhỏ hơn SQL, restore: pg_restore ..."
}
$mongoNote = if ($MongoFormat -eq "Json") {
    "mongodb_*_json/collections/*.json = mongoexport --jsonArray. Đọc được, import lại phức tạp hơn bson."
} else {
    "mongodb_*_bson = mongodump. Restore: mongorestore --drop ..."
}

$meta = @{
    createdAtUtc   = (Get-Date).ToUniversalTime().ToString("o")
    postgresFormat = $PostgresFormat
    mongoFormat    = $MongoFormat
    postgres       = @{ host = $pgHost; port = $pgPort; database = $pgDb; user = $pgUser }
    mongodb        = @{ host = $mongoHost; port = $mongoPort; database = $mongoDb; user = $mongoUser }
    notes          = @($pgNote, $mongoNote)
}
$meta | ConvertTo-Json -Depth 5 | Set-Content -Path (Join-Path $OutputDir "backup-manifest.json") -Encoding UTF8

Write-Host ""
Write-Host "UniHub backup -> $OutputDir"
Write-Host "  Postgres: $PostgresFormat  |  Mongo: $MongoFormat"
Write-Host ""

$results = @()
if (-not $MongoOnly) {
    try {
        $f = Invoke-PgBackup
        $results += "PostgreSQL: $f"
    } catch {
        $results += "PostgreSQL: FAILED - $($_.Exception.Message)"
        Write-Warning $_.Exception.Message
    }
}

if (-not $PostgresOnly) {
    try {
        $d = if ($MongoFormat -eq "Json") { Invoke-MongoJsonExport } else { Invoke-MongoBsonDump }
        $results += "MongoDB: $d"
    } catch {
        $results += "MongoDB: FAILED - $($_.Exception.Message)"
        Write-Warning $_.Exception.Message
    }
}

@"
UniHub backup
Created: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Postgres format: $PostgresFormat
Mongo format: $MongoFormat
Folder: $OutputDir

$($results -join "`n")

$pgNote
$mongoNote
"@ | Set-Content -Path (Join-Path $OutputDir "README.txt") -Encoding UTF8

Write-Host ""
Write-Host "Done."
$results | ForEach-Object { Write-Host "  $_" }
Write-Host ""
Write-Host "Folder: $OutputDir"
