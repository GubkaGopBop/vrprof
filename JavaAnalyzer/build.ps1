$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    New-Item -ItemType Directory -Force 'build\classes','build\tests','build\lib' | Out-Null
    $javaSources = @(Get-ChildItem -LiteralPath 'src\vrprof' -Filter '*.java' | ForEach-Object FullName)
    & javac --release 17 -encoding UTF-8 -cp 'lib\gson-2.13.2.jar' -d 'build\classes' @javaSources
    if ($LASTEXITCODE -ne 0) { throw 'Компиляция Java не прошла.' }
    Copy-Item -LiteralPath 'lib\gson-2.13.2.jar' -Destination 'build\lib\gson-2.13.2.jar' -Force
    "Manifest-Version: 1.0`nMain-Class: vrprof.Main`nClass-Path: lib/gson-2.13.2.jar`n`n" | Set-Content 'build\MANIFEST.MF' -Encoding ascii
    $jarTool = Get-Command jar -ErrorAction SilentlyContinue
    if ($jarTool) { $jarPath = $jarTool.Source }
    else {
        $javaProperties = & java -XshowSettings:properties -version 2>&1
        $javaHomeLine = $javaProperties | Where-Object { "$_" -match '^\s*java.home\s*=' } | Select-Object -First 1
        if (!$javaHomeLine) { throw 'Не удалось определить каталог JDK. Добавьте его bin в PATH.' }
        $javaHomePath = ("$javaHomeLine" -split '=',2)[1].Trim()
        $jarPath = Join-Path $javaHomePath 'bin\jar.exe'
    }
    & $jarPath --create --file 'build\vrprof-analyzer.jar' --manifest 'build\MANIFEST.MF' -C 'build\classes' .
    if ($LASTEXITCODE -ne 0) { throw 'Создание JAR не прошло.' }
    Write-Output 'Создан build/vrprof-analyzer.jar; требуется Java 17 или новее.'
} finally { Pop-Location }
