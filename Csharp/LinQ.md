---
tags:
  - "#Csharp"
  - "#review"
status: draft
date: 2026-05-12
---

> 前置：[[接口、枚举器和迭代器]]


### lambda表达式
#### 一些规则
(参数列表) => 表达式 / 代码块
- 无参：`() => 123`
- 一个参数可省略括号：`x => x * 2`
- 多参：`(x,y) => x + y`
- 多行用 `{ }` + `return`：
#### 怎么调用？
1、本质是委托调用，可以是赋值给委托变量调用，如
```csharp
// 赋值
 Action hello = () => Console.WriteLine("Hello Lambda"); 
// 直接调用 
hello();
```
2、LINQ中作为方法参数传入调用，因为就是LINQ中的很多方法里使用了委托。
#### 通过return返回lambda
```csharp
定义一个函数，返回值类型是一个委托
Func<int, int> MakeDoubleFunc()
{
    return x => x * 2;
}
```
因为返回的是lambda表达式，是一个函数，所以用对应的委托（一个输入int型的输入参数和int型的返回值）作为这个函数的类型。
```csharp
Func<int, int> f = MakeDoubleFunc();  
int result = f(10);
或者直接
int result = MakeDoubleFunc()(10)
```





## 1. LINQ 是什么

LINQ（Language Integrated Query，语言集成查询）是一组定义在 `System.Linq` 命名空间下的**扩展方法**，对 `IEnumerable<T>` 进行查询操作。从内部看，Where、Select 等方法接收的是**委托**（`Func<T, bool>`、`Func<T, TResult>` 等），所以可以直接传入 lambda。

## 2. 为什么 LINQ 能在 `IEnumerable<T>` 上工作

`IEnumerable<T>` 的 `GetEnumerator()` 提供了**逐项遍历**的能力——只要能一个个拿出来，就能一个个筛选、转换、排序。LINQ 的所有操作都建立在这个基础之上。

| 接口 | LINQ 能直接用？ | 原因 |
|------|:---:|------|
| `IEnumerable<T>` | 能 | 元素类型已知 |
| `IEnumerable` | 不能 | 元素类型是 `object`，需先 `.Cast<T>()` 或 `.OfType<T>()` |

在 .NET 中，数组、`List<T>`、`Dictionary<TKey,TValue>` 等都实现了 `IEnumerable<T>`，所以 LINQ 可以直接在上面工作。


## 3. 常用方法

```csharp
var items = new List<Person> { ... };

// 筛选
var adults = items.Where(x => x.Age > 18);

// 投影——只取某个属性
var names = items.Select(x => x.Name);

// 排序
var sorted = items.OrderBy(x => x.Name);

// 取第一个（无匹配时抛异常）
var first = items.First(x => x.Id == 5);

// 取第一个（无匹配时返回 default）
var firstOrNull = items.FirstOrDefault(x => x.Id == 5);

// 判断是否有满足条件的元素
bool hasAdmin = items.Any(x => x.Role == "admin");

// 计数
int count = items.Count(x => x.IsActive);
```

| 方法 | 作用 | 返回类型 |
|------|------|------|
| `Where` | 筛选 | `IEnumerable<T>` |
| `Select` | 投影/转换 | `IEnumerable<TResult>` |
| `OrderBy` / `OrderByDescending` | 排序 | `IOrderedEnumerable<T>` |
| `ThenBy` / `ThenByDescending` | 二级排序 | `IOrderedEnumerable<T>` |
| `First` / `FirstOrDefault` | 取第一个 | `T` |
| `Last` / `LastOrDefault` | 取最后一个 | `T` |
| `Single` / `SingleOrDefault` | 取唯一元素 | `T` |
| `Any` | 是否有元素满足条件 | `bool` |
| `All` | 是否全部满足条件 | `bool` |
| `Count` | 计数 | `int` |
| `Skip` / `Take` | 跳过/取前 N 个 | `IEnumerable<T>` |
| `Distinct` | 去重 | `IEnumerable<T>` |
| `GroupBy` | 分组 | `IEnumerable<IGrouping<TKey, T>>` |
| `Join` / `GroupJoin` | 连接 | `IEnumerable<TResult>` |
| `ToList` / `ToArray` | 立即执行并收集 | `List<T>` / `T[]` |

## 4. 两种语法

```csharp
// 方法语法（Method Syntax）—— 链式调用 lambda
var result = items.Where(x => x.Age > 18)
                  .OrderBy(x => x.Name)
                  .Select(x => x.Name);

// 查询语法（Query Syntax）—— 像 SQL
var result = from x in items
             where x.Age > 18
             orderby x.Name
             select x.Name;
```

> 编译器会把查询语法翻译成方法语法，二者完全等价。方法语法更灵活（可以链式调用任何扩展方法），查询语法在 `join` / `group by` 等复杂场景可读性更好。

### 查询语法独有的操作

```csharp
// join
var result = from p in persons
             join o in orders on p.Id equals o.PersonId
             select new { p.Name, o.Product };

// group by
var groups = from p in persons
             group p by p.City into cityGroup
             select new { City = cityGroup.Key, Count = cityGroup.Count() };
```

## 5. 延迟执行（Deferred Execution）

**LINQ 查询不是在定义时执行，而是在遍历时才执行**：

```csharp
var items = new List<int> { 1, 2, 3 };
var query = items.Where(x => x > 1);  // 还没执行

items.Add(4);                           // 后续添加的也会被"看到"

var result = query.ToList();            // 这时候才真正筛选
// result = [2, 3, 4]
```

| 延迟执行 | 立即执行 |
|----------|----------|
| `Where`、`Select`、`OrderBy`、`Skip`、`Take`、`Distinct`、`GroupBy`、`Join` | `ToList()`、`ToArray()`、`ToDictionary()`、`First()`、`Single()`、`Count()`、`Any()`、`All()` |

> 延迟执行意味着每次遍历 `query`，都会**重新**走一遍筛选逻辑。如果结果需要被多次使用，先 `.ToList()` 存起来，避免重复计算。

**"多次遍历"的陷阱**：

```csharp
var query = items.Where(x => ExpensiveCheck(x));

query.Count();  // 遍历一次
query.ToList(); // 又遍历一次 → ExpensiveCheck 执行了两遍！
```

## 6. 与 foreach 的关系

```csharp
// foreach 底层就是调用 GetEnumerator
foreach (var x in items.Where(x => x.Age > 18))
{
    Console.WriteLine(x.Name);
}
```

`foreach` + LINQ 的配合是：LINQ 返回 `IEnumerable<T>`，`foreach` 消费它。每次 `foreach` 迭代，LINQ 的 lambda 都会**实时执行**。

## 7. 总结速查

| 概念 | 核心作用 | 关键点 |
|------|------|------|
| LINQ | 对集合进行查询/转换 | 扩展方法，基于 `IEnumerable<T>` |
| `Where` | 筛选 | 返回满足条件的元素 |
| `Select` | 投影 | 将每个元素转换成另一种形式 |
| 方法语法 | lambda 链式调用 | `.Where().Select()` |
| 查询语法 | SQL 风格 | `from x in ... where ... select` |
| 延迟执行 | 定义时不执行，遍历时才执行 | Where/Select 是延迟的 |
| 立即执行 | 调用即执行 | `ToList()`、`First()`、`Count()` |
| `IEnumerable` vs `IEnumerable<T>` | 非泛型 LINQ 无法直接操作 | 需 `.Cast<T>()` 桥接 |

## 8. 匿名类型

### 8.1 是什么

匿名类型是编译器**帮你自动生成**的类——你不用写 `class`，编译器根据 `new { }` 里的属性推断出一个只读的类。

```csharp
// 你不写 class Person { string Name; int Age; }
// 直接：
var person = new { Name = "张三", Age = 25 };
```

编译器在背后做的事：
- 生成一个类（名字类似 `<>f__AnonymousType0'2`，你永远看不到）
- 每个属性都是 **只读** 的（只有 `get`，没有 `set`）
- 自动生成 `Equals`、`GetHashCode`、`ToString` 方法

### 8.2 为什么要它——和 LINQ 的关系

LINQ 的 `Select` 做投影时，你经常只想取几个字段，不想新建一个类：

```csharp
// 只取名字和城市，没必要为这个组合单独写一个 class
var result = persons.Select(p => new { p.Name, p.City });
// result 的类型：IEnumerable<匿名类型 { string Name, string City }>
```

等价的查询语法：
```csharp
var result = from p in persons
             select new { p.Name, p.City };
```

### 8.3 三条约束（以及为什么）

| 约束 | 原因 |
|------|------|
| 只能用 `var` 声明 | 编译器生成的类名你不知道，没法写显式类型 |
| 属性只读 | 编译器只为每个属性生成 `get`，不生成 `set`——匿名类型的设计意图是"数据快照"，不是可变对象 |
| 只能做局部变量 | 匿名类型的类型名在方法外不可见，所以不能做方法参数、返回值、字段——它的作用域被限制在当前方法内 |

> 如果需要把匿名类型的数据传出方法，要么用 `object` + 反射（不推荐），要么老老实实写一个具名类 / record。

## 9. 补充：类型筛选、索引与滑动窗口

### Cast vs OfType

| 方法 | 态度 | 遇到转不过去的元素 |
| --- | --- | --- |
| `Cast<T>()` | 信任 | 抛异常 |
| `OfType<T>()` | 尝试 | 静默跳过 |

> 记忆点：**cast 是信任，oftype 是尝试**。

### Select 的索引重载

`Select` 有一个重载，lambda 可以收两个参数：第一个是当前元素，第二个是它在集合里的索引。

```csharp
items.Select((item, index) => $"{index}: {item.Name}");
```

### 排序：传比较委托

`List<T>.Sort` 接受一个返回 `int` 的比较委托，两个参数就是 list 里的两个元素：

```csharp
result.Sort((a, b) => a[0].CompareTo(b[0]));
```

返回负数 / 0 / 正数，分别表示 a 排前 / 两者相等 / a 排后。

### Zip + Skip：滑动窗口

```csharp
var pairs = valuesToPair.Zip(
        valuesToPair.Skip(1), (a, b) => new double[] { a, b })
    .ToList();
```

- `Zip` 把两个序列按位置合并，每个位置合并成一个 `[a, b]`
- `Skip(1)` 得到"错位一格"的新序列
- 两者一配，就成了**相邻两两成对**——滑动窗口形式的分组
- 多出来的元素会被 `Zip` 忽略（以短的那个为准）

### SelectMany：扁平化

把嵌套的"集合的集合"摊平成一维，同时支持在一对多关系里做投影转换。比如一个"字典的集合"，`SelectMany` 之后就是每个字典的所有 pair 组成的一维集合。

### 闪卡

#csharp-flashcards

`Cast<T>` 和 `OfType<T>` 的区别::`Cast` 是**信任**（转不了就抛异常），`OfType` 是**尝试**（转不了就跳过）。

```csharp
var pairs = valuesToPair.Zip
		(valuesToPair.Skip(1), (a, b) => new double[] { a, b }).
		ToList();
```
ZIP是什么操作？这么写是为了做什么？
?
`Zip` 把两个序列按位置合并成对；配合 `Skip(1)`，就是把序列错位一格再和自己配对，结果是**相邻两两成对的滑动窗口**。多出来的元素被忽略。
<!--SR:!2026-09-15,6,190-->

LINQ的foreach，==对逐个元素进行操作==。
<!--SR:!2026-10-01,110,296-->

匿名方法是什么，因为什么叫它匿名？是用什么特殊的写法怎么声明的？会显示声明返回值吗？什么情况可以省略匿名函数的参数列表？匿名函数可不可以用作用域外的局部变量？可以的话为什么？
?
- 匿名方法是在实例化委托时内联声明的方法，因为只会被使用一次，所以不需要名字。
- 匿名方法一般用在声明委托变量的初始化表达式，组合委托时在赋值语句的右边，为委托增加事件时在赋值语句的右边
- ![](../pic/Pasted%20image%2020260403140453.png)
- 不显示声明返回值，在语句块里写return，如果委托有返回值的话
- 匿名方法的参数列表必须跟委托的参数列表一致，包括ref，out修饰符。如果匿名方法中没使用参数且委托的参数列表不使用out，则可以省略参数列表。
- 匿名方法必须==省略params==关键字
- 匿名方法可以捕获外围作用域的==局部变量==和环境，具名方法不行。只要委托还在，这个外部变量就不会被回收。
<!--SR:!2027-02-17,180,230-->

lambda表达式的=>读作什么？什么情况需要显示参数的类型（一般情况下是参数是隐式类型的）？参数列表的圆括号什么情况下可以省略？
?
- Lambda 运算符 =>,读作"==goes to=="
- 编译器可以从委托的声明中知道委托参数的类型，所以lambda表达式可以省略类型参数。除非有ref，out参数，此时要显示类型
- 对于只有一个隐式类型参数，可以省略两端的圆括号。否则必须要加圆括号，没有参数时，使用空的圆括号
- ![](../pic/Pasted%20image%2020260403142852.png)
<!--SR:!2026-09-21,67,210-->


LINQ的核心价值在于用一致的方法查询==不同类型==的数据源。内存集合，数据库，xml
<!--SR:!2026-09-21,114,290-->

供LINQ查询的数据源必须实现==IEnumerable或IQueryable==接口的类型，常见的如数组，列表。
<!--SR:!2027-03-27,206,250-->

LINQ的查询是延迟的，除非使用==ToList()或者ToArray()==方法将查询结果转换为列表或者数组，LINQ查询才会立即执行。
<!--SR:!2026-12-14,189,310-->

LINQ的两种语法风格
?
- 查询语法，类似于SQL查询语句
- 方法语法，利用c#的扩展方法加lambda表达式。
<!--SR:!2026-10-01,122,290-->

`LINQ的ofType<T>`的作用::用来筛选指定的类型
<!--SR:!2026-12-03,181,310-->


什么是查询表达式Query Expression，为什么它是声明式语法？怎么做到筛选，排序，分组和投影数据？
?
- 是一种用**声明式语法**（类似 SQL）编写的代码结构，用于从数据源中**筛选、排序、分组、投影**数据。
- **声明式（Declarative）** 是一种编程风格或编程范式，是说明你想要什么，-**命令式（Imperative）** 编程，详细描述如何做，如if else等
- **from**: 指定数据源（如集合、数组等）。
- **where**: 用于过滤数据，指定查询的条件。
- **select**: 定义要查询的数据项（可以是原始数据或计算结果）。
- **orderby**: 用于排序数据（可选）。
- **group**: 用于分组数据（可选）。
- `var result = from student in students`
             `where student.Age > 18`
             `select student.Name;`
<!--SR:!2026-10-12,121,268-->

LINQ中的==1;;方法链式调用==
- 是一种编程技巧，它允许==1;;你将多个方法调用连接在一起，在一个表达式中依次调用多个方法==。
- `var result = people.Where(p => p.Age > 18)`
                   `.OrderBy(p => p.Name)`
                   `.Select(p => p.Name);`
<!--SR:!2026-09-29,75,228-->

LINQ中，凡是返回 ==IEnumerable<...>== 的，都是延迟执行。
<!--SR:!2027-06-10,281,287-->

LINQ中，凡是返回 ==int / double / bool / List<>== 的，都是立即执行。
<!--SR:!2027-04-20,253,288-->

为什么sum,any,count是聚合操作::就是把多条数据聚合成一个结果。
<!--SR:!2027-06-20,284,287-->