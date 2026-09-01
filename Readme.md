## DevImgGen (汉化修改版)

Windows Device Image Generator (MiniKit codename) v1.7 — 全界面中文化 + Build页面增强

## 项目简介

对微软内部工具 `devimggen.exe` 和 `diglib.dll` 的逆向工程重实现，用于构建 Windows 10X / FactoryOS / Andromeda 设备镜像。

- 逆向工程: devimggen.exe
- 逆向工程: diglib.dll

## 需要单独准备的内容
- 具备有效设备布局（ARM/ARM64强制要求）与FM、OEMInput的构建文件
- 一个与你所要构建版本的系统相差不远的Imaging Tools Support（ADK自带，x86版本）（为了避免因版本差异而导致构建失败）

## 需要额外说明的内容
- 对于重构的cab可能不一定能够直接使用，可能需要对wcp.dll和TurboStack.dll进行修补以屏蔽对hash和证书的验证
- ARM/ARM64必须保证你的设备布局可用
- 部分ARM64版本在构建时可能会需要很长时间进行构建，请耐心等待
- 此工具不提供通过imageapp和updateapp的修补方式，只做构建帮助
- 使用mobilepackagegen重构的包一定要确定你的cab没有问题而且FMs都是全的

## 本版本修改内容

### 界面汉化
所有用户可见字符串已翻译为中文，覆盖：
- 着陆页（功能选择）
- 驱动导出页
- 创建配置包页
- 构建镜像页
- 驱动重叠对话框
- DigLib 进度日志输出

### Build 页面增强
1. **OS包目录检测** — 检测 `Retail\<架构>\fre` 路径是否存在
2. **OEMInput 用户选择** — 不再基于 ReferenceOEMInput.xml 自动生成，由用户直接选择
3. **imggen/imageapp 用户选择** — 支持选择 `imggen.cmd`（通过cmd.exe /k）或 `imageapp.exe`（直接运行），语法自动区分
4. **架构四选一** — x86 / AMD64 / ARM / ARM64
5. **窗口自动缩放** — Build页面加载时自动调整窗体高度以容纳所有控件，离开时恢复原大小
6. **驱动配置包可选** — 有驱动时自动复制 Volantis 并追加 Driver FM 到 OEMInput 副本
7. **移除自带构建工具** — 项目不再自带 imageapp.exe 和 imggen.cmd，均由用户单独指定路径

### 构建命令语法
- **imggen.cmd**: `imggen.cmd "<output>" "<OEMInput>" "<pkgDir>" <小写架构>`
- **imageapp.exe**: `imageapp.exe "<output>" "<OEMInput>" "<pkgDir>" +StrictSettingPolicies /CPUType:<架构>`

## 工作流程

1. **导出驱动** — 从系统 Driver Store 导出已安装驱动（可选ZIP打包）
2. **创建配置包** — 处理驱动，生成 CBS CAB + FM + XRO映射（需要 signtool/infverif/apivalidator/inf2cat）
3. **构建镜像** — 选择 OS包目录、OEMInput、imggen/imageapp、架构、输出路径，启动构建

## 前置要求

- .NET Framework 4.8
- 驱动配置包创建需要: signtool.exe, infverif.exe, apivalidator.exe, inf2cat.exe（WDK工具，需在PATH中）
- 构建镜像需要: 用户自己的 imggen.cmd 或 imageapp.exe（ADK工具）

## 项目结构

```
DevImgGen.sln
├── DevImgGen/          # 主GUI程序 (WinForms, .NET 4.8)
│   ├── Pages/          # 页面（Landing/Export/CreateConfig/Build）
│   ├── Dialogs/        # 对话框（驱动重叠）
│   ├── Controls/       # 自定义控件
│   ├── lib/            # 引用的DLL
│   ├── universalDDIs/  # 通用DDI白名单
│   ├── Volantis/       # 设备平台FM模板
│   ├── certificates/   # OEM测试证书
│   └── 资源文件         # ReferenceOEMInput.xml, DriverBridgeConfig.xml, WindowsProtectedFiles.xml
├── DigLib/             # 核心驱动处理库
├── AppXCommon/         # AppX包COM互操作（参考实现）
├── AppXImaging/        # AppX镜像（参考实现）
├── CabApiWrapper/      # CAB API封装（参考实现）
├── imageapp/           # imageapp重实现（参考实现）
├── Inf2Cat/            # INF转CAT（参考实现）
└── ApiValidator/       # API验证器（参考实现）
```


## 参考

- [Albacore](https://github.com/thebookisclosed/)
- [UUPMediaCreator fork](https://github.com/thebookisclosed/UUPMediaCreator)

## 声明

按原样提供。仅供研究用途。

-- 原作者 2023，汉化修改 2026
