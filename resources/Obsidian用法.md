---
date: 2026-06-04
tags:
  - reference
  - obsidian
status: evergreen
---

# Obsidian 提示框 (Callout) 语法速查

> Obsidian 内置的提示框语法，用 `> [!type]` 开头，支持折叠、标题自定义、嵌套。

## 基本语法

```
> [!类型] 可选标题
> 内容行 1
> 内容行 2
```

**折叠版本：** 类型后面加 `-`（默认展开）或 `+`（默认折叠）

```
> [!类型]- 标题（默认展开，点一下收起）
> 内容

> [!类型]+ 标题（默认折叠，点一下展开）
> 内容
```

## 全部 15 种类型

### 信息类

> [!note]
> `[!note]` — 笔记、备注

> [!info]
> `[!info]` — 补充信息

> [!tip]
> `[!tip]` — 技巧、提示

> [!hint]
> `[!hint]` — 同 tip，别名

> [!important]
> `[!important]` — 重要信息

> [!abstract]
> `[!abstract]` — 摘要、TL;DR

> [!summary]
> `[!summary]` — 同 abstract，别名

> [!tldr]
> `[!tldr]` — 同 abstract，别名

### 任务类

> [!todo]
> `[!todo]` — 待办事项

> [!example]
> `[!example]` — 示例

> [!question]
> `[!question]` — 问题、FAQ

> [!help]
> `[!help]` — 同 question，别名

> [!faq]
> `[!faq]` — 同 question，别名

### 警示类

> [!warning]
> `[!warning]` — 警告

> [!caution]
> `[!caution]` — 同 warning，别名

> [!attention]
> `[!attention]` — 同 warning，别名

> [!danger]
> `[!danger]` — 危险、错误

> [!error]
> `[!error]` — 同 danger，别名

> [!bug]
> `[!bug]` — 缺陷、Bug

> [!failure]
> `[!failure]` — 失败、故障

> [!missing]
> `[!missing]` — 缺失内容

### 其他

> [!success]
> `[!success]` — 成功、完成

> [!check]
> `[!check]` — 同 success，别名

> [!done]
> `[!done]` — 同 success，别名

> [!cite]
> `[!cite]` — 引用、出处

> [!quote]
> `[!quote]` — 引用、引言

> [!definition]
> `[!definition]` — 定义

## 自定义标题

```
> [!tip] 我的自定义标题
> 提示框类型不变，但标题可以随意改
```

## 默认折叠

```
> [!note]- 
> 默认是藏起来的
```

## 嵌套内容

提示框内可以放任何 Markdown：

```
> [!example] 完整示例
> ### 标题
> - 列表项 1
> - 列表项 2
> 
> ```csharp
> var x = 1;  // 代码块也能放
> ```
> 
> | 表格 | 也能 |
> |------|------|
> | 放   | 进来 |
```


## 常用组合速记

| 场景 | 语法 |
|------|------|
| 重点强调 | `> [!important]` |
| 警告/注意 | `> [!warning]` |
| 危险/禁止 | `> [!danger]` |
| 提示/技巧 | `> [!tip]` |
| 补充说明 | `> [!info]` |
| 待办 | `> [!todo]` |
| 问题 | `> [!question]` |
| 示例 | `> [!example]` |
| 成功/完成 | `> [!success]` |
| 定义 | `> [!definition]` |
| 引用 | `> [!quote]` |
| Bug | `> [!bug]` |

## 颜色映射

```
[!note]        [抽象] 蓝紫色    ████
[!info]        [信息] 蓝色      ████
[!tip]         [提示] 青蓝色    ████
[!success]     [成功] 绿色      ████
[!question]    [问题] 黄色      ████
[!warning]     [警告] 橙色      ████
[!failure]     [失败] 红色      ████
[!danger]      [危险] 红色      ████
[!bug]         [缺陷] 红色      ████
[!example]     [示例] 紫色      ████
[!quote]       [引用] 灰色      ████
```


## 改变字体颜色

- <span style="color:red;">改变字体颜色</span>  → `<span style="color:red;">文本</span>`