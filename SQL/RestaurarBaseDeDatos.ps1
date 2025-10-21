# Script para restaurar la base de datos CAPFISDb
# El archivo .bak debe estar en la misma carpeta que este script

# Configuracion
$DatabaseName = "CAPFISDb"
$ServerInstance = "localhost" # Cambia esto si tu SQL Server esta en otro servidor o instancia

# Obtener la ruta donde esta el script
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackupFile = Join-Path $ScriptPath "CAPFISDb.bak"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Restauracion de Base de Datos CAPFISDb" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Ruta del script: $ScriptPath" -ForegroundColor Yellow
Write-Host "Archivo de backup: $BackupFile" -ForegroundColor Yellow
Write-Host "Servidor: $ServerInstance" -ForegroundColor Yellow
Write-Host ""

# Verificar que el archivo .bak existe
if (-not (Test-Path $BackupFile)) {
    Write-Host "ERROR: No se encontro el archivo CAPFISDb.bak" -ForegroundColor Red
    Write-Host "Asegurate de que el archivo este en la misma carpeta que este script." -ForegroundColor Red
    Write-Host "Ruta esperada: $BackupFile" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}

Write-Host "Archivo de backup encontrado" -ForegroundColor Green
Write-Host ""

try {
    # Importar el modulo de SQL Server
    if (-not (Get-Module -ListAvailable -Name SqlServer)) {
        Write-Host "Instalando modulo SqlServer..." -ForegroundColor Yellow
        Install-Module -Name SqlServer -Force -AllowClobber -Scope CurrentUser
    }
    Import-Module SqlServer -ErrorAction Stop

    Write-Host "Modulo SqlServer cargado" -ForegroundColor Green
    Write-Host ""

    # Obtener informacion del archivo de backup
    Write-Host "Obteniendo informacion del backup..." -ForegroundColor Yellow
    $BackupFileList = Invoke-Sqlcmd -ServerInstance $ServerInstance -Query "RESTORE FILELISTONLY FROM DISK = '$BackupFile'" -ErrorAction Stop
    
    $DataLogicalName = ($BackupFileList | Where-Object { $_.Type -eq 'D' }).LogicalName
    $LogLogicalName = ($BackupFileList | Where-Object { $_.Type -eq 'L' }).LogicalName
    
    Write-Host "  - Archivo de datos (logico): $DataLogicalName" -ForegroundColor Gray
    Write-Host "  - Archivo de log (logico): $LogLogicalName" -ForegroundColor Gray
    Write-Host ""

    # Obtener rutas por defecto de SQL Server
    $DefaultPathsQuery = "DECLARE @DataPath NVARCHAR(500), @LogPath NVARCHAR(500); " +
        "EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultData', @DataPath OUTPUT; " +
        "EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultLog', @LogPath OUTPUT; " +
        "IF @DataPath IS NULL SELECT @DataPath = SUBSTRING(physical_name, 1, CHARINDEX(N'master.mdf', LOWER(physical_name)) - 1) FROM master.sys.master_files WHERE database_id = 1 AND file_id = 1; " +
        "IF @LogPath IS NULL SET @LogPath = @DataPath; " +
        "SELECT @DataPath AS DataPath, @LogPath AS LogPath;"

    $DefaultPaths = Invoke-Sqlcmd -ServerInstance $ServerInstance -Query $DefaultPathsQuery -ErrorAction Stop

    $DataPath = $DefaultPaths.DataPath
    $LogPath = $DefaultPaths.LogPath

    Write-Host "Rutas de destino:" -ForegroundColor Yellow
    Write-Host "  - Datos: $DataPath" -ForegroundColor Gray
    Write-Host "  - Log: $LogPath" -ForegroundColor Gray
    Write-Host ""

    # Verificar si la base de datos existe
    $DbExists = Invoke-Sqlcmd -ServerInstance $ServerInstance -Query "SELECT COUNT(*) AS Exists FROM sys.databases WHERE name = '$DatabaseName'" -ErrorAction Stop
    
    if ($DbExists.Exists -gt 0) {
        Write-Host "La base de datos '$DatabaseName' existe. Eliminandola..." -ForegroundColor Yellow
        
        # Poner en modo SINGLE_USER y dropear
        Invoke-Sqlcmd -ServerInstance $ServerInstance -Query "ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" -ErrorAction Stop
        Invoke-Sqlcmd -ServerInstance $ServerInstance -Query "DROP DATABASE [$DatabaseName];" -ErrorAction Stop
        
        Write-Host "Base de datos eliminada" -ForegroundColor Green
        Write-Host ""
    }

    # Restaurar la base de datos
    Write-Host "Restaurando base de datos..." -ForegroundColor Yellow
    
    $RestoreQuery = "RESTORE DATABASE [$DatabaseName] FROM DISK = '$BackupFile' WITH " +
        "MOVE '$DataLogicalName' TO '$DataPath$DatabaseName.mdf', " +
        "MOVE '$LogLogicalName' TO '$LogPath$($DatabaseName)_log.ldf', " +
        "REPLACE, RECOVERY;"

    Invoke-Sqlcmd -ServerInstance $ServerInstance -Query $RestoreQuery -QueryTimeout 300 -ErrorAction Stop
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  Restauracion Completada" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "La base de datos '$DatabaseName' esta lista para usar." -ForegroundColor Cyan

} catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ERROR" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Detalles del error:" -ForegroundColor Yellow
    Write-Host $_.Exception -ForegroundColor Gray
    Read-Host "Presiona Enter para salir"
    exit 1
}

Write-Host ""
Read-Host "Presiona Enter para salir"