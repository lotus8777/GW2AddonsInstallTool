# 激战2 插件安装与更新工具 (GW2 Addons Install Tool)

![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)
![WPF](https://img.shields.io/badge/UI-WPF-brightgreen.svg)

基于 **.NET 10.0 WPF** 框架重构与优化的《激战2》（Guild Wars 2）插件安装、更新与管理工具。

---

## 📖 使用教程与注意事项

### 💡 安装与更新教程

1. **下载与解压**：下载最新版插件安装工具，请解压到除游戏根目录以外的**任意位置文件夹**内。
2. **启动工具**：右键以**管理员模式**打开 `GW2-addons-installtool.exe`。
3. **设置路径与模式**：
   - 点击“选择激战2游戏目录”，选择您的游戏根目录。
   - 推荐选择 **Nexus 模式**。
4. **选择插件**：勾选右侧需要安装的插件（不了解功能的插件请勿盲目勾选）。
5. **一键安装/更新**：点击“安装/更新插件”按钮，等待进度完成即可。

---

### ⚠️ 注意事项与快捷键说明

- **游戏内更新提示**：游戏内弹出的插件更新提示按钮请直接无视，切勿在游戏内点击更新，更新请统一使用本工具。
- **Nexus 模式优势**：支持热加载、热卸载与热更新！安装后游戏内内置 Nexus 插件库（快捷键：`Ctrl + O`），提供丰富的在线插件资源。
- **第三方软件兼容提醒**：
  - 若安装自定义增益 UI 插件，请禁用或取消 GeForce Experience / AMD 显卡驱动的“游戏内覆盖”选项。
  - 建议禁用或退出 RTSS (RivaTuner Statistics Server) 帧率监控软件。

---

## ✨ 主要特性

- **🚀 现代化 .NET 10 架构**：采用最新的 .NET 10.0-windows WPF 技术栈，全面提升运行效率与响应速度。
- **🎮 多模式插件支持**：
  - **Nexus 模式**：强烈推荐，支持游戏内插件热加载、热卸载与热更新（默认快捷键 `Ctrl + O`）。
  - **正常模式 / 疑难模式**：兼容传统插件放置逻辑与历史插件组合。
- **📦 丰富插件库整合**：自动同步与一键安装/更新 ARCDPS、ReShade 滤镜、SCT 流动输出、团队机制、团队增益、治疗统计、坐骑工具等。
- **🔍 智能检测与自动修复**：
  - 自动检测并校验 Visual C++ 2015-2022 Redistributable (x64) 运行库。
  - 自动校验 MD5 与文件完整性，避免损坏或版本冲突。
  - 原生 `Microsoft.Win32.OpenFolderDialog` 智能路径识别。

---

## 🛠️ 技术栈

- **框架**：.NET 10.0 (WPF)
- **语言**：C# 13
- **依赖库**：
  - `System.Text.Json` (内置 JSON 反序列化与 API 解析)
  - `System.IO.Compression` (原生 Zip 压缩解压)

---

## 📂 项目结构

```text
GW2AddonsInstallTool/
├── GW2-addons-installtool.slnx      # .NET 10 解决方案文件
├── src/
│   ├── GW2-addons-installtool/      # WPF 主程序源码 (GW2-addons-installtool.exe)
│   └── Updater/                     # 独立自动更新程序源码 (Updater.exe)
├── README.md                        # 项目说明文档
└── .gitignore                       # Git 忽略配置
```

---

## 🔨 编译与构建

### 前置要求
- Windows 10 / 11 操作系统
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) 或更高版本

### 构建步骤

```bash
# 1. 克隆代码仓库
git clone https://github.com/lotus8777/GW2AddonsInstallTool.git
cd GW2AddonsInstallTool

# 2. 编译解决方案
dotnet build GW2-addons-installtool.slnx -c Release

# 3. 发布独立执行文件
dotnet publish GW2-addons-installtool.slnx -c Release
```

发布生成的目标可执行程序保存在：`src/GW2-addons-installtool/bin/Release/net10.0-windows/publish/`

---

## 📝 历史更新日志

<details>
<summary>展开查看历史更新日志</summary>

- **.NET 10 重构版**：重写为 .NET 10 WPF 架构，优化 VC++ 2015-2022 检测算法，移除 Newtonsoft.Json 改用原生 System.Text.Json。
- **3.2.3**：新增 DPS 插件推荐版本一键安装，Nexus 模式支持滤镜模式一和模式二。
- **3.2.2**：DPS 版本增加显示常规版或先行版提示。
- **3.2.1**：检测游戏是否运行移至安装逻辑中，方便游戏中打开安装工具查看插件快捷键描述。
- **3.2.0**：更新 UI；新增安装工具自我更新功能；支持 Nexus 模式。
- **3.1.9**：重新修改 DPS 列表显示；设置内增加疑难模式1和模式2。
- **3.1.8**：修改界面显示；修改正常模式仅安装 ReShade 时的安装逻辑；修复 .NET 版无法打开教程按钮问题。
- **3.1.7**：不再支持 Gshade 安装；修改 DPS 插件历史版本显示数量。
- **3.1.6**：移除国服美服模式；启用 24 小时自动检测 DPS 版本并打包上传服务器。
- **3.1.5**：添加自选 DPS 插件版本功能。
- **3.1.4**：更换过时的 WebClient 为 HttpClient。
- **3.1.3**：增加服务器端已上传 DPS 最新版本是否可用显示；设置里可恢复 DPS 默认标题栏。
- **3.1.2**：增加国服更新时间和美服更新时间切换模式。
- **3.1.1**：修复安装神油工具插件独立版时的错误弹框；游戏运行中打开工具改为弹框提示是否关闭游戏。
- **3.1.0**：修复未选择 arcdps 插件正常安装时调用文件未复制问题。
- **3.0.9**：添加尝试启动程序时强制结束激战2；主界面添加单次有效安装上一个版本 arcdps 选项；更新 VC++ 包。
- **3.0.8**：根据游戏版本更改插件加载核心 `d3d9.dll` 和 `dxgi.dll` 放置位置。
- **3.0.7**：添加版本更新提醒；使神油工具(独立版)、自定义增益 UI 安装逻辑正常化。
- **3.0.6**：检测到激战2运行提示结束进程；更新/安装保留默认配置。
- **3.0.5**：添加神油工具集独立版（快捷键 `Shift+Alt+Y`）；添加自定义增益 UI 插件（快捷键 `Shift+Alt+P`）。

</details>

---

## 📄 声明与许可

本工具仅供《激战2》玩家交流与插件便捷管理使用。插件版权归各自原作者所有（ARCDPS, ReShade, Raidcore Nexus 等）。
