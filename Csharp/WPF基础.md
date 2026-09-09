---
date: 2026-05-29
tags:
  - csharp
  - wpf
  - review
status: refined
sr-due: 2026-07-16
sr-interval: 18
sr-ease: 230
---

# WPF 基础

## WPF vs WinForm

### 几个基础概念

**像素（Pixel）**，是计算机屏幕上能显示的最小颜色方块。

**分辨率（Resolution）**，是指屏幕上像素点的总数量（或者总密度）。

**DPI，Dots Per Inch**，每英寸包含多少个像素点，本质上描述的是屏幕的「像素密度」。  
例如：
- 96 DPI：传统显示器
- 200+ DPI：高分屏、4K屏、笔记本 Retina 屏
DPI 越高，相同物理尺寸内塞入的像素越多。

### WinForm 的困境

WinForm 诞生于「**固定像素时代**」。即像素的物理尺寸是固定的。

它的布局体系、控件尺寸、坐标系统，大多直接基于：
- 像素（px）
- GDI/GDI+ 绘制
- 绝对定位

```
Button.Width = 100
```
在 WinForm 看来，就是「100个物理像素」。

但问题在于：不同显示器的 DPI 并不相同。随着2K、4K、Retina、高分屏笔记本出现后，**同样的 100px，在物理世界里会越来越小。**

可以这么理解，一个屏幕，在调节分辨率的时候就是在调整像素的大小，分辨率越高，由于屏幕尺寸固定，像素点的物理尺寸就越小。

### WPF 的解决方案

WPF引入了设备无关单位（DIP），1 DIP = 1/96 英寸。这是一个接近物理尺寸的**逻辑单位**，不是严格的物理长度。

比如：
- 系统告诉 WPF：「当前 DPI = 96」，那么 1 DIP = 1 物理像素
- 系统告诉 WPF：「当前 DPI = 144」（150% 缩放），那么 1 DIP = 1.5 物理像素

所以设计的时候指定尺寸为 960 DIP，那么在不同的机器上，WPF 会自动进行换算，DPI=96 的系统得到正好 960 个物理像素，DPI=144 的系统得到 1440 个物理像素，最后渲染出来的物理尺寸就差不多是 10 英寸。

> 至于为什么是差不多，涉及到无穷精度 → 离散化的矛盾，导致真实物理尺寸不一定是 10 英寸。

### 总结

`Button.Width = 96`
- 对于 WinForm：96 个物理像素，不同显示屏上物理尺寸不一致
- 对于 WPF：96 个 DIP，自动换算，结果总是差不多 1 英寸大小

| 特性 | Winform | WPF |
|------|---------|-----|
| 布局模式 | 基于绝对坐标（X, Y 像素定位） | 自适应布局（Grid、StackPanel 弹性盒） |
| 渲染引擎 | 主要依赖于 CPU 渲染 | DirectX，全面支持 GPU 硬件加速 |

> WinForm 是问 Windows 这个操作系统要的资源，而 WPF 是进程借助显卡开放的接口去渲染，里面所有的控件、动画都是利用显卡的计算能力，直接在屏幕上画出来的。

---

## XML

Extensible Markup Language 可扩展标记语言。

平常见的最多的是，VS 生成代码的 XML 注释文件，如下：

```
<summary> annotate </summary> 
<param name="id">  </param> 
<returns>   </returns>
<remark> xxx </remark>
```

是给这些注释贴上了各式各样的标签，如总结性的注释、对每个参数的注释、返回的注释等等。

- **什么是标记**：贴的标签就是标记的意思，用尖括号组成一个有开头和结尾的标签
- **什么是可扩展**：贴的这个标签是自己发明的

XML 有很多用途，但 XML 本质上只负责运输数据，程序读取 XML 文件做出各种各样的行为，比如使用 XML 做软件的说明书（配置文件），可以做不同软件之间的翻译官，将数据导出成统一格式的 XML 文件，各个系统都能读懂这个 XML 标签。

### JSON 文件格式

```json
{
    "城市": "北京",
    "温度": 25
}
```

JSON 文件更轻快、省流量。

---

## XAML

**Extensible Application Markup Language** — WPF 的 UI 语言，本质是 XML 文件，用标签画界面。

### XAML 的标签

#### 关键概念

XAML 里的**标签必须是后台有对应 C# 类**，写一个标签，WPF 会 new 一个标签对象放到窗口上。

```xml
<!-- 这行 XAML -->
<Button Content="点我" Width="100" Height="30"/>

<!-- 等价于 C# 代码 -->
Button btn = new Button();
btn.Content = "点我";
btn.Width = 100;
btn.Height = 30;
```

#### XAML 的标签都能代表什么

① **WPF 控件**：具备交互功能（接收鼠标点击和键盘输入），继承自 `System.Windows.Controls.Control` 基类。跟 WinForm 的控件不同，WPF 背后生成的 C# 控件，是一套**纯现代的、基于图形学和面向对象思想重写**的全新控件体系。比如 WPF 的按钮在操作系统眼里，根本不是一个「按钮」，而是一堆**由数学公式计算出来的线条、图形和渐变色**。

② **UI 元素**：布局面板（给里面的控件排版，比如 Grid）、装饰元素（画边框或形状，比如 Border）

③ **配置与数据标签**

#### XAML 标签怎么跟后台进行联系

WPF 把界面和逻辑分在两个文件里：
- `.xaml` 文件 = 只管「长什么样」— 放什么控件、什么颜色、多大尺寸
- `.xaml.cs` 文件 = 只管「干什么」— 按下按钮之后执行什么逻辑

连接它们的是两个东西：`x:Name`（给控件起名，cs 文件就能访问它）和 `Click="..."`（点按钮时调用哪个方法）。

> 参考：[[../resources/WPF基础知识点完全参考手册|WPF 参考手册]] #3 代码后置

#### XAML 语法

> 参考：[[../resources/WPF基础知识点完全参考手册|WPF 参考手册]] #2 XAML 语法

> **自闭合标签**：如果一个标签中完全没有子元素，也没有任何文字，可以选择自己把自己关起来。

基本知识点：
- `x:Name` — 给控件起名字，在 C# 代码里可以直接用这个名字访问它
- `TextBlock` — 显示一段只读文字，比 Label 更轻量
- `TextBox` — 允许用户输入文字的输入框
- `Button` — 按钮，Content 属性决定显示什么文字
- `Click="BtnGreet_Click"` — 事件绑定，点击按钮时执行 BtnGreet_Click 方法
- `sender` — 事件处理方法的第一个参数，就是被点击的那个按钮本身

### XAML 常用的标签

#### 布局容器

WPF 用面板（Panel）来决定控件摆在哪里。面板就是容器，负责决定里面的子控件怎么排列，不同面板有不同的排列规则。

![](../pic/Pasted%20image%2020260526170002.png)
![](../pic/Pasted%20image%2020260526170125.png)
![](../pic/Pasted%20image%2020260526170142.png)

> 参考：[[../resources/WPF基础知识点完全参考手册|WPF 参考手册]] #4 布局面板

#### 常用控件

每个控件都有自己的专属事件，CheckBox 的 Checked/Unchecked，Slider 的 ValueChanged。都是 XAML 声明控件 + 事件名字 → C# 写处理方法。

CheckBox（勾选）→ RadioButton（单选）→ ComboBox（下拉）→ ListBox（列表）→ Slider（滑块）+ ProgressBar（进度条）

> 参考：[[../resources/WPF基础知识点完全参考手册|WPF 参考手册]] #5 常用控件速查

---

## 数据绑定

Binding 让 UI 和数据自动同步。不需要写 C# 代码，WPF 能自动完成这件事情，实现界面逻辑跟业务功能解耦。

### 两种常见的绑定形式

#### ① 控件间的绑定

下面的例子中，两个 TextBlock 都绑定到了 Slider：

```xml
<Border Background="#E8F5E9" CornerRadius="8" Padding="16" Margin="0,5">
    <StackPanel>
        <Slider x:Name="SldFontSize"
                Minimum="12" Maximum="48" Value="20"
                Width="300" Margin="0,0,0,10"/>
        <TextBlock Text="{Binding ElementName=SldFontSize, Path=Value, StringFormat='字号: {0:F0}'}"
                               Foreground="#555" Margin="0,0,0,8"/>
        <TextBlock Text="拖动滑块改变我的大小！"
                    FontSize="{Binding ElementName=SldFontSize, Path=Value}"
                    Foreground="#2B579A" FontWeight="Bold"/>
    </StackPanel>
</Border>
```

绑定语法：`{Binding ElementName=源控件, Path=源属性}`

#### ② 控件与 C# 类的绑定（INotifyPropertyChanged）

```csharp
public partial class Lesson04_Binding : Page
{
    private readonly PersonViewModel _vm = new PersonViewModel();

    public Lesson04_Binding()
    {
        InitializeComponent();
        this.DataContext = _vm;
    }
    
    private void BtnIncrement_Click(object sender, RoutedEventArgs e)
        _vm.Counter++;
}

public class PersonViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    
    private string _userName = "张三";
    public string UserName
    {
        get { return _userName; }
        set { _userName = value; OnPropertyChanged(); }
    }

    private int _userAge = 25;
    public int UserAge
    {
        get { return _userAge; }
        set { _userAge = value; OnPropertyChanged(); }
    }

    private int _counter;
    public int Counter
    {
        get { return _counter; }
        set { _counter = value; OnPropertyChanged(); }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### 理解数据绑定

#### 绑定「合同」

```xml
<TextBox Text="{Binding UserName}"/>
```

合同说明：TextBox 的 Text 跟 UserName 绑定，UserName 变了，Text 也要跟着变。引出两个问题：**谁是 UserName？怎么做到跟着变？**

#### 「合同」上的对象是谁？

WPF 用 DataContext 解决这个问题。每个控件都有一个 DataContext 属性（每一个控件背后都是一个 C# 类）。如果合同上写了绑定 UserName，但没有给该控件设置 DataContext 属性，WPF 就会沿着控件树往上寻找，直到找到 DataContext 里有 UserName 这个属性。
（上面的例子里，就是给page这个控件的DataContext属性赋值了一个类，这个类里有UserName属性）

>WPF不鼓励给控件起名字，而是利用树的继承性，不管是这里的控件树，还是事件的路由机制

#### 怎么做到跟着变？

数据源必须实现 `INotifyPropertyChanged` 接口，通过触发 `PropertyChanged` 事件来向文本框发布数据变更通知。

#### 合同上可以追加一些「条款」

- `Binding xxx` — 跟谁连
- `Mode = TwoWay` — 两头都跟着变
- `UpdateSourceTrigger = PropertyChanged` — 每输入/删除一个字符，后台的数据立刻跟着变；`LostFocus` 则是失去焦点时后台数据才变

> TextBox 默认的绑定合同里就是 `Mode = TwoWay`

> 本质上，数据绑定都是 C# 属性之间的同步，第一种控件间的绑定，底层依然是具体的 C# 类。

---

## MVVM 模式

Model-View-ViewModel：数据变了，界面跟着变；界面变了，数据也跟着变。

---

## 样式与资源

Style 是 WPF 专门提供的一种机制，用来批量、统一地控制界面元素的长相。

### 三个核心概念

- **Style** — 属性设置集合，有 `x:Key` 的叫显式样式（需手动引用），没有的叫隐式样式（自动作用于所有同类型控件）
- **ControlTemplate** — 控件模板，完全重画控件的外观
- **Trigger** — 触发器，条件满足时自动改属性

> 参考：[[../resources/WPF基础知识点完全参考手册|WPF 参考手册]] #11

---

## 事件处理

### 事件绑定和常用事件列表

> 参考：[[../resources/WPF基础知识点完全参考手册|WPF 参考手册]] #6.1 事件绑定, #6.2 常用事件列表

### 路由事件（Routed Event）

> 参考：[[../resources/WPF基础知识点完全参考手册|WPF 参考手册]] #6.3 路由事件

---

## 基础信息

- 基类：`System.Windows.Window`
- 依赖库：`PresentationCore`、`PresentationFramework`、`WindowsBase`
