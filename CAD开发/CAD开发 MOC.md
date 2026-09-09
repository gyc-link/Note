---
date: 2026-05-09
tags:
  - moc
  - CAD二次开发
status: draft
---

# CAD 二次开发 MOC

## 基础概念

先看这两篇，建立对 CAD 数据库和 API 的基本认知：

- [[CAD二次开发基础]] — Wrapper 架构、图形数据库四大容器、符号表、事务、ObjectId/Handle、DBObject/Entity
- [[文档和图形数据库]] — 数据库创建/保存/读取、Wblock、Insert、文档管理、锁定

## 数据扩展（从轻到重）

- [[扩展数据、扩展字典、有名对象字典]] — XData（≤16KB）→ 扩展字典（单实体）→ 有名对象字典（全局），以及组字典、多线样式
- 适用场景判断：临时标记用 XData，与实体绑定的复杂数据用扩展字典，全局配置用有名对象字典

## 交互与界面

- [[事件]] — 应用程序事件 / 文档事件 / 对象事件，事件处理四原则
- [[draft/用户界面-draft]] — 模态窗口（ShowModalDialog/ShowModalWindow）、WPF vs WinForms 基础

## 常见问题

- 修改图形 → 用实体类（Entity）
- 精确几何计算 → 用几何类（Curve3d、Line3d 等）
- 对象没有 ObjectId → 还没加入数据库，先 AddNewlyCreatedDBObject
- 事务 Abort 后 ObjectId 失效 → 需要重新获取

## 相关领域

- [[../Csharp/draft/委托]] — 事件机制的基础
- [[../Csharp/接口、枚举器和迭代器]] — IEnumerable 在 CAD 符号表中的实现
- [[../数学/向量]] — 三维点/向量运算基础
- [[../数学/向量的叉乘]] — 判断方向、求法向量
