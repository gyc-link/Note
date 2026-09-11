---
date: 2026-06-04
tags:
  - "#计算机"
  - review
status: refined
sr-due: 2026-08-05
sr-interval: 38
sr-ease: 250
---

# Shell 与命令行

>[!question]- 什么是Shell
>- Shell（壳层）是用户与操作系统内核（Kernel）之间的接口程序。 
>- 本质是向操作系统内核解释和执行你的命令
>- shell 壳 -- kernel 核

## Shell 与命令行环境

**为什么会有这么多 Shell？** 

|                | 本质                     | 年代    | 来源              | 运行在哪          |
| -------------- | ---------------------- | ----- | --------------- | ------------- |
| **CMD**        | DOS 时代的命令解释器           | 1980s | 微软              | Windows       |
| **Bash**       | Linux 的默认命令解释器         | 1989  | GNU 项目          | Linux/macOS   |
| **Git Bash**   | Bash 打包移植到 Windows     | ~2005 | Git for Windows | Windows       |
| **PowerShell** | Windows 高级命令环境         | 2006  | 微软              | Windows（现跨平台） |
| **WSL2**       | 真 Linux 内核跑在 Windows 里 | 2019  | 微软              | Windows 轻量虚拟机 |

各自来历：
- **CMD**：DOS 的历史遗产，微软一直没删，但功能极弱。
- **Bash**：`sh`（1977 Bourne shell）太简陋，GNU 在 1989 年重写了一个加强版。随 Linux 普及成为默认 shell。
- **Git Bash**：Git 需要 Linux 环境才能跑，于是把 Bash、grep（搜索）、sed 等打包成 Windows 原生程序塞进 Git 安装包。所以你装 Git 时会顺带得到一个能跑 Linux 命令的终端。
- **PowerShell**：微软 2006 年认识到 CMD 不够用。核心创新——**管道传的是对象而不是文本**。`Get-Process | Where CPU > 100` 不需要做字符串解析，因为管道里流的直接是进程对象，`CPU` 是它的属性。
- **WSL2**：2019 年微软放弃"模拟 Linux"，直接塞了一个真 Linux 内核跑在 Hyper-V 虚拟机里。`wsl --install` 一键装好。

### CMD 常用命令

| 命令 | 作用 |
| --- | --- |
| `mklink /D 链接名 目标路径` | 创建**目录符号链接**（`/D` 表示目录，不加则是文件链接） |
| `mklink /H` | 创建硬链接 |
| `mklink /J` | 创建目录联接（junction） |

> `mklink` 是 CMD 内建命令，PowerShell 里不能直接敲，要写 `cmd /c mklink ...`。

![](../pic/Pasted%20image%2020260613105201.png)

## PowerShell 执行策略

执行优先级
PowerShell 执行策略限制了脚本运行，可通过 `Set-ExecutionPolicy` 调整优先级（RemoteSigned / Unrestricted 等）。

![](../pic/Pasted%20image%2020260329203843.png)

Set-ExecutionPolicy Unrestricted

- **Restricted**：默认策略。禁止运行任何脚本，只能运行单条命令。
- **AllSigned**：只允许运行由受信任发布者签名的脚本。
- **RemoteSigned**：推荐设置。本地脚本可直接运行，互联网下载的脚本必须签名。
- **Unrestricted**：解除所有限制。允许运行任何脚本，但运行网络脚本时会有警告。

**为什么会有这个限制？**

早年 Windows 病毒最常见的传播方式是邮件附件 `发票.pdf.vbs`，双击就中招。后来微软推出 PowerShell，功能远比 `.bat`/`.vbs` 强大（可访问系统底层、注册表、网络），但也意味着恶意脚本能干的事更多。所以微软默认把 PowerShell 脚本执行锁死（Restricted），防止用户或程序不小心运行恶意 `.ps1` 文件。

>ps1文件就是PowerShell的脚本文件。

本质是一个防呆设计——功能越强，越要加锁。

**为什么 `npx playwright install` 会触发？**

1、
npm 调用 Playwright 自带的 `.ps1` 脚本时，这些脚本来自网络且没有微软数字签名 → PowerShell 识别为"来路不明的脚本" → 直接拒绝运行。设为 `RemoteSigned` 后，本地写的脚本随便跑，网络下载的会检查签名（没签名的弹警告但仍可运行）。

2、
你的 `RemoteSigned` 配置本身没有问题，真正的问题是 **ZIP 版 Node 的 `npm.ps1` 被 Windows 标记为了互联网来源（Zone.Identifier）**，PowerShell 因此要求数字签名并拒绝执行。使用 `Unblock-File` 清除标记后即可正常使用。

![](../pic/Pasted%20image%2020260603182444.png)

>[!note] 补充
>这里涉及两个ps1脚本，一个是Node.js的脚本，负责启动npx工具。
>另一个是playwright的脚本（或者其他互联网上的包）npx调用这些包里的.ps1安装脚本。

## WSL

Windows 一键安装 Linux 环境：

```bash
wsl --install    # 管理员 PowerShell 中运行
```

- 启用虚拟机平台 + WSL 功能
- 下载并安装最新 Linux 内核
- 设为 WSL 2 + 安装默认 Ubuntu

Linux 是开源操作系统内核，上面跑着各种**发行版**（Ubuntu、Debian、CentOS 等）。和 Windows 的关键差异：

- **命令行优先**：Linux 的默认交互方式是 shell（如 bash），大部分操作没有 GUI，通过命令完成
- **文件路径**：用正斜杠 `/`（`/home/user/file.txt`），盘符是挂载点（`/mnt/c/` 对应 C 盘）
- **用户权限**：普通用户 + `sudo` 提权，没有 Windows 的管理员/普通用户二元切换
- **包管理器**：软件通过 apt/yum 等包管理器安装和更新，不走 exe 安装包

> WSL 的 Ubuntu 就是一个完整的 Linux 发行版跑在 Windows 里，所以 `wsl` 进终端后走的是 Linux 那套命令体系。

## 包管理与依赖

不同的语言生态有各自的依赖管理方式，但核心目标都是：**项目之间隔离，版本不打架**。

**Node.js (npm)**：依赖下载到项目本地的 `node_modules/`。每个项目自带一份，互不干扰。别人拿到项目后 `npm install` 就能根据 `package.json` 自动补全——所以 `node_modules/` 不用提交到 git。

**Python (conda)**：需要先手动创建虚拟环境（`conda create -n myenv`），环境文件夹统一放在 Miniconda 的大目录里。用 `conda activate` 激活后，项目才"接上"那个环境。好处是一个项目用 Python 3.9 + 旧版 numpy，另一个用 Python 3.12 + 新版——互不影响。

**全局安装**：某些工具需要全局可用（不局限在某个项目里）：
```bash
npm install <包名> -g    # 装到系统 PATH 里，哪个目录都能用
```
例如 `npx`、`playwright` 这类命令行工具常全局装。

**npx — 用完就扔的运行方式**：

npx 解决的是"不想装，但想用"的问题：

1. 去 npm 仓库**临时下载**你要的工具
2. **直接运行**它
3. 运行完**自动删掉**，不占空间、不污染全局

对比：
- **npm** = 安装工具（装到你电脑里，全局或项目依赖）
- **npx** = 运行工具（用完就扔，不安装）

例如 `npx skills add lijigang/ljg-skills -g --all`：临时下载并运行 skill 脚本，把远程仓库里的 skills 批量添加到本地 `.claude/`，执行完脚本不留痕。

## Claude Code

https://code.claude.com/docs/zh-CN/setup

## 串联相关知识点

### shell、终端、CLI、内核

- 你作为**人类**，决定采用 **CLI（命令行模式）** 来操作电脑 。
- 你打开了 **Cmder（终端）**，在漂亮的窗口里用键盘敲下了一行文本命令 。
- 终端把这行文本原封不动地递给了坐在后面的 **PowerShell 7（Shell/命令解释器）**。
- Shell 负责理解这行字，把它翻译成二进制，扔给 **Windows 内核** 去真正控制硬件。

>[!NOTE] c#到机器码
>就跟 Shell 充当文本命令的‘翻译官’一样，C# 到机器码的过程也有一个‘翻译官’。当 C# 语言被编译成包含 CIL（通用中间语言）的托管可执行文件（如 `.exe`）并运行时，操作系统内核会直接启动 .NET 运行时（CLR）。随后，CLR 内部的 **JIT 编译器** 接管程序，将 CIL 逐行/按需翻译成 CPU 认识的机器码并交由硬件执行

## 闪卡

#补充-flashcards 

Shell（壳层）是用户与==1;;操作系统内核（Kernel）==之间的==1;;接口程序==。
操作系统最核心、管理硬件的部分叫==1;;内核（Kernel）;;中英文==。因为普通用户无法直接操作内核，所以需要一个保护并包裹在==1;;内核==外层的“外壳”，这就是 Shell（壳）的由来。
根据用户交互方式的不同，Shell主要分为两类：
- ==1;;CLI;;用户输入文本命令，shell把文本命令翻译成二进制，系统内核根据这个二进制去控制硬件==
- ==1;;GUI;;用户点击鼠标、图标或窗口，图形shell 将图形操作翻译为系统调用，由内核驱动硬件执行==
<!--SR:!2026-09-27,75,270-->

