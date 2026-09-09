---
date: 2026-05-08
tags:
  - "#markdown"
  - "#网络"
status: draft
---

## 针对 Obsidian 与 VS Code 之间的 Markdown 链接兼容性问题

### 1. 路径逻辑转换

- **Obsidian 的“聪明”模式**：它默认使用“最简路径”，只要文件名唯一，它能自动在库里搜寻，这导致 VS Code 无法通过文件系统定位。
- **标准相对路径**：使用 `../` 明确指向父级或同级目录。
    - **VS Code**：通过文件系统指针寻址，必须依赖相对路径。
    - **Obsidian**：兼容相对路径，能完美识别。

### 2. 空格问题的解决

- **规则**：将空格替换为 `%20` 或用 `< >` 包裹。
- **结果**：符合 URI 标准，解决markdown预览引擎因空格导致的链接断开。

### 3. WikiLink

WikiLink是Obsidian 内部默认格式，极致简洁。

### 4. 修改obsidian配置

#### 第一步：修改 Obsidian 原生设置

进入 `设置 -> 文件与链接`：

- **链接格式**：选择 **“相对文件路径”**（解决 VS Code 找不到图的问题）。
- **使用 Wiki 链接**：**关闭**（确保新生成的图片直接是 `![]()` 格式）。

#### 第二步：一键修复旧文档

利用插件（如 _Consistent Attachments and Links_）：

1. 运行 `Convert All Embed Paths to Relative`（转换所有嵌入路径为相对路径）。
2. 运行 `Replace all WikiLinks with Markdown Links`（将 WikiLink 彻底转为标准 Markdown）。
    - _注：此过程会自动将空格转为 `%20` 并生成 `../` 路径。_

#### 第三步：验证结果

完成转换后，你的文件链接应呈现如下形态： `![](../../pic/Pasted%20image%2020260508.png)`

> **总结：** 只要坚持 **“相对路径 + %20 编码”**，笔记就能脱离 Obsidian 的环境束缚，在 VS Code、GitHub 甚至直接发布到网页时，图片依然能够精准显示。



