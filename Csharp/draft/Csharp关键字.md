---
date: 2026-06-01
tags:
  - review
  - Csharp
status: refined
sr-due: 2026-07-28
sr-interval: 30
sr-ease: 250
---
### abstract
#### 1. 本质

abstract 的两个核心含义：
- **类**标记为 abstract → 不能直接 `new`，只能被继承
- **成员**标记为 abstract → 没有方法体，子类**必须** override

#### 2. 抽象类

| 可包含         | 不可                           |
| ----------- | ---------------------------- |
| 字段、属性、普通方法  | `new` 实例化                    |
| 构造函数（供子类调用） | `sealed` 修饰符（矛盾：sealed 阻止继承） |
| 抽象方法/属性     | `private abstract` 方法        |

虽然抽象类不能直接 `new`，但子类实例化时会**隐式调用**抽象类的构造函数，用于初始化共有状态。

#### 3. 抽象方法

```csharp
public abstract class Shape
{
    public abstract double GetArea();  // 没有 {} 体，分号结尾
}
```

规则：
- 必须在抽象类中声明
- 不能有方法体（连 `{}` 都不能写）
- 不能用 `private`、`static`、`virtual` 修饰
- 子类必须用 `override` 实现

#### 4. 与 virtual 的对比

|          | `abstract`         | `virtual`                |
| -------- | ------------------ | ------------------------ |
| 是否有默认实现  | 无                  | 有                        |
| 子类是否必须重写 | **必须**             | 可选                       |
| 所在类      | 必须是抽象类             | 普通类即可                    |
| 组合       | 不能和 `virtual` 同时出现 | 可被 `override` 或 `new` 遮蔽 |

#### 5. 与接口的区别

| | 抽象类 (Is-a) | 接口 (Can-do) |
|---|---|---|
| 关系 | "**是**什么" | "**能**做什么" |
| 方法实现 | 可有具体实现 | 无（C# 8+ 可有默认实现） |
| 构造函数 | 有 | 无 |
| 字段 | 可以有 | 不能有 |
| 多继承 | 单继承 | 可实现多个接口 |
| 访问修饰符 | 完整控制 | 成员默认 `public` |

> **选择原则**：需要共享代码/字段 → 抽象类；只需定义能力契约 → 接口。两者可组合——抽象类实现接口，提供默认行为。


### this

`this` 代表**当前类的实例**，引用堆中那个具体的对象。

#### 1. 区分同名变量

```csharp
class Person
{
    private string name;
    public Person(string name)
    {
        this.name = name;  // this.name = 字段；name = 形参
    }
}
```

#### 2. 传递当前对象 / 隐式调用

```csharp
class Person
{
    public string Name;
    public void SayHello()
    {
        Console.WriteLine(this.Name);  // 显式用 this
        SayGoodbye();      // 隐式等价于 this.SayGoodbye()
    }
    public void SayGoodbye() { }
}
```

实例方法本质上隐藏了一个参数——编译器签名是 `void SayHello(Person this)`。调用 `A.SayHello()` 时，`A` 的引用被传入作为 `this`。所以 `this`**不能在 static 方法中使用**——静态方法属于类本身，不关联任何实例。

#### 3. 构造函数互相调用

```csharp
class Person
{
    public string Name;
    public int Age;

    public Person(string name) : this(name, 0) { }  
    // 调用两个参数的构造函数

    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
}
```

#### 4. 声明扩展方法

第一个参数用 `this` 修饰目标类型，让密封类也能”新增”方法：

```csharp
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
}

// “hello”.IsNullOrEmpty() 等价于 StringExtensions.IsNullOrEmpty(“hello”)
```

#### 5. 索引器

索引器是带参数的属性，用 `this` 代替名称，像数组一样访问对象：

```csharp
class StringCollection
{
    private List<string> _items = new();

    public string this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }
}

var sc = new StringCollection();
sc[0] = “hello”;   // 等价于 sc.this[0].set(“hello”)
Console.WriteLine(sc[0]);
```

- 索引器可重载（如 `this[int]` 和 `this[string]` 并存）
- 索引器总是实例成员，不能是静态

> **两个核心作用**：
> ① 引用当前实例（区分字段/形参、传递对象、构造函数互调）；
> ② 代替名称（索引器、扩展方法第一个参数）。

## 闪卡

#csharp-flashcards

this 关键字的两个核心作用：
1. ==1;;引用当前实例==（区分字段/形参、传递当前对象、构造函数互调）
2. ==1;;代替名称==（索引器、扩展方法的第一个参数）
在索引器中写 `public int this[int index]`，是用 `this` 代替了具体的方法名；
在扩展方法中写 `this string str`，也是在语法上指代该类型的实例。
注意：this 不能在==1;;static==方法中使用，因为==1;;静态方法属于类，不关联任何实例==，但扩展方法是例外，它属于语法糖，编译器会将==1;;实例调用转为一个静态方法调用==。
<!--SR:!2026-09-22,61,230-->


隐式类型转换运算符
为了让两个==1;;没有继承关系的;;什么定语？==类型可以互相隐式转换，相对应的还有==1;;explicit operator 1;;强制类型转换运算符==。
示例：
```csharp
public static implicit operator TypedValue[](TypedValueList src)  
{  
return src != null ? src.ToArray() : null;  
}
```
1、类型转换运算符必须是==1;;静态==的
2、通常写在TypedValueList这个类中,或者说==1;;上层的类，后来的类==中。
3、明确概念，是==1;;类的对象，即src从TypedValueList转成TypedValue[];;谁==进行隐式转换
<!--SR:!2027-02-04,148,239-->