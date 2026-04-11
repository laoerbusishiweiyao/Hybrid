# 设置控制台代码页为 UTF-8
chcp 65001 | Out-Null

# 设置 PowerShell 输入输出编码
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::InputEncoding = [System.Text.Encoding]::UTF8

# 确保输出流使用 UTF-8
$OutputEncoding = [System.Text.Encoding]::UTF8

# 🎨 启用虚拟终端处理
$Host.UI.RawUI.WindowTitle = $Host.UI.RawUI.WindowTitle  # 触发刷新

$WorkingDirectory = Split-Path -Parent $Script:MyInvocation.MyCommand.Path
Write-Host "工作目录：$WorkingDirectory" -ForegroundColor Gray

Import-Module (Join-Path $WorkingDirectory "Modules/Logger.psm1") -Force

$Configuration = "Release"
$TargetFramework = "net10.0"

$ProjectPath = "../../Server/SourceGenerator.Unity"
$UnityPlugins = "../../Unity/Assets/Plugins"

function Test-DotNetEnvironment {
    param([string]$RequiredFramework = "net10.0")
    
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Warning "❌ 未找到 dotnet 命令，请先安装 .NET SDK"
        Write-Information "🔗 https://dotnet.microsoft.com/download"
        return $false
    }
    
    $requiredMajorVersion = $RequiredFramework -replace 'net', '' -split '\.' | Select-Object -First 1
    $installedSdks = dotnet --list-sdks 2>$null | ForEach-Object {
        if ($_ -match '^(\d+)\.') { [int]$matches[1] }
    } | Sort-Object -Unique
    
    if ($installedSdks -notcontains $requiredMajorVersion) {
        Write-Warning "❌ 缺少 .NET SDK $requiredMajorVersion.x"
        Write-Information "🔗 https://dotnet.microsoft.com/download/dotnet/$requiredMajorVersion"
        return $false
    }
    
    Write-Information "✅ .NET 环境检查通过 (SDK: $(dotnet --version))"
    return $true
}

if (-not (Test-DotNetEnvironment -RequiredFramework $TargetFramework)) {
    Write-Warning "❌ 环境检查未通过，请安装缺失的组件后重试"
    exit 1
}

Write-Information "📦 正在构建 SourceGenerator.Unity..."

Push-Location -Path $ProjectPath
dotnet restore
dotnet publish -c $Configuration
Pop-Location

Write-Information "✅ SourceGenerator.Unity 构建完成。"

Copy-Item -Path $ProjectPath/bin/$Configuration/publish/SourceGenerator.Unity.dll -Destination $UnityPlugins/SourceGenerator.Unity.dll -Force