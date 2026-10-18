# 模块隔离检查

Windows、.NET 10，使用本机已安装的 Dalamud CN API 15 和 Omni 依赖 DLL：

```powershell
dotnet run --project Tests/BetterGlamourManagement/Checks.csproj -c Release
```

默认查找 APPDATA 下 Hooks/26-10-17-01 和 installedPlugins/OmniToolbox/1.1.7.2；可用 `-p:DalamudLibPath=路径` 及 `-p:OmniLibPath=路径` 覆盖。不需要个人方案或登录数据。

检查包含剪贴板格式、校验、方案版本、原生结构、预览生命周期模拟及 CPU 像素/JPEG 处理。此项目编译本模块源码，借用宿主程序集依赖；它不替代完整宿主构建，也不能验证实时 GPU／游戏 UI 行为。完整构建仍需根项目引用的 OmniToolbox.Common、KamiToolKit、OmenTools 相邻仓库。
