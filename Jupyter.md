---
jupyter:
  jupytext:
    formats: ipynb,md
    text_representation:
      extension: .md
      format_name: markdown
      format_version: '1.3'
      jupytext_version: 1.19.3
  kernelspec:
    display_name: ai_env
    language: python
    name: python3
---

```python jupyter={"is_executing": false}
import numpy as np
A = np.dot([1,2],[2,4])
print(A)
```

## Jupyter是什么

Jupyter是一个交互式编程环境
允许一小段一小段执行代码，并立即看到结果

## .ipynb文件是什么？

全称是 IPython Notebook
本质上是一个Json文件

![255](pic/Pasted%20image%2020260724111638.png)

## .ipynb文件怎么运行的


Python
 |
 | 负责执行代码
 |
python.exe


Jupyter
 |
 | 负责提供交互界面
 |
.ipynb


Kernel
 |
 | 连接两者
 |
ipykernel

Jupyter 是一个让 Python 可以“分块运行”的交互环境；
ipynb 是它的文件格式；
ipykernel是在某个 Python 环境中安装 ipykernel 后，这个 Python 环境就具备了启动 Jupyter Kernel 的能力，可以被 Jupyter 选择作为 Notebook 的执行环境。
VS Code 的 Jupyter 插件只是把这个环境集成进编辑器。

## 常见的库

numpy
matplotlib
