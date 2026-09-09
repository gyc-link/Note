---
date: 2026-05-11
tags:
  - CAD二次开发
  - review
status: draft
---


## 1. 自定义对话框
CAD提供了Application类下的静态函数来显示窗口
ShowModalDialog ShowModalWindow

`public static DialogResult ShowModalDialog(Form formToShow) `

他们是模态窗口，就是窗体弹出来后，用户不能操作后面的主窗体，反之非模态窗口用户就能操作后面的主窗体，比如cad的图层管理器、属性面板是非模态窗口。
分别弹winform和wpf





