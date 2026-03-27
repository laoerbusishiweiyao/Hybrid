$MinVersion = "24"
$VueProject = "../../Web/unity-web-ui"
$OutputDir = "../../Release/EdgeServer/vue"

# ============================================================
# 🔧 工具函数：检测 Node.js 环境
# ============================================================
function Test-NodeEnvironment {    
    if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
        Write-Host "❌ 未找到 node 命令，请先安装 Node.js" -ForegroundColor Red
        Write-Host "🔗 https://nodejs.org/" -ForegroundColor Yellow
        return $false
    }
    
    if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
        Write-Host "❌ 未找到 npm 命令" -ForegroundColor Red
        return $false
    }
    
    $nodeVersion = node --version 2>$null
    if ($nodeVersion -match '^v(\d+)\.') {
        $majorVersion = [int]$matches[1]
        if ($majorVersion -lt [int]$MinVersion) {
            Write-Host "❌ Node.js 版本过低 (当前: $nodeVersion, 需要: >= $MinVersion)" -ForegroundColor Red
            return $false
        }
        Write-Host "✅ Node.js 环境检查通过 ($nodeVersion)" -ForegroundColor Green
        return $true
    }
    
    Write-Host "⚠️  无法解析 Node.js 版本: $nodeVersion" -ForegroundColor Yellow
    return $true  # 继续尝试
}

if (-not (Test-NodeEnvironment -RequiredFramework $TargetFramework)) {
    Write-Host "❌ 环境检查未通过，请安装缺失的组件后重试" -ForegroundColor Red
    exit 1
}

Write-Host "📦 正在构建 Vue 项目..." -ForegroundColor Cyan
Write-Host "🎯 项目目录: $VueProject" -ForegroundColor Gray
Write-Host "📤 输出目录: $OutputDir" -ForegroundColor Gray

try {
    Push-Location $VueProject

    if (-not (Test-Path "package.json")) {
        throw "未找到 package.json，确认这是 Vue 项目根目录"
    }

    if (-not (Test-Path "node_modules")) {
        Write-Host "📥 安装 npm 依赖..." -ForegroundColor Gray
        npm install --loglevel=error
        if ($LASTEXITCODE -ne 0) { 
            throw "npm install 失败" 
        }
    }

    Write-Host "🔨 编译 Vue..." -ForegroundColor Gray
    npm run build
    if ($LASTEXITCODE -ne 0) { throw "npm run build 失败" }

    $VueDist = Join-Path $VueProject "dist"
    if (-not (Test-Path $VueDist)) {
        throw "未找到 Vue 构建输出目录：$VueDist"
    }

    Write-Host "📋 复制前端文件到: $OutputDir" -ForegroundColor Gray
    if (Test-Path $OutputDir) {
        Remove-Item $OutputDir -Recurse -Force
    }
    Copy-Item -Path $VueDist -Destination $OutputDir -Recurse -Force
        
    Write-Host "✅ Vue 项目构建完成" -ForegroundColor Green
}
catch {
    Write-Host "❌ Vue 构建失败: $_" -ForegroundColor Red
    throw
}
finally {
    Pop-Location
}