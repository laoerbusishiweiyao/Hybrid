# ============================================================
# 🧩 Logger.psm1 - 通用日志输出模块
# 功能: 统一日志格式、颜色、时间戳、级别控制
# 使用: Import-Module "./Logger.psm1"
# ============================================================

$LoggerConfig = @{
    TimeFormat  = "HH:mm:ss"
    ColorScheme = @{
        Info    = "Cyan"
        Success = "Green"
        Warning = "Yellow"
        Error   = "Red"
        Debug   = "Gray"
        Dim     = "DarkGray"
    }
    LogLevel    = "Debug"
}

$LevelPriority = @{
    Debug   = 1
    Info    = 2
    Warning = 3
    Error   = 4
}

# ============================================================
# 🔧 核心函数
# ============================================================

<#
.SYNOPSIS
    统一日志输出函数
.PARAMETER Message
    日志内容
.PARAMETER Level
    日志级别: Debug | Info | Warning | Error | Success
.PARAMETER Color
    强制指定前景色 (覆盖默认方案)
#>
function Write-Log {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $true)]
        [string]$Message,
        
        [ValidateSet("Debug", "Info", "Warning", "Error", "Success")]
        [string]$Level = "Info"
    )
    
    # 级别过滤
    $currentPriority = $LevelPriority[$LoggerConfig.LogLevel]
    $msgPriority = $LevelPriority[$Level]
    if ($msgPriority -lt $currentPriority -and $Level -ne "Success") {
        return  # 低于配置级别且非成功消息则静默
    }
    
    # 构建前缀
    $prefix = "[$(Get-Date -Format $LoggerConfig.TimeFormat)] "
    
    # 输出
    Write-Host $prefix -NoNewline -ForegroundColor $LoggerConfig.ColorScheme[$Level]
    Write-Host $Message -ForegroundColor $LoggerConfig.ColorScheme[$Level]
}

# ============================================================
# 🎯 便捷包装函数
# ============================================================

function Write-Info { param([string]$Msg) Write-Log -Message $Msg -Level Info    @args }
function Write-Success { param([string]$Msg) Write-Log -Message $Msg -Level Success @args }
function Write-Warning { param([string]$Msg) Write-Log -Message $Msg -Level Warning @args }
function Write-Error { param([string]$Msg) Write-Log -Message $Msg -Level Error   @args }
function Write-Debug { param([string]$Msg) Write-Log -Message $Msg -Level Debug   @args }

# 🔧 导出函数
Export-ModuleMember -Function Write-Debug, Write-Info, Write-Success, Write-Warning, Write-Error

# 模块导入时自动执行
$script:Initializer = {} & $script:Initializer