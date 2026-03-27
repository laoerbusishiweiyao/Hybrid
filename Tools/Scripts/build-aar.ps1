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

# ============================================================
# 🤖 NativeBridge AAR 发布构建脚本
# 功能: 构建Android AAR包 → 验证 → 复制到Unity插件目录 → 输出构建耗时
# ============================================================

# 🔧 配置区域
$ProjectPath = "../../Native/Android"
$AarSource = "$ProjectPath/NativeBridge/build/outputs/aar/NativeBridge-release.aar"
$UnityPluginsPath = "../../Unity/Assets/Plugins/Android"
$AarTarget = "$UnityPluginsPath/NativeBridge.aar"
$GradleTask = ":NativeBridge:assembleRelease"
$RequiredClass = "WebViewComponent"  # 用于验证包内是否包含指定类

<#
.SYNOPSIS
    检查 Android 构建环境
.DESCRIPTION
    验证项目是否包含必要的构建文件 (gradlew.bat) 以及 Java 环境是否配置正确
#>
function Test-AndroidEnvironment {    
    Write-Host ""
    Write-Debug "🔍 检查 Android 构建环境..."
    
    # 检查 gradlew
    $gradlewPath = Join-Path $ProjectPath "gradlew.bat"
    if (-not (Test-Path $gradlewPath)) {
        Write-Error "❌ 未找到 gradlew.bat, 请确认项目路径: $ProjectPath"
        return $false
    }
    
    # 检查 Java 环境
    if (-not (Get-Command java -ErrorAction SilentlyContinue)) {
        Write-Error "❌ 未找到 java 命令, 请安装并配置 JAVA_HOME"
        return $false
    }
    
    # 检查 ANDROID_HOME (可选但推荐)
    if (-not $env:ANDROID_HOME) {
        Write-Warning "⚠️ 未设置 ANDROID_HOME 环境变量，构建可能失败"
    }
    
    Write-Success "✅ 环境检查通过"
    return $true
}

<#
.SYNOPSIS
    执行 Gradle 构建任务
.DESCRIPTION
    使用 gradlew.bat 执行指定的 Gradle 任务
.PARAMETER Task
    要执行的 Gradle 任务名称 (例如: :NativeBridge:assembleRelease)
.PARAMETER WorkingDirectory
    项目根目录 (包含 gradlew.bat)
#>
function Invoke-GradleBuild {
    param(
        [string]$Task,
        [string]$WorkingDirectory
    )
    
    Write-Host ""
    Write-Debug "🚀 开始执行 Gradle 构建: $Task"

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    
    $logFile = [System.IO.Path]::GetTempFileName()
    $spinner = @('⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏')
    $i = 0
    
    try {
        Push-Location $WorkingDirectory
        
        # 设置编码并执行构建
        $process = Start-Process "cmd.exe" `
            -ArgumentList "/c", "chcp 65001 >nul && gradlew.bat $Task --no-daemon" `
            -Wait:$false -NoNewWindow -PassThru `
            -RedirectStandardOutput $logFile

        # 隐藏光标防止闪烁
        [System.Console]::CursorVisible = $false
        while (!$process.HasExited) {
            # `r 是回车符，用于覆盖当前行
            Write-Host -NoNewline "`r$($spinner[$i % 10]) 正在构建中..." 
            Start-Sleep -Milliseconds 100
            $i++
        }

        # 等待进程退出
        $process.WaitForExit()

        $stopwatch.Stop()

        Write-Host "`r" -NoNewline
        
        if ($process.ExitCode -eq 0) {
            Write-Success "✅ Gradle 构建成功 (耗时：$($stopwatch.Elapsed.TotalSeconds.ToString("F2"))秒)"
            return $true
        }
        else {
            Write-Error "❌ Gradle 构建失败 (退出码: $(process.ExitCode))"
            Get-Content $logFile -Encoding UTF8
            return $false
        }
    }
    catch { 
        $stopwatch.Stop()       
        Write-Error "❌ 构建过程异常: $($_.Exception.Message)"
        return $false
    }
    finally {   
        [System.Console]::CursorVisible = $true

        if (Test-Path $logFile) {
            Remove-Item $logFile -Force -ErrorAction SilentlyContinue
        }
         
        Pop-Location
    }
}

<#
.SYNOPSIS
    验证 AAR 包是否包含指定类
.DESCRIPTION
    检查 AAR 包中的 classes.jar 是否包含指定的类名
.PARAMETER AarPath
    AAR 文件路径
.PARAMETER ClassName
    要验证的类名
#>
function Test-AarContainsClass {
    param(
        [string]$AarPath,
        [string]$ClassName
    )

    Write-Host ""

    if (-not (Test-Path $AarPath)) {
        Write-Error "❌ AAR 文件不存在: $AarPath"
        return $false
    }
    
    Write-Debug "🔍 验证 AAR 包内容: 检查类 [$ClassName]..."
    
    try {
        # 创建临时目录解压
        $tempDir = Join-Path $env:TEMP "aar_verify_$(Get-Random)"
        New-Item $tempDir -ItemType Directory -Force | Out-Null
        
        # 解压 classes.jar
        Expand-Archive -Path $AarPath -DestinationPath $tempDir -Force
        $classesJar = Join-Path $tempDir "classes.jar"
        
        if (-not (Test-Path $classesJar)) {
            Write-Error "❌ AAR 中未找到 classes.jar"
            return $false
        }
        
        # 列出jar内容并查找类
        $jarList = jar -tf $classesJar 2>$null
        if ($jarList -match $ClassName) {
            Write-Success "✅ 验证通过: 包内包含类 [$ClassName]"
            return $true
        }
        else {
            Write-Error "❌ 验证失败: 未找到类 [$ClassName]"
            return $false
        }
    }
    catch {
        Write-Error "❌ 验证过程异常: $($_.Exception.Message)"
        return $false
    }
    finally {
        # 清理临时目录
        if (Test-Path $tempDir) {
            Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

<#
.SYNOPSIS
    复制 AAR 到 Unity 插件目录
.DESCRIPTION
    将构建好的 AAR 文件复制到 Unity 项目的 Plugins/Android 目录
.PARAMETER Source
    源 AAR 文件路径
.PARAMETER Target
    目标路径 (Plugins/Android 目录)
#>
function Copy-AarToUnity {
    param(
        [string]$Source,
        [string]$Target
    )
    
    Write-Host ""
    Write-Debug "📋 复制 AAR 到 Unity 插件目录..."
    
    # 确保目标目录存在
    if (-not (Test-Path $Target)) {
        New-Item -ItemType Directory -Path (Split-Path $Target) -Force | Out-Null
    }
    
    try {
        Copy-Item -Path $Source -Destination $Target -Force
        $fileSize = (Get-Item $Target).Length / 1KB
        Write-Success "✅ 复制成功: $(Split-Path $Target -Leaf) ($([math]::Round($fileSize, 2)) KB)"
        return $true
    }
    catch {
        Write-Error "❌ 复制失败: $($_.Exception.Message)"
        return $false
    }
}

# ============================================================
# 🚀 主执行流程
# ============================================================

function Start-AarPublish {    
    Write-Host "🤖 NativeBridge AAR 发布构建启动" -ForegroundColor Cyan

    $globalStopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $buildStartTime = Get-Date
    
    # 1️⃣ 环境检查
    if (-not (Test-AndroidEnvironment)) {
        Write-Error "环境检查未通过，构建终止"
        return
    }
    
    # 2️⃣ 执行 Gradle 构建
    if (-not (Invoke-GradleBuild -Task $GradleTask -WorkingDir $ProjectPath)) {
        Write-Error "构建失败，发布终止"
        return
    }
    
    # 3️⃣ 验证 AAR 内容 (可选)
    if (-not (Test-AarContainsClass -AarPath $AarSource -ClassName $RequiredClass)) {
        Write-Error "❌ 类验证失败"
        return
    }
    
    # 4️⃣ 复制到 Unity 目录
    if (-not (Copy-AarToUnity -Source $AarSource -Target $AarTarget)) {
        Write-Error "🛑 复制失败，发布未完成"
        return
    }
    
    # 5️⃣ 输出汇总信息
    $globalStopwatch.Stop()
    $buildEndTime = Get-Date
    $totalDuration = "{0}秒" -f [math]::Round($globalStopwatch.Elapsed.TotalSeconds, 2)
    
    Write-Host ""
    Write-Host "📅 开始时间: $($buildStartTime.ToString('HH:mm:ss'))" -ForegroundColor White
    Write-Host "📅 结束时间: $($buildEndTime.ToString('HH:mm:ss'))" -ForegroundColor White
    Write-Host "⏱️  总耗时:    $totalDuration" -ForegroundColor White
}

# ============================================================
# 🎯 脚本入口
# ============================================================

# 支持参数: -SkipVerify 跳过类验证
Start-AarPublish @PSBoundParameters