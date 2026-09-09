---
date: 2026-09-09
tags:
  - "#计算机"
  - git
  - ssh
status: draft
---

# Git 初始化与 SSH 配置（Windows / Ubuntu）

## 引入

[[Git|Git]] 那篇讲的是 Git 的**用法**——四个区域、分支、合并。这一篇解决的是**把环境配好**：第一次装好电脑后，怎么把本地仓库建起来、怎么配上 SSH 免密，让 push/pull 不再反复输账号密码。

同一个命令在两个系统上**大部分相同**，区别只集中在几处：密钥文件放在哪、ssh-agent 怎么启动、换行符和文件权限。这篇把 Windows 和 Ubuntu 分开展示，避免照着一个系统抄到另一个系统上出问题。

---

## 一、Git 初始化

### `git init` 做了什么

在项目文件夹里执行：

```bash
cd 你的项目文件夹
git init
```

结果：文件夹里多出一个隐藏的 `.git` 目录。**这个目录就是仓库本体**——之前 [[Git|Git]] 里讲的四个对象（blob/tree/commit/tag）、分支指针、config 全都存在这里面。

>[!important] 关键：两个"区域"的分工
>`.git` 目录之外是**工作区**（你平时编辑的文件），`.git` 目录里是**版本库**（历史快照）。
>- 删掉 `.git` → 历史没了，但你的文件还在；
>- 删掉外面的文件 → 可以用 `.git` 里的历史恢复回来。

执行完 `git init`，文件处于**未跟踪（untracked）**状态，需要再走两步：

```bash
git add .              # 把文件加入暂存区
git commit -m "首次提交"
```

### 裸仓库（`--bare`）

```bash
git init --bare 项目名.git
```

>[!help]- 裸仓库是什么？和普通仓库的区别
>普通仓库 = **工作区 + 版本库**（你能在里面写代码）。
>裸仓库 = **只有版本库，没有工作区**。它不能用来写代码，专门放在服务器/内网主机上当"中央仓库"，供大家 `push` 上去、`clone` 下来。GitHub 上的仓库本质就是裸仓库。

### 初始化后必做的两件配置

Git 提交时要把"谁提交的"写进 commit 元信息，所以先配身份：

```bash
# 全局（对这台机器所有仓库生效）
git config --global user.name "你的名字"
git config --global user.email "你的邮箱"

# 查看当前已生效的配置
git config --list
```

>[!info] 三个配置级别，后面的覆盖前面的
>| 级别 | 命令 | 存放位置 |
>|------|------|---------|
>| 系统级 system | `git config --system` | Git 安装目录（所有用户共享） |
>| 全局 global | `git config --global` | `~/.gitconfig` |
>| 项目级 local | `git config`（默认） | 项目里的 `.git/config` |
>
>优先级：**local > global > system**。比如公司项目要单独用工作邮箱，就在那个项目里执行 `git config user.email "工作邮箱"`，只影响这个项目。

---

## 二、SSH 配置（免密连接 GitHub）

### 为什么用 SSH 而不是 HTTPS

| 方式 | 认证方式 | 体验 |
|------|---------|------|
| HTTPS | 用户名 + 密码 / token | 可能反复要求输密码（除非配 credential helper） |
| SSH | 密钥对（公钥 + 私钥） | 配置一次，之后免密 |

### 原理：公钥 + 私钥

>[!help]- 为什么"公钥放服务器"就能免密
>1. 本地生成一对密钥：**私钥**（自己留，绝不外传）和**公钥**（可以公开）。
>2. 把**公钥**贴到 GitHub。
>3. 连接时，GitHub 用你的公钥加密一段信息发给你，只有你的**私钥**能解开并回应。
>4. 回应正确 → 证明"你就是私钥主人" → 放行。
>
>打个比方：公钥是一把**锁**，挂在 GitHub 门上；私钥是开这把锁的**钥匙**，你自己揣兜里。谁有钥匙，锁就为谁开。

### 步骤

```bash
# 1. 生成密钥（推荐 ed25519；老系统兼容才用 rsa）
ssh-keygen -t ed25519 -C "你的邮箱@example.com"
# 一路回车 = 默认路径 + 不设密码
```

>[!info] 生成结果：两个文件
>| 文件 | 是什么 | 怎么处理 |
>|------|--------|---------|
>| `id_ed25519` | **私钥** | 留在本地，绝不发给任何人 |
>| `id_ed25519.pub` | **公钥** | 内容粘贴到 GitHub |

```bash
# 2. 查看公钥内容，复制它
cat ~/.ssh/id_ed25519.pub

# 3. GitHub → Settings → SSH and GPG keys → New SSH key → 粘贴 → 保存

# 4. 测试连接（首次会问是否信任，输入 yes）
ssh -T git@github.com
# 看到 "Hi <用户名>! You've successfully authenticated..." 就成功了
```

>[!warning] 常见报错：`Permission denied (publickey)`
>几乎都是这三个原因之一：
>1. 公钥没贴对——一定是 `.pub` 结尾那个文件，漏字符也会失败。
>2. 私钥没被 ssh-agent 加载（见下节，Windows 尤其容易踩）。
>3. 密钥存到了自定义路径，但 SSH 没找到。

### ssh-agent：让私钥"常驻内存"

如果生成密钥时设了密码，每次 push 都要输一遍；`ssh-agent` 帮你记住，一次加载全程免密。**Windows 上这一步最容易卡住**，见下一节。

```bash
# Ubuntu / Git Bash 通用
eval "$(ssh-agent -s)"      # 启动 agent
ssh-add ~/.ssh/id_ed25519   # 把私钥交给 agent 保管
```

### 多账号场景（可选）

一台机器同时用公司和个人两个 GitHub 账号时，用 `~/.ssh/config` 指定不同私钥：

```
# ~/.ssh/config
Host github-personal
    HostName github.com
    User git
    IdentityFile ~/.ssh/id_ed25519

Host github-work
    HostName github.com
    User git
    IdentityFile ~/.ssh/id_ed25519_work
```

克隆时改用别名：

```bash
git clone git@github-personal:用户名/仓库.git
```

---

## 三、Windows 和 Ubuntu 的区别

命令本身几乎一样，区别在这几处：

### 1. 密钥文件放在哪

| 系统 | `.ssh` 目录位置 |
|------|----------------|
| Windows | `C:\Users\你的用户名\.ssh\` |
| Ubuntu | `~/.ssh/`（= `/home/你的用户名/.ssh/`） |

>[!help]- Windows 里的 `~` 是什么
>在 Git Bash 里，`~` 就是你的用户目录 `C:\Users\你的用户名`，所以 `~/.ssh` 和 `C:\Users\你的用户名\.ssh` 是同一个地方。命令里统一写 `~/.ssh`，两个系统就通用了。

### 2. ssh-agent 启动方式

**Ubuntu**：桌面登录时一般已经自动跑着 agent，`ssh-add` 直接用即可。

**Windows**：OpenSSH 的 agent 服务默认是**停用**的，需要先手动开：

```powershell
# 用管理员身份打开 PowerShell
Get-Service ssh-agent                        # 查看状态
Set-Service ssh-agent -StartupType Automatic # 设为开机自启
Start-Service ssh-agent                      # 现在启动
ssh-add ~/.ssh/id_ed25519                    # 加载私钥
```

>[!warning] Windows 上最常见的坑
>不启动这个服务，`ssh-add` 会报错，之后 push 就一直 `Permission denied (publickey)`。

### 3. 换行符（LF vs CRLF）

两个系统对"换行"的编码不同：

| 系统 | 换行符 | 实际字符 |
|------|--------|---------|
| Windows | CRLF | `\r\n` |
| Ubuntu | LF | `\n` |

多人协作时换行符混着来，`git diff` 会显示"整行都变了"（其实是换行符不同）。用 `core.autocrlf` 让 Git 自动转换：

```bash
# Windows（提交时转成 LF，检出时转回 CRLF）
git config --global core.autocrlf true

# Ubuntu / macOS（提交时转成 LF，检出保持 LF）
git config --global core.autocrlf input
```

### 4. 文件权限

**Ubuntu**：私钥文件权限太开放（比如 644），SSH 会**拒绝使用**，必须收紧：

```bash
chmod 600 ~/.ssh/id_ed25519
chmod 700 ~/.ssh
```

**Windows**：没有 `chmod` 这套权限概念，不用管这一步。

### 总结对照表

| 项目 | Windows | Ubuntu |
|------|---------|--------|
| `.ssh` 位置 | `C:\Users\用户名\.ssh` | `~/.ssh` |
| 生成密钥命令 | `ssh-keygen -t ed25519 -C "邮箱"` | 相同 |
| 复制公钥 | `clip < ~/.ssh/id_ed25519.pub`（Git Bash） | `cat ~/.ssh/id_ed25519.pub` 手动复制 |
| ssh-agent | 手动开服务（见上） | 已自动运行 |
| 换行符 | `core.autocrlf true` | `core.autocrlf input` |
| 私钥权限 | 无需处理 | `chmod 600` |
| 终端 | Git Bash / PowerShell | 自带 bash |

---

## 四、最小可用流程（TL;DR）

新电脑配好 Git + SSH 的完整顺序：

```bash
# 1. 配身份
git config --global user.name "名字"
git config --global user.email "邮箱"

# 2. 生成密钥
ssh-keygen -t ed25519 -C "邮箱"

# 3. 贴公钥到 GitHub（cat 出来复制，Windows 用 clip）

# 4. 测连接
ssh -T git@github.com

# 5. 初始化 / 克隆仓库
git init            # 或 git clone git@github.com:用户名/仓库.git
```

---

## 闪卡

#补充-flashcards

`git init` 之后多出的隐藏目录是==1;;`.git`==，它是仓库本体；目录之外是==1;;工作区==。
<!--SR:!2026-09-10,1,230-->

SSH 免密连接里，贴在 GitHub 上的是==1;;公钥（`.pub`）==，留在本地绝不外传的是==1;;私钥==。
<!--SR:!2026-09-12,3,250-->
