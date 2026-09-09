栈和队列是限定插入和删除只能在表的端点进行的线性表（所以是线性表的子集）

## 引入
### 栈

![](../../pic/Pasted%20image%2020260609175627.png)

栈的操作具有 **后进先出** 的固有特性。因为栈限制了插入和删除只能在表尾。

#### 具有后进先出特性的问题

- 括号匹配的检验
- 表达式求值
- 函数调用
- 递归调用的实现

### 队列

队列的操作具有 先进先出 的固有特性。因为队列限制插入在表尾，删除在表头。
跟排队一样。

#### 具有先进先出特性的问题

- 脱机打印输出：按申请的先后顺序依次输出
- 多用户系统中，多个用户排成队，分时循环使用CPU
- 实时控制系统中，信号按接收的先后顺序依次处理

## 栈的定义和特点

- 栈（stack） (Last In First Out的线性表) LIFO
- 仅在表尾进行插入，删除的线性表，表尾称为**栈顶Top**，表头称为**栈底Base**
- 入栈 压入 PUSH 
- 出栈 弹出 POP
- 顺序栈或链栈，顺序栈更常用

#### C# 中的栈 `Stack<T>`

位于 `System.Collections.Generic`，泛型集合，元素类型必须一致。

**构造**

```csharp
Stack<int> stack = new Stack<int>();
Stack<int> stack2 = new Stack<int>(32);          // 指定初始容量
Stack<int> stack3 = new Stack<int>(existingList); // 从已有集合初始化
```

**实例方法**

| 方法 | 签名 | 用途 |
|------|------|------|
| `Push` | `(T item) → void` | 元素压入栈顶 |
| `Pop` | `() → T` | 弹出栈顶元素并返回；栈空时抛 `InvalidOperationException` |
| `Peek` | `() → T` | 查看栈顶元素但不移除；栈空时抛异常 |
| `TryPop` | `(out T result) → bool` | 安全弹出，成功返回 `true`，失败（栈空）返回 `false` |
| `TryPeek` | `(out T result) → bool` | 安全查看栈顶，不抛异常版本 |
| `Clear` | `() → void` | 清空所有元素 |
| `Contains` | `(T item) → bool` | 是否包含指定元素（线性查找 O(n)） |
| `ToArray` | `() → T[]` | 复制成一个数组（从栈顶到栈底的顺序） |
| `CopyTo` | `(T[] array, int index) → void` | 复制到已有数组的指定位置 |
| `TrimExcess` | `() → void` | 如果元素数不到容量的 90%，收缩容量以节省内存 |

**属性**

| 属性 | 类型 | 说明 |
|------|------|------|
| `Count` | `int` | 栈中元素个数 |

```csharp
Stack<int> stack = new Stack<int>();
stack.Push(10);
stack.Push(20);
stack.Push(30);

stack.Count;   // 3
stack.Peek();  // 30（不移除）
stack.Pop();   // 30
stack.Pop();   // 20

// 安全弹出
if (stack.TryPop(out int val))
    Console.WriteLine(val);  // 10
else
    Console.WriteLine("栈空");

// 遍历（从栈顶到栈底，不改变栈内容）
foreach (int item in stack)
    Console.WriteLine(item);
```

> [!NOTE]
> `Stack<T>` 的 `foreach` 遍历顺序是**从栈顶到栈底**，不会移除元素。如果要边遍历边弹空，用 `while (stack.Count > 0) { var v = stack.Pop(); }`。

## 队列的定义和特点

- 队列 queue FIFO