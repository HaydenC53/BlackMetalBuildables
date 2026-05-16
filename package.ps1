param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^v?\d+\.\d+\.\d+$')]
    [string] $Version
)

$ErrorActionPreference = 'Stop'

$packageVersion = $Version.TrimStart('v')
$versionTag = "v$packageVersion"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $repoRoot 'src\BlackMetalBuildables.csproj'
$distPath = Join-Path $repoRoot 'dist'
$stagePath = Join-Path $distPath "BlackMetalBuildables-$packageVersion"
$zipPath = Join-Path $distPath "BlackMetalBuildables-$packageVersion.zip"
$dllPath = Join-Path $repoRoot 'src\bin\Debug\net472\BlackMetalBuildables.dll'

function Assert-FileExists {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required file not found: $Path"
    }
}

function Assert-IconIsValid {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    Add-Type -AssemblyName System.Drawing

    $image = [System.Drawing.Image]::FromFile($Path)
    try {
        if ($image.Width -ne 256 -or $image.Height -ne 256) {
            throw "icon.png must be exactly 256x256. Found $($image.Width)x$($image.Height)."
        }
    }
    finally {
        $image.Dispose()
    }
}

function Set-ManifestVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Version
    )

    $source = Get-Content -LiteralPath $Path -Raw
    $pattern = '"version_number"\s*:\s*"(?<version>\d+\.\d+\.\d+)"'
    $match = [regex]::Match($source, $pattern)

    if (-not $match.Success) {
        throw "Could not find version_number in $Path."
    }

    $updatedSource = [regex]::Replace(
        $source,
        $pattern,
        "`"version_number`": `"$Version`"",
        1
    )

    Set-Content -LiteralPath $Path -Value $updatedSource -Encoding UTF8 -NoNewline
}

function Set-PluginVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Version
    )

    $source = Get-Content -LiteralPath $Path -Raw
    $pattern = 'PluginVersion\s*=\s*"(?<version>\d+\.\d+\.\d+)"'
    $match = [regex]::Match($source, $pattern)

    if (-not $match.Success) {
        throw "Could not find PluginVersion in $Path."
    }

    $updatedSource = [regex]::Replace(
        $source,
        $pattern,
        "PluginVersion = `"$Version`"",
        1
    )

    Set-Content -LiteralPath $Path -Value $updatedSource -Encoding UTF8 -NoNewline
}

Assert-FileExists -Path $projectPath
Assert-FileExists -Path (Join-Path $repoRoot 'manifest.json')
Assert-FileExists -Path (Join-Path $repoRoot 'README.md')
Assert-FileExists -Path (Join-Path $repoRoot 'CHANGELOG.md')
Assert-FileExists -Path (Join-Path $repoRoot 'icon.png')

Set-ManifestVersion -Path (Join-Path $repoRoot 'manifest.json') -Version $packageVersion
Set-PluginVersion -Path (Join-Path $repoRoot 'src\Plugin.cs') -Version $packageVersion
Assert-IconIsValid -Path (Join-Path $repoRoot 'icon.png')

Write-Host "Building BlackMetalBuildables $versionTag..."
dotnet build $projectPath

Assert-FileExists -Path $dllPath

if (-not (Test-Path -LiteralPath $distPath -PathType Container)) {
    New-Item -ItemType Directory -Path $distPath | Out-Null
}

if (Test-Path -LiteralPath $stagePath) {
    Remove-Item -LiteralPath $stagePath -Recurse -Force
}

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

$pluginsPath = Join-Path $stagePath 'plugins'
New-Item -ItemType Directory -Path $pluginsPath -Force | Out-Null

Copy-Item -LiteralPath (Join-Path $repoRoot 'manifest.json') -Destination $stagePath
Copy-Item -LiteralPath (Join-Path $repoRoot 'README.md') -Destination $stagePath
Copy-Item -LiteralPath (Join-Path $repoRoot 'CHANGELOG.md') -Destination $stagePath
Copy-Item -LiteralPath (Join-Path $repoRoot 'icon.png') -Destination $stagePath
Copy-Item -LiteralPath $dllPath -Destination $pluginsPath

Write-Host "Creating package zip..."
Compress-Archive -Path (Join-Path $stagePath '*') -DestinationPath $zipPath

Write-Host "Package contents:"
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $zip.Entries | ForEach-Object {
        Write-Host "  $($_.FullName)"
    }
}
finally {
    $zip.Dispose()
}

Write-Host ""
Write-Host "Package created:"
Write-Host "  $zipPath"
