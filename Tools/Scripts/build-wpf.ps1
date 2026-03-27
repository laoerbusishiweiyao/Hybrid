$Configuration = "Release"
$TargetFramework = "net10.0"

$WpfOutput = "../../../../$Configuration/Windows"

# ============================================================
# 🔧 工具函数：检测 .NET 环境
# ============================================================
function Test-DotNetEnvironment {
    param([string]$RequiredFramework = "net10.0")
    
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Host "❌ 未找到 dotnet 命令，请先安装 .NET SDK" -ForegroundColor Red
        Write-Host "🔗 https://dotnet.microsoft.com/download" -ForegroundColor Yellow
        return $false
    }
    
    $requiredMajorVersion = $RequiredFramework -replace 'net', '' -split '\.' | Select-Object -First 1
    $installedSdks = dotnet --list-sdks 2>$null | ForEach-Object {
        if ($_ -match '^(\d+)\.') { [int]$matches[1] }
    } | Sort-Object -Unique
    
    if ($installedSdks -notcontains $requiredMajorVersion) {
        Write-Host "❌ 缺少 .NET SDK $requiredMajorVersion.x" -ForegroundColor Red
        Write-Host "🔗 https://dotnet.microsoft.com/download/dotnet/$requiredMajorVersion" -ForegroundColor Yellow
        return $false
    }
    
    Write-Host "✅ .NET 环境检查通过 (SDK: $(dotnet --version))" -ForegroundColor Green
    return $true
}

if (-not (Test-DotNetEnvironment -RequiredFramework $TargetFramework)) {
    Write-Host "❌ 环境检查未通过，请安装缺失的组件后重试" -ForegroundColor Red
    exit 1
}

if (Test-Path $WpfOutput) {
    Write-Host "🧹 正在清空目录..." -ForegroundColor Cyan
    Remove-Item $WpfOutput -Recurse -Force
    Write-Host "✅ 目录已清空。" -ForegroundColor Green
}

Write-Host "📦 正在构建 Wpf..." -ForegroundColor Cyan

Push-Location -Path "../../Native/Windows/Hybrid.Windows/Hybrid.Native.Windows"
dotnet restore
dotnet publish -c $Configuration -o $WpfOutput
Pop-Location

Write-Host "✅ Wpf 构建完成。" -ForegroundColor Green