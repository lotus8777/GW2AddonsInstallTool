# 激战2 插件安装与更新工具 (GW2 Addons Install Tool)

![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)
![WPF](https://img.shields.io/badge/UI-WPF-brightgreen.svg)

基于 **.NET 10.0 WPF** 框架重构与优化的《激战2》（Guild Wars 2）插件安装、更新与管理工具。

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
  - `Newtonsoft.Json` (API 数据解析)
  - `System.IO.Compression` (原生 Zip 压缩解压)

---

## 📂 项目结构

```text
GW2-addons-installtool/
├── GW2-addons-installtool.slnx      # .NET 10 解决方案文件
├── src/
│   ├── GW2-addons-installtool/      # WPF 主程序源码 (3.GW2-addons-installtool.exe)
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

## 📄 声明与许可

本工具仅供《激战2》玩家交流与插件便捷管理使用。插件版权归各自原作者所有（ARCDPS, ReShade, Raidcore Nexus 等）。
