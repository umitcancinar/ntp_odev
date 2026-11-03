param(
    [ValidateSet('Menu', 'Check', 'Build', 'Tests', 'Publish')]
    [string]$Action = 'Menu',
    [ValidateSet('All', 'FlappyBird', 'KeyEventLogger', 'ChatApp', 'SpeechApp', 'FaceRecognition')]
    [string]$Project = 'All'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$projects = @(
    [pscustomobject]@{ Name='FlappyBird'; Repo='YMH219-FlappyBird'; Solution='FlappyBird.sln' },
    [pscustomobject]@{ Name='KeyEventLogger'; Repo='YMH219-KeyEventLogger'; Solution='KeyEventLogger.sln' },
    [pscustomobject]@{ Name='ChatApp'; Repo='YMH219-ChatApp'; Solution='ChatApp.sln' },
    [pscustomobject]@{ Name='SpeechApp'; Repo='YMH219-SpeechApp'; Solution='YMH219-SpeechApp.sln' },
    [pscustomobject]@{ Name='FaceRecognition'; Repo='YMH219-FaceRecognition'; Solution='FaceRecognition.sln' }
)

function Require-Sdk {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw '.NET 10 SDK yok. https://dotnet.microsoft.com/download/dotnet/10.0 adresinden x64 SDK kurun; terminali yeniden acin. Alternatif: winget install --id Microsoft.DotNet.SDK.10 --exact'
    }
    $sdkLines = @(& dotnet --list-sdks)
    if ($LASTEXITCODE -ne 0 -or -not ($sdkLines | Where-Object { $_ -match '^10\.' })) {
        throw '.NET 10 SDK bulunamadi. Sadece Runtime degil SDK kurun.'
    }
}

function Invoke-Dotnet {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet islemi basarisiz (kod $LASTEXITCODE): $($Arguments -join ' ')" }
}

function Check-Sources {
    param([object[]]$Items)
    foreach ($item in $Items) {
        $source = Join-Path (Join-Path $PSScriptRoot $item.Repo) $item.Solution
        if (-not (Test-Path -LiteralPath $source)) { throw "Eksik cozum: $source" }
    }
    Write-Host 'Kaynak cozum yollari dogrulandi.'
}

function Build-Project {
    param($Item)
    Require-Sdk
    $solution = Join-Path (Join-Path $PSScriptRoot $Item.Repo) $Item.Solution
    if (-not (Test-Path -LiteralPath $solution)) { throw 'Kaynak kod paketi bu klasorde yok. Kaynak ZIP dosyasini da acin.' }
    Invoke-Dotnet -Arguments @('build', $solution, '-c', 'Release')
}

function Test-Project {
    param($Item)
    Require-Sdk
    $test = Join-Path (Join-Path $PSScriptRoot $Item.Repo) "tests/$($Item.Name).Tests"
    Invoke-Dotnet -Arguments @('run', '--project', $test, '-c', 'Release')
}

function Open-Project {
    param($Item)
    if ($env:OS -ne 'Windows_NT') { throw 'Uygulama arayuzleri Windows gerektirir.' }
    $portable = Join-Path $PSScriptRoot "windows-apps/$($Item.Name)/$($Item.Name).exe"
    if (Test-Path -LiteralPath $portable) {
        Start-Process -FilePath $portable -WorkingDirectory (Split-Path -Parent $portable) | Out-Null
        return
    }
    Build-Project -Item $Item
    $exe = Join-Path (Join-Path $PSScriptRoot $Item.Repo) "src/$($Item.Name)/bin/Release/net10.0-windows/$($Item.Name).exe"
    if (-not (Test-Path -LiteralPath $exe)) { throw "Uygulama bulunamadi: $exe" }
    Start-Process -FilePath $exe -WorkingDirectory (Split-Path -Parent $exe) | Out-Null
}

function Publish-Project {
    param($Item)
    Require-Sdk
    $source = Join-Path (Join-Path $PSScriptRoot $Item.Repo) "src/$($Item.Name)/$($Item.Name).csproj"
    $destination = Join-Path $PSScriptRoot "windows-apps/$($Item.Name)"
    Invoke-Dotnet -Arguments @('publish', $source, '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-p:PublishSingleFile=true', '-p:IncludeNativeLibrariesForSelfExtract=true', '-p:EnableCompressionInSingleFile=true', '-o', $destination)
}

try {
    $selected = @($projects | Where-Object { $Project -eq 'All' -or $_.Name -eq $Project })
    switch ($Action) {
        'Check' {
            Require-Sdk
            Invoke-Dotnet -Arguments @('--info')
            Check-Sources -Items $selected
            Write-Host 'SDK ve kaynak yollar bulundu. Gercek UI, kamera ve mikrofon kontrolu ayrica yapilmalidir.'
        }
        'Build' { foreach ($item in $selected) { Build-Project -Item $item } }
        'Tests' { foreach ($item in $selected) { Test-Project -Item $item } }
        'Publish' { foreach ($item in $selected) { Publish-Project -Item $item } }
        'Menu' {
            while ($true) {
                Write-Host ''
                Write-Host 'YMH219 - Windows uygulamalari'
                Write-Host '1  Flappy Bird'
                Write-Host '2  Key Event Logger'
                Write-Host '3  Chat (iki pencere acar)'
                Write-Host '4  Speech-to-Text / Text-to-Speech'
                Write-Host '5  Yuz Algilama ve Tanima'
                Write-Host 'B  Tum kaynaklari derle'
                Write-Host 'T  Tum davranis testlerini calistir'
                Write-Host 'K  SDK ve kaynak kontrolu'
                Write-Host '0  Cikis'
                $choice = Read-Host 'Secim'
                if ($choice -eq '0') { break }
                try {
                    switch ($choice.ToUpperInvariant()) {
                        '1' { Open-Project -Item $projects[0] }
                        '2' { Open-Project -Item $projects[1] }
                        '3' { Open-Project -Item $projects[2]; Open-Project -Item $projects[2] }
                        '4' { Open-Project -Item $projects[3] }
                        '5' { Open-Project -Item $projects[4] }
                        'B' { foreach ($item in $projects) { Build-Project -Item $item } }
                        'T' { foreach ($item in $projects) { Test-Project -Item $item } }
                        'K' { Require-Sdk; Invoke-Dotnet -Arguments @('--info'); Check-Sources -Items $projects }
                        default { Write-Host 'Listeden bir secim girin.' }
                    }
                }
                catch { Write-Host $_.Exception.Message -ForegroundColor Red }
            }
        }
    }
}
catch { Write-Error $_.Exception.Message; exit 1 }
