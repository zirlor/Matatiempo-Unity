# BuildAndroid.ps1
# Limpia caché de Gradle/Unity y lanza build AAB en batch mode

$ProjectRoot = "C:\Users\zirow\source\repos\IMRtecnologygroup\polixgamestudio\Paper Hangman Matatiempo\Paper Hangman Matatiempo"
$RepoRoot    = "C:\Users\zirow\source\repos\IMRtecnologygroup\polixgamestudio\Paper Hangman Matatiempo"
$UnityExe    = "C:\Program Files\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe"
$LogFile     = Join-Path $ProjectRoot "BuildLogs\UnityBuild_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

New-Item -ItemType Directory -Force -Path (Split-Path $LogFile) | Out-Null

Write-Host "Limpiando carpetas temporales..."

# Carpeta de Gradle generada por Unity
$Produccion = Join-Path $RepoRoot "produccion"
if (Test-Path $Produccion) {
    Write-Host "Eliminando $Produccion"
    Remove-Item -Recurse -Force $Produccion -ErrorAction SilentlyContinue
}

# Caché de Bee
$Bee = Join-Path $ProjectRoot "Library\Bee"
if (Test-Path $Bee) {
    Write-Host "Eliminando $Bee"
    Remove-Item -Recurse -Force $Bee -ErrorAction SilentlyContinue
}

# Limpiar build anterior
$BuildOutputAab = Join-Path $ProjectRoot "Builds\Matatiempo.aab"
if (Test-Path $BuildOutputAab) {
    Write-Host "Eliminando build anterior $BuildOutputAab"
    Remove-Item -Recurse -Force $BuildOutputAab -ErrorAction SilentlyContinue
}
$BuildOutputApk = Join-Path $ProjectRoot "Builds\Matatiempo.apk"
if (Test-Path $BuildOutputApk) {
    Write-Host "Eliminando build anterior $BuildOutputApk"
    Remove-Item -Recurse -Force $BuildOutputApk -ErrorAction SilentlyContinue
}

Write-Host "Lanzando Unity en batch mode para AAB..."
$LogFileAab = Join-Path $ProjectRoot "BuildLogs\UnityBuild_AAB_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
$ArgsAab = @(
    "-batchmode",
    "-quit",
    "-projectPath", "`"$ProjectRoot`"",
    "-executeMethod", "BuildAAB.Build",
    "-logFile", "`"$LogFileAab`""
)
$ProcAab = Start-Process -FilePath $UnityExe -ArgumentList $ArgsAab -Wait -PassThru
Write-Host "Build AAB finalizó con exit code $($ProcAab.ExitCode)"
Write-Host "Log: $LogFileAab"

$Success = $true

if ($ProcAab.ExitCode -eq 0) {
    $AabPath = Join-Path $ProjectRoot "Builds\Matatiempo.aab"
    Write-Host "✅ Build AAB exitoso. AAB esperado en: $AabPath"
    if (Test-Path $AabPath) {
        $Size = (Get-Item $AabPath).Length
        Write-Host "Tamaño AAB: $([math]::Round($Size/1MB,2)) MB"
        $ReleasesDir = Join-Path $ProjectRoot "Releases"
        New-Item -ItemType Directory -Force -Path $ReleasesDir | Out-Null
        $Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $ReleasePath = Join-Path $ReleasesDir "Matatiempo_$Timestamp.aab"
        Copy-Item -Path $AabPath -Destination $ReleasePath -Force
        Write-Host "📦 Copiado AAB a Releases: $ReleasePath"
    } else {
        Write-Host "⚠️ No se encontró archivo AAB en la ruta esperada. Revisa el log."
        $Success = $false
    }
} else {
    Write-Host "❌ Build AAB falló. Revisa el log."
    $Success = $false
}

if ($Success) {
    Write-Host "Lanzando Unity en batch mode para APK..."
    $LogFileApk = Join-Path $ProjectRoot "BuildLogs\UnityBuild_APK_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
    $ArgsApk = @(
        "-batchmode",
        "-quit",
        "-projectPath", "`"$ProjectRoot`"",
        "-executeMethod", "BuildAAB.BuildAPK",
        "-logFile", "`"$LogFileApk`""
    )
    $ProcApk = Start-Process -FilePath $UnityExe -ArgumentList $ArgsApk -Wait -PassThru
    Write-Host "Build APK finalizó con exit code $($ProcApk.ExitCode)"
    Write-Host "Log: $LogFileApk"
    if ($ProcApk.ExitCode -eq 0) {
        $ApkPath = Join-Path $ProjectRoot "Builds\Matatiempo.apk"
        Write-Host "✅ Build APK exitoso. APK esperado en: $ApkPath"
        if (Test-Path $ApkPath) {
            $SizeApk = (Get-Item $ApkPath).Length
            Write-Host "Tamaño APK: $([math]::Round($SizeApk/1MB,2)) MB"
            $TimestampApk = Get-Date -Format "yyyyMMdd_HHmmss"
            $ReleasePathApk = Join-Path $ReleasesDir "Matatiempo_$TimestampApk.apk"
            Copy-Item -Path $ApkPath -Destination $ReleasePathApk -Force
            Write-Host "📦 Copiado APK a Releases: $ReleasePathApk"
        } else {
            Write-Host "⚠️ No se encontró archivo APK en la ruta esperada. Revisa el log."
        }
    } else {
        Write-Host "❌ Build APK falló. Revisa el log."
    }
}
