@echo off
chcp 65001 >nul
echo ===============================================
echo 仓库管理系统 (DeviceWarehouseSystem) 发布脚本
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

:: 清理之前的发布文件
echo 清理之前的发布文件...
if exist "bin\Release\net10.0\publish" (
    rd /s /q "bin\Release\net10.0\publish"
)

:: 发布项目
echo 发布项目...
dotnet publish -c Release -o "bin\Release\net10.0\publish"
if %errorlevel% neq 0 (
    echo 错误: 项目发布失败
    pause
    exit /b 1
)

echo 项目发布成功

:: 显示发布结果
echo.
echo ===============================================
echo 发布结果
 echo ===============================================
echo 发布目录: %cd%\bin\Release\net10.0\publish
echo.
echo 部署步骤:
echo 1. 将发布目录中的所有文件复制到服务器
 echo 2. 在服务器上安装.NET 10.0 Runtime
 echo 3. 配置appsettings.json中的数据库连接字符串
 echo 4. 运行DeviceWarehouse.API.exe启动服务
 echo 5. 或使用IIS托管应用
 echo.
echo 按任意键打开发布目录...
pause >nul
explorer "bin\Release\net10.0\publish"

pause