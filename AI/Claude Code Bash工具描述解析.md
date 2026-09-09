# Bash 工具完整解析

> 解析自：`Claude Code` 的 Bash 工具 JSON 定义
> 原文链接：[[Claude code能力边界#Bash工具的具体描述|Claude Code 能力边界 - Bash 工具]]

## 基本信息

- **工具名**：`Bash`
- **底层引擎**：标准 Bash Shell
- **状态保持**：工作目录在命令间保持，但 Shell 状态（环境变量、alias 等）不保持，每次命令从用户 profile（bash/zsh）重新初始化

## 核心原则：能用专用工具就别用 Bash

| 任务 | 用专用工具 | 别用 Bash |
|---|---|---|
| 文件搜索 | `Glob` | `find`、`ls` |
| 内容搜索 | `Grep` | `grep`、`rg` |
| 读文件 | `Read` | `cat`、`head`、`tail` |
| 编辑文件 | `Edit` | `sed`、`awk` |
| 写文件 | `Write` | `echo >`、`cat <<EOF` |
| 输出信息 | 直接输出文本 | `echo`、`printf` |

## 参数列表

| 参数 | 类型 | 必填 | 说明 |
|---|---|---|---|
| `command` | string | 是 | 要执行的命令 |
| `timeout` | number | 否 | 超时（毫秒），默认 120s，最大 600s（10分钟） |
| `description` | string | 否 | 命令用途的清晰描述（主动语态，不写"complex""risk"等词） |
| `run_in_background` | bool | 否 | 设为 true 后台运行，不用等结果 |
| `dangerouslyDisableSandbox` | bool | 否 | **危险**：绕过沙箱运行命令 |

### description 参数示例

| 命令 | 描述 |
|---|---|
| `ls` | "List files in current directory" |
| `git status` | "Show working tree status" |
| `npm install` | "Install package dependencies" |
| `find . -name "*.tmp" -exec rm {} \\;` | "Find and delete all .tmp files recursively" |
| `git reset --hard origin/main` | "Discard all local changes and match remote main" |

## 使用规则

### 路径与目录

- 始终使用**绝对路径**，避免 `cd`
- 包含空格的文件路径用**双引号**包裹
- 不要用 `cd <dir>` + `git` 命令的组合，会触发额外权限弹窗

### 多命令执行

| 场景 | 方式 |
|---|---|
| 独立命令，可并行 | 一次发多个 Bash 调用 |
| 依赖顺序，前失败后不执行 | `&&` 串联 |
| 依赖顺序，前失败仍继续 | `;` 串联 |
| **禁止** | 用换行符分隔命令 |

### 超时与后台

- 长任务用 `run_in_background`，完成时会通知你
- 不要用 `sleep` 轮询后台任务——完成时会自动通知
- 必须轮询外部进程时，用检查命令（如 `gh run view`）替代 `sleep`

## Git 安全协议

> Bash 工具中对 Git 的约束最严格。

- 永远不更新 git config
- 以下命令**永远禁止**（除非用户明确要求）：
  - `push --force`、`reset --hard`、`checkout .`、`restore .`、`clean -f`、`branch -D`
- 永远不跳过 hooks：`--no-verify`、`--no-gpg-sign`
- 永远不 force push 到 main/master
- 永远创建**新 commit**，不 amend 已有 commit
- `git add` 只加指定文件，不用 `-A` 或 `.`
- 不用 `-i` 交互式 flag（`rebase -i`、`add -i`）
- 不用 `--no-edit`（对 rebase 无效）
- 空 commit 不创建

### 提交流程

1. 并行运行：`git status`（不用 `-uall`）+ `git diff` + `git log`
2. 分析变更，撰写 commit message（1-2 句，focus on "why"）
3. `git add <specific-files>` → `git commit`（用 HEREDOC 传 message）
4. 检查 hooks 是否失败，失败则修复后创建新 commit

<details>
<summary>Commit message 示例</summary>

```bash
git commit -m "$(cat <<'EOF'
Fix buffer overflow in image parser
EOF
)"
```
</details>

## PR 创建流程

1. 并行运行：`git status` + `git diff` + 检查远程分支 + `git log` + `git diff [base]...HEAD`
2. 分析**所有** commit（不是只看最新一个），撰写 PR 标题和描述
3. `git push -u` → `gh pr create`（用 HEREDOC 传 body）

<details>
<summary>PR 创建示例</summary>

```bash
gh pr create --title "the pr title" --body "$(cat <<'EOF'
## Summary
<1-3 bullet points>

## Test plan
[Bulleted markdown checklist of TODOs for testing the pull request...]
EOF
)"
```
</details>

> 注意：PR 标题控制在 **70 字符内**，详情写在 body 里。

## 其他操作

```bash
# 查看 GitHub PR 评论
gh api repos/foo/bar/pulls/123/comments
```

## 沙箱相关

`dangerouslyDisableSandbox` 设为 `true` 会绕过操作系统级沙箱隔离（文件系统 + 网络），高风险操作会弹窗确认。
