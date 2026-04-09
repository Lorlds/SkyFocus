# SkyFocus

SkyFocus 是一个基于 WinUI 3 和 Windows App SDK 的 Windows 原生专注应用。当前版本采用原创的航空主题设计方向，包含专注计时、环境音、本地统计、软阻断和航线进度反馈等能力。

## 当前进度

仓库内已经有一套可运行的 v1 骨架：
- WinUI 3 打包桌面应用
- `Home / Focus / History / Sounds / Settings` 五个页面
- SQLite 本地持久化
- 专注流程：创建、开始、暂停、恢复、完成、放弃
- 软阻断与提醒服务
- 环境音播放与占位音频资源
- 覆盖核心流程的单元测试

本地已完成验证：
- `dotnet build SkyFocus.csproj -c Debug -p:Platform=x64`
- `dotnet test SkyFocus.Tests\\SkyFocus.Tests.csproj -c Debug -p:Platform=x64`
- 已验证可执行文件能成功启动

## 技术栈

- C#
- .NET 8
- WinUI 3
- Windows App SDK
- CommunityToolkit.Mvvm
- Microsoft.Data.Sqlite
- MSTest

## 仓库结构

- `SkyFocus.csproj`：主应用工程
- `SkyFocus.Tests/`：测试工程
- `Models/`：数据模型与状态对象
- `Services/`：专注引擎、持久化、音频、提醒、设置、导航、统计
- `ViewModels/`：页面状态与壳层状态
- `Views/`：XAML 页面
- `Resources/`、`Strings/`、`Assets/`：主题、文案、本地资源

## 构建与测试

在仓库根目录执行：

```powershell
dotnet build SkyFocus.csproj -c Debug -p:Platform=x64
dotnet test SkyFocus.Tests\SkyFocus.Tests.csproj -c Debug -p:Platform=x64
```

直接运行构建产物：

```powershell
.\bin\x64\Debug\net8.0-windows10.0.26100.0\win-x64\SkyFocus.exe
```

## 下一步建议

- 打磨视觉系统和动效
- 用正式环境音替换占位音频
- 强化历史统计和成就表现
- 验证 MSIX 打包和干净机器安装流程
- 准备上架所需文案、图标和截图

## 备注

- 当前仓库根目录就是应用工程根目录。
- `SkyFocus.Tests/` 已在 `SkyFocus.csproj` 中显式排除，不会被主项目误编译。
- `Assets/Audio/` 中的音频目前是占位资源，不是最终品牌素材。
