@echo off
chcp 65001 >nul
echo ===============================================
echo 仓库管理系统 (DeviceWarehouseSystem) 部署脚本
echo ===============================================
echo.

:: 检查.NET SDK是否安装
echo 检查.NET SDK环境...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo 错误: 未检测到.NET SDK，请先安装.NET 10.0 SDK
    echo 下载地址: https://dotnet.microsoft.com/download/dotnet/10.0
    pause
    exit /b 1
)

echo .NET SDK 已安装

:: 检查项目目录
if not exist "DeviceWarehouseSystem\DeviceWarehouse.API" (
    echo 错误: 未找到项目目录，请确保脚本在正确的位置执行
    pause
    exit /b 1
)

:: 进入API项目目录
echo 进入项目目录...
cd "DeviceWarehouseSystem\DeviceWarehouse.API"

:: 安装依赖
echo 安装项目依赖...
dotnet restore
if %errorlevel% neq 0 (
    echo 错误: 依赖安装失败
    pause
    exit /b 1
)

echo 依赖安装成功

:: 构建项目
echo 构建项目...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo 错误: 项目构建失败
    pause
    exit /b 1
)

echo 项目构建成功

:: 提示数据库配置
echo.
echo ===============================================
echo 数据库配置提示
echo ===============================================
echo 1. 请确保SQL Server服务正在运行
 echo 2. 数据库连接字符串已在appsettings.json中配置
 echo 3. 首次启动时会自动创建数据库并初始化数据
 echo.
 echo 按任意键启动服务...
pause >nul

:: 启动服务
echo 启动服务...
dotnet run --launch-profile "https"

:: 如果服务意外退出
echo 服务已停止
pause