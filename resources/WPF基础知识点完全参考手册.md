# WPF 基础知识点完全参考手册

> 覆盖本教程全部 8 节课的所有知识点。可当作速查手册使用。

---

## 目录

1. [WPF 是什么](#1-wpf-是什么)
2. [XAML 语法](#2-xaml-语法)
3. [代码后置 (Code-Behind)](#3-代码后置-code-behind)
4. [布局面板](#4-布局面板)
5. [常用控件速查](#5-常用控件速查)
6. [事件系统](#6-事件系统)
7. [数据绑定](#7-数据绑定)
8. [INotifyPropertyChanged](#8-inotifypropertychanged)
9. [ObservableCollection 与列表绑定](#9-observablecollection-与列表绑定)
10. [DataTemplate 数据模板](#10-datatemplate-数据模板)
11. [样式 (Style)](#11-样式-style)
12. [控件模板 (ControlTemplate)](#12-控件模板-controltemplate)
13. [触发器 (Trigger)](#13-触发器-trigger)
14. [资源 (Resources)](#14-资源-resources)
15. [值转换器 (IValueConverter)](#15-值转换器-ivalueconverter)
16. [附加属性](#16-附加属性)
17. [MVVM 模式入门](#17-mvvm-模式入门)
18. [常见问题与调试](#18-常见问题与调试)

---

## 1. WPF 是什么

**WPF** (Windows Presentation Foundation) 是微软的桌面应用 UI 框架。

核心设计理念：
- **界面与逻辑分离** — XAML 画界面，C# 写逻辑
- **数据驱动 UI** — 通过 Binding 让数据和界面自动同步
- **矢量渲染** — 无论分辨率多高都不模糊

**关键文件类型：**

| 文件         | 作用               |
| ---------- | ---------------- |
| `.sln`     | 解决方案文件，管理多个项目    |
| `.csproj`  | 项目文件，定义编译选项和文件列表 |
| `.xaml`    | 界面定义，XML 格式      |
| `.xaml.cs` | 代码后置，C# 逻辑       |
| `App.xaml` | 应用程序入口，全局资源定义处   |

**应用程序启动流程：**
```
App.xaml (StartupUri="MainWindow.xaml")
    → App 类构造
        → MainWindow 构造
            → InitializeComponent() 加载 MainWindow.xaml
                → 窗口显示
```

---

## 2. XAML 语法

### 2.1 基本规则

XAML 是 XML 的方言。每个标签对应一个 .NET 类。
### 2.2 命名空间

```xml
<Window x:Class="WpfApp1.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"   ← WPF 核心控件
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"               ← x: 前缀功能
        xmlns:local="clr-namespace:WpfApp1">                                 ← 引用自己的 C# 类
```

### 2.3 x:Name — 给控件起名

```xml
<TextBlock x:Name="TxtGreeting" Text="你好"/>
<!-- 现在 C# 代码里可以直接写 TxtGreeting.Text = "xxx" -->
```

### 2.4 属性值类型

```xml
<!-- 字符串 -->
<Button Content="按钮"/>

<!-- 数字 -->
<Button Width="120" Height="40"/>

<!-- 枚举 -->
<StackPanel Orientation="Horizontal"/>

<!-- 颜色 -->
<Border Background="#FF0000"/>        <!-- 十六进制 -->
<Border Background="Red"/>            <!-- 命名颜色 -->
<Border Background="#CCFF0000"/>      <!-- 带透明度的 ARGB -->

<!-- 复杂属性（元素语法） -->
<Button>
    <Button.Background>
        <SolidColorBrush Color="Blue"/>
    </Button.Background>
</Button>
```

### 2.5 标记扩展 (Markup Extension)

用花括号 `{}` 表示的特殊语法，最常见的是 Binding 和 StaticResource。

```xml
<!-- Binding -->
<TextBlock Text="{Binding UserName}"/>

<!-- StaticResource -->
<Button Style="{StaticResource DangerButton}"/>

<!-- x:Null -->
<Button Background="{x:Null}"/>
```

---

## 3. 代码后置 (Code-Behind)

每个 `.xaml` 文件都有一个对应的 `.xaml.cs` 文件，由 `partial class` 连接。

```xml
<!-- MainWindow.xaml -->
<Window x:Class="WpfApp1.MainWindow" ...>
    <Button x:Name="BtnGreet" Content="打招呼" Click="BtnGreet_Click"/>
</Window>
```

```csharp
// MainWindow.xaml.cs
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();  // ← 加载 XAML，创建所有控件
    }

    private void BtnGreet_Click(object sender, RoutedEventArgs e)
    {
        // sender = 被点击的按钮（即 BtnGreet）
        // e = 事件携带的数据
    }
}
```

**sender 的用法：**

```csharp
// 多个按钮共用一个事件处理器，通过 sender 区分
private void BtnDigit_Click(object sender, RoutedEventArgs e)
{
    Button btn = sender as Button;  // 拿到被点的按钮
    string digit = btn.Content.ToString();  // 拿到按钮上的文字
}
```

---

## 4. 布局面板

### 4.1 总览

| 面板 | 排列方式 | 适用场景 |
|------|----------|----------|
| Grid | 表格（行×列） | 最常用，几乎所有界面骨架 |
| StackPanel | 堆叠（横/竖） | 局部排列，简单列表 |
| DockPanel | 停靠（上下左右） | 窗口框架（菜单栏+状态栏+内容区） |
| WrapPanel | 自动换行 | 标签云、照片墙 |

### 4.2 Grid（最常用）

```xml
<Grid>
    <!-- 定义 2 行 -->
    <Grid.RowDefinitions>
        <RowDefinition Height="60"/>     <!-- 固定 60px -->
        <RowDefinition Height="*"/>      <!-- 占满剩余空间 -->
        <RowDefinition Height="2*"/>     <!-- 占 2 份比例 -->
        <RowDefinition Height="Auto"/>   <!-- 根据内容自动调整 -->
    </Grid.RowDefinitions>

    <!-- 定义 3 列 -->
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <!-- 放控件到指定格子 -->
    <Button Grid.Row="0" Grid.Column="1" Content="我在第0行第1列"/>

    <!-- 跨行/跨列 -->
    <Button Grid.Row="0" Grid.RowSpan="2"           ← 跨 2 行
            Grid.Column="2" Grid.ColumnSpan="2"     ← 跨 2 列
            Content="大按钮"/>
</Grid>
```

### 4.3 StackPanel

```xml
<!-- 竖向（默认） -->
<StackPanel Orientation="Vertical">
    <Button Content="A"/>
    <Button Content="B"/>
    <Button Content="C"/>
</StackPanel>

<!-- 横向 -->
<StackPanel Orientation="Horizontal">
    <Button Content="1"/>
    <Button Content="2"/>
</StackPanel>
```

### 4.4 DockPanel

```xml
<DockPanel LastChildFill="True">     <!-- 最后一个填满剩余空间 -->
    <Border DockPanel.Dock="Top" Height="50" Background="Blue"/>
    <Border DockPanel.Dock="Bottom" Height="30" Background="Orange"/>
    <Border DockPanel.Dock="Left" Width="200" Background="Green"/>
    <!-- 最后一个不加 Dock，自动填满 -->
    <Border Background="White"/>
</DockPanel>
```

### 4.5 WrapPanel

```xml
<WrapPanel>
    <Button Content="标签A" Width="80" Margin="3"/>
    <Button Content="标签B" Width="80" Margin="3"/>
    <!-- 排不下自动换行 -->
</WrapPanel>
```

### 4.6 常用布局属性

```xml
<Button HorizontalAlignment="Center"    <!-- 水平对齐：Left/Center/Right/Stretch -->
        VerticalAlignment="Center"      <!-- 垂直对齐：Top/Center/Bottom/Stretch -->
        Margin="10,5,10,5"             <!-- 外边距：左上右下 -->
        Padding="8,4"                   <!-- 内边距 -->
        Width="120" Height="40"/>       <!-- 尺寸（不设则由内容决定） -->
```

---

## 5. 常用控件速查

### 5.1 Button

```xml
<Button Content="按钮文字" Width="100" Height="36"
        Background="#2B579A" Foreground="White"
        Click="Button_Click"/>
```

### 5.2 TextBlock（只读文字）

```xml
<TextBlock Text="一段文字" FontSize="16" FontWeight="Bold" Foreground="#333"/>

<!-- 多格式文字 -->
<TextBlock>
    <Run Text="普通文字"/>
    <Run Text="加粗" FontWeight="Bold"/>
    <Run Text="蓝色" Foreground="Blue"/>
</TextBlock>
```

### 5.3 TextBox（输入框）

```xml
<TextBox Width="200" Height="28"
         Text="默认文字"
         VerticalContentAlignment="Center"/>  <!-- 文字垂直居中 -->
```

### 5.4 CheckBox

```xml
<CheckBox Content="我同意" IsChecked="True"
          Checked="CheckBox_Checked"
          Unchecked="CheckBox_Unchecked"/>
```

```csharp
private void CheckBox_Checked(object sender, RoutedEventArgs e)
{
    bool isChecked = (sender as CheckBox).IsChecked == true;
}
```

### 5.5 RadioButton（同一父容器内自动互斥）

```xml
<StackPanel>
    <RadioButton Content="选项A" IsChecked="True" Checked="Radio_Checked"/>
    <RadioButton Content="选项B" Checked="Radio_Checked"/>
    <RadioButton Content="选项C" Checked="Radio_Checked"/>
</StackPanel>
```

### 5.6 ComboBox（下拉选择）

```xml
<ComboBox SelectedIndex="0" SelectionChanged="Combo_Changed">
    <ComboBoxItem Content="选项一"/>
    <ComboBoxItem Content="选项二"/>
</ComboBox>
```

### 5.7 ListBox（列表选择）

```xml
<ListBox SelectionChanged="List_Changed">
    <ListBoxItem Content="项目A"/>
    <ListBoxItem Content="项目B"/>
</ListBox>
```

### 5.8 Slider

```xml
<Slider Minimum="0" Maximum="100" Value="50"
        ValueChanged="Slider_Changed"/>
```

### 5.9 ProgressBar

```xml
<ProgressBar Minimum="0" Maximum="100" Value="75" Height="8"/>
```

### 5.10 Image

```xml
<Image Source="C:\path\to\image.png" Width="200"/>
```

---

## 6. 事件系统

### 6.1 事件绑定

```xml
<!-- XAML 里声明事件名 -->
<Button Click="Button_Click"/>
<Border MouseEnter="Border_MouseEnter"/>
<TextBox KeyDown="TextBox_KeyDown"/>
```

```csharp
// C# 里实现处理方法
private void Button_Click(object sender, RoutedEventArgs e) { }
private void Border_MouseEnter(object sender, MouseEventArgs e) { }
private void TextBox_KeyDown(object sender, KeyEventArgs e) { }
```

### 6.2 常用事件列表

| 事件 | 适用控件 | 触发时机 | 事件参数类型 |
|------|----------|----------|-------------|
| `Click` | Button 等 | 点击 | `RoutedEventArgs` |
| `Checked/Unchecked` | CheckBox, RadioButton | 勾选/取消 | `RoutedEventArgs` |
| `SelectionChanged` | ComboBox, ListBox | 选项变化 | `SelectionChangedEventArgs` |
| `ValueChanged` | Slider | 值变化 | `RoutedPropertyChangedEventArgs<double>` |
| `MouseEnter` | 任意控件 | 鼠标进入 | `MouseEventArgs` |
| `MouseLeave` | 任意控件 | 鼠标离开 | `MouseEventArgs` |
| `MouseMove` | 任意控件 | 鼠标移动 | `MouseEventArgs` |
| `KeyDown` | 可聚焦控件 | 按下键盘 | `KeyEventArgs` |

### 6.3 路由事件 (Routed Event)

WPF 事件的最大特点——事件会沿控件树"冒泡"。

```
用户点击一个 Button
    ↓
事件从 Button 开始
    ↓ 向上冒泡
Button 的父容器（如 StackPanel）
    ↓ 继续冒泡
StackPanel 的父容器（如 Grid）
    ↓ 继续冒泡
Grid 的父容器（如 Window）
```

```xml
<!-- 外层容器监听子控件的 Click 事件 -->
<Border Button.Click="Bubble_Click">
    <StackPanel>
        <Border Button.Click="Bubble_Click">
            <Button Content="点我"/>   ← 点击这里，外层 和 中间层 都会触发！
        </Border>
    </StackPanel>
</Border>
```

```csharp
private void Bubble_Click(object sender, RoutedEventArgs e)
{
    // sender       = 当前处理事件的这一层（外层容器）
    // e.OriginalSource = 最初触发事件的控件（按钮本身）
}
```

### 6.4 初始化事件问题

XAML 中设置的属性（`IsChecked="True"`、`SelectedIndex="0"`、`Value="60"`）会在页面初始化时触发对应事件。如果事件处理器中访问了还未就绪的控件会抛出 NullReferenceException。

```csharp
// 解决方案：加 IsLoaded 守卫
private void CheckBox_Changed(object sender, RoutedEventArgs e)
{
    if (!IsLoaded) return;   // 页面未加载完毕不处理
    // ... 正常处理逻辑
}
```

---

## 7. 数据绑定

### 7.1 为什么需要 Binding？

没有 Binding：
```csharp
// 每次数据变了都要手动更新 UI，容易漏
Slider_ValueChanged(...) { TxtDisplay.Text = Slider.Value.ToString(); }
```

有 Binding：
```xml
<!-- 声明式绑定，自动同步 -->
<TextBlock Text="{Binding ElementName=Slider, Path=Value}"/>
```

### 7.2 三种绑定方式

#### 方式 1：控件间绑定 (ElementName)

```xml
<Slider x:Name="SldFont" Value="20"/>
<TextBlock Text="{Binding ElementName=SldFont, Path=Value,
                           StringFormat='字号: {0:F0}'}"/>
<TextBlock FontSize="{Binding ElementName=SldFont, Path=Value}"/>
```

语法：`{Binding ElementName=源控件名, Path=源属性名}`

#### 方式 2：DataContext 绑定

```xml
<!-- XAML -->
<TextBlock Text="{Binding UserName}"/>
<!-- 没指定 ElementName → 自动从 DataContext 找 UserName -->
```

```csharp
// C# 构造函数中设置 DataContext
this.DataContext = new PersonViewModel();
```

DataContext 的特性：**自动向下传递**。给 Window 设置了 DataContext，里面所有控件都共享。

#### 方式 3：ItemsSource 集合绑定

```xml
<ListBox ItemsSource="{Binding Contacts}"        ← 绑定到集合
         SelectedItem="{Binding SelectedContact}" ← 绑定当前选中项
         ItemTemplate="{StaticResource ContactTemplate}"/>
```

### 7.3 Binding 常用参数

```xml
<TextBox Text="{Binding UserName,
                Mode=TwoWay,                          ← 双向绑定（默认：TextBox 是 TwoWay，TextBlock 是 OneWay）
                UpdateSourceTrigger=PropertyChanged,  ← 每输入一个字符就更新数据源
                StringFormat='姓名: {0}',             ← 格式化
                FallbackValue='默认值',                ← 绑定失败时的备用值
                TargetNullValue='(空)'}"/>            ← 绑定值为 null 时的显示
```

### 7.4 DataContext 的查找链

```
控件自身.DataContext
    ↓ 如果为 null
父控件.DataContext
    ↓ 如果为 null
祖父控件.DataContext
    ↓ ...一直向上
Window.DataContext
```

这意味着你只需要在最外层设置一次 DataContext，所有子控件自动继承。

---

## 8. INotifyPropertyChanged

### 8.1 为什么需要它？

普通类的属性改变时，UI 不知道。实现了这个接口后，属性变 → 通知 UI → UI 自动刷新。

### 8.2 标准实现

```csharp
public class PersonViewModel : INotifyPropertyChanged
{
    // 1. 声明事件
    public event PropertyChangedEventHandler PropertyChanged;

    // 2. 属性 setter 中调用通知方法
    private string _userName;
    public string UserName
    {
        get { return _userName; }
        set
        {
            _userName = value;
            OnPropertyChanged();  // ← 通知 UI 刷新
        }
    }

    // 3. 通知方法
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### 8.3 [CallerMemberName] 的作用

```csharp
// 调用 OnPropertyChanged() 时，编译器自动把调用者的名字填进去
// 在 UserName 的 setter 里调用 → propertyName = "UserName"
// 在 UserAge 的 setter 里调用  → propertyName = "UserAge"
// 不用手写字符串，不会拼错
```

### 8.4 通知其他属性

当一个属性变了，可能导致另一个只读属性也变了：

```csharp
public Contact SelectedContact
{
    set
    {
        _selectedContact = value;
        OnPropertyChanged();                // 通知 SelectedContact 变了
        OnPropertyChanged("HasNoSelection"); // 通知 HasNoSelection 也变了
    }
}

public bool HasNoSelection
{
    get { return SelectedContact == null; }  // 依赖 SelectedContact
}
```

---

## 9. ObservableCollection 与列表绑定

### 9.1 List vs ObservableCollection

```csharp
// ❌ 用 List：Add 之后 UI 不会刷新
List<Contact> contacts = new List<Contact>();
contacts.Add(new Contact());  // UI 不变！

// ✅ 用 ObservableCollection：Add/Remove 后 UI 自动刷新
ObservableCollection<Contact> contacts = new ObservableCollection<Contact>();
contacts.Add(new Contact());  // UI 自动多一行
contacts.Remove(someContact); // UI 自动少一行
```

### 9.2 ItemsSource — 列表控件的数据源

```xml
<ListBox ItemsSource="{Binding Contacts}"           ← 数据来自 ViewModel 的 Contacts 集合
         SelectedItem="{Binding SelectedContact}"   ← 选中哪一项
         DisplayMemberPath="Name"/>                ← 简单场景：只显示 Name 属性
```

### 9.3 主从模式 (Master-Detail)

```xml
<!-- 左边：列表 -->
<ListBox ItemsSource="{Binding Contacts}"
         SelectedItem="{Binding SelectedContact}"/>

<!-- 右边：详情（自动显示选中项的信息） -->
<StackPanel>
    <TextBlock Text="{Binding SelectedContact.Name}"/>
    <TextBlock Text="{Binding SelectedContact.Email}"/>
    <TextBlock Text="{Binding SelectedContact.Role}"/>
</StackPanel>
```

用户在列表中点不同的人，右边详情自动切换——不需要任何 C# 事件处理代码。

---

## 10. DataTemplate 数据模板

### 10.1 作用

定义集合中**每一项**的显示外观。不用 DataTemplate 时，ListBox 只显示对象的 ToString() 结果。

### 10.2 基本用法

```xml
<Page.Resources>
    <DataTemplate x:Key="ContactTemplate">
        <Border Padding="12" Background="White">
            <StackPanel>
                <TextBlock Text="{Binding Name}" FontSize="16" FontWeight="Bold"/>
                <TextBlock Text="{Binding Role}" FontSize="13" Foreground="Gray"/>
            </StackPanel>
        </Border>
    </DataTemplate>
</Page.Resources>

<ListBox ItemsSource="{Binding Contacts}"
         ItemTemplate="{StaticResource ContactTemplate}"/>
```

### 10.3 DataTemplate 中的 Binding

模板里的 `{Binding Name}` 绑定的是**集合元素**的属性，不是 DataContext 的属性。

```
DataContext = ViewModel (有 Contacts, SelectedContact)
    ↓ ItemsSource 绑定
Contacts[i] = Contact 对象
    ↓ ItemTemplate 中的 Binding
{Binding Name} = Contact.Name
{Binding Role} = Contact.Role
```

---

## 11. 样式 (Style)

### 11.1 Style = 属性设置打包

```xml
<!-- 定义 Style -->
<Style x:Key="MyButtonStyle" TargetType="Button">
    <Setter Property="Background" Value="#E74C3C"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="FontSize" Value="16"/>
    <Setter Property="Width" Value="140"/>
    <Setter Property="Height" Value="40"/>
    <Setter Property="Cursor" Value="Hand"/>
</Style>

<!-- 使用 Style -->
<Button Content="删除" Style="{StaticResource MyButtonStyle}"/>
<Button Content="清空" Style="{StaticResource MyButtonStyle}"/>
```

### 11.2 显式 vs 隐式样式

```xml
<!-- 显式：有 x:Key，需要手动引用 -->
<Style x:Key="RedButton" TargetType="Button">...</Style>
<Button Style="{StaticResource RedButton}"/>

<!-- 隐式：没有 x:Key，自动作用于所有同类型控件 -->
<Style TargetType="Button">
    <Setter Property="FontSize" Value="14"/>
</Style>
<!-- 所有 Button 字号自动变成 14 -->
```

### 11.3 样式继承 (BasedOn)

```xml
<Style x:Key="BaseBtn" TargetType="Button">
    <Setter Property="Width" Value="100"/>
</Style>

<Style x:Key="BigBtn" TargetType="Button" BasedOn="{StaticResource BaseBtn}">
    <Setter Property="Height" Value="50"/>  <!-- 继承了 Width=100，额外加了 Height -->
</Style>
```

### 11.4 Style 中可以设置任何属性

包括事件处理器：

```xml
<Style TargetType="Button">
    <EventSetter Event="Click" Handler="CommonButton_Click"/>
</Style>
```

---

## 12. 控件模板 (ControlTemplate)

### 12.1 作用

Style 改的是属性值（颜色、大小等），ControlTemplate **完全重画控件的外观结构**。

### 12.2 基本结构

```xml
<Style x:Key="RoundButton" TargetType="Button">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <!-- 这里完全自定义外观 -->
                <Border Background="{TemplateBinding Background}"
                        CornerRadius="8"
                        BorderBrush="{TemplateBinding BorderBrush}">
                    <Border.Effect>
                        <DropShadowEffect BlurRadius="6" ShadowDepth="2" Opacity="0.3"/>
                    </Border.Effect>
                    <!-- 必须放 ContentPresenter 才能显示按钮文字 -->
                    <ContentPresenter HorizontalAlignment="Center"
                                      VerticalAlignment="Center"/>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

### 12.3 TemplateBinding vs Binding

```xml
<!-- TemplateBinding：绑定到「使用这个模板的控件」自身的属性 -->
<Border Background="{TemplateBinding Background}"/>
<!-- 意思：边框的背景色 = 按钮的 Background 属性值 -->

<!-- 普通 Binding：绑定到 DataContext -->
<TextBlock Text="{Binding UserName}"/>
```

### 12.4 ContentPresenter 不可省略

```xml
<ControlTemplate TargetType="Button">
    <Border>
        <!-- 没有 ContentPresenter → 按钮上的文字不会显示！ -->
        <ContentPresenter/>   ← 必须放在模板里
    </Border>
</ControlTemplate>
```

---

## 13. 触发器 (Trigger)

### 13.1 属性触发器

当某个属性满足条件时，自动改变其他属性。

```xml
<Style TargetType="TextBlock">
    <Setter Property="Foreground" Value="Gray"/>
    <Setter Property="FontSize" Value="18"/>
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Foreground" Value="Red"/>
            <Setter Property="FontSize" Value="26"/>
            <Setter Property="FontWeight" Value="Bold"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

### 13.2 常用触发条件

| Property | 说明 |
|----------|------|
| `IsMouseOver` | 鼠标悬停 |
| `IsPressed` | 被按下 |
| `IsFocused` | 获得焦点 |
| `IsEnabled` | 可用状态 |
| `IsChecked` | CheckBox/RadioButton 勾选状态 |

### 13.3 ControlTemplate 中的 Trigger

```xml
<ControlTemplate TargetType="Button">
    <Border x:Name="border" Background="Blue">
        <ContentPresenter/>
    </Border>
    <ControlTemplate.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter TargetName="border" Property="Background" Value="DarkBlue"/>
        </Trigger>
    </ControlTemplate.Triggers>
</ControlTemplate>
```

注意：ControlTemplate.Triggers 中用 `TargetName` 指向模板内部的元素。

---

## 14. 资源 (Resources)

### 14.1 资源层级（就近原则）

```
App.xaml → Application.Resources      ← 全局可用
    ↓
Window/Page → Resources               ← 当前窗口/页面可用
    ↓
控件 → Resources                      ← 该控件及子控件可用
```

查找顺序：**从内到外**。子控件的 Resources → 父控件的 Resources → Window.Resources → Application.Resources。

### 14.2 定义位置

```xml
<!-- App.xaml：全局 -->
<Application.Resources>
    <Style x:Key="GlobalStyle" TargetType="Button">...</Style>
</Application.Resources>

<!-- Page/Window：页面级 -->
<Page.Resources>
    <Style x:Key="PageStyle" TargetType="Button">...</Style>
</Page.Resources>

<!-- 控件：控件级 -->
<Button>
    <Button.Resources>
        <Style TargetType="TextBlock">...</Style>
    </Button.Resources>
</Button>
```

### 14.3 资源类型

Resources 可以放任何对象：

```xml
<Page.Resources>
    <!-- 样式 -->
    <Style x:Key="..." TargetType="...">...</Style>

    <!-- 数据模板 -->
    <DataTemplate x:Key="...">...</DataTemplate>

    <!-- 转换器 -->
    <local:BoolToVisibilityConverter x:Key="BoolToVisibility"/>

    <!-- 普通对象 -->
    <SolidColorBrush x:Key="MyBlue" Color="#2B579A"/>

    <!-- 字符串 -->
    <sys:String x:Key="AppTitle">我的应用</sys:String>
</Page.Resources>
```

---

## 15. 值转换器 (IValueConverter)

### 15.1 为什么需要？

某些属性类型不兼容，不能直接绑定。比如 `Visibility` 是枚举，不能用 `bool` 直接赋值。

### 15.2 实现一个转换器

```csharp
public class BoolToVisibilityConverter : IValueConverter
{
    // 数据 → UI 方向：bool → Visibility
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue = (value is bool) && (bool)value;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    // UI → 数据方向：Visibility → bool（双向绑定时需要）
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is Visibility) && ((Visibility)value == Visibility.Visible);
    }
}
```

### 15.3 注册和使用

```xml
<!-- App.xaml 或 Page.Resources 中注册 -->
<local:BoolToVisibilityConverter x:Key="BoolToVisibility"/>

<!-- 在 Binding 中使用 -->
<TextBlock Visibility="{Binding HasNoSelection,
                          Converter={StaticResource BoolToVisibility}}"/>
```

---

## 16. 附加属性

### 16.1 概念

属性定义在**另一个类**身上，但写在**当前控件**的标签里。

```xml
<!-- Grid.Row 是 Grid 类定义的属性，但写在 Button 上 -->
<Button Grid.Row="0" Grid.Column="1"/>

<!-- DockPanel.Dock 是 DockPanel 类定义的属性，写在 Border 上 -->
<Border DockPanel.Dock="Top"/>

<!-- Canvas.Left 是 Canvas 类定义的属性，写在 Rectangle 上 -->
<Rectangle Canvas.Left="50" Canvas.Top="100"/>
```

### 16.2 原理

父容器通过附加属性读取子控件的位置信息：

```
Grid 在布局时：
  遍历所有子控件
  → 读 Grid.Row 附加属性的值
  → 决定这个控件放哪个格子
```

---

## 17. MVVM 模式入门

### 17.1 三层结构

```
Model          ViewModel          View
(数据)         (中介)             (界面)

Contact        ContactListVM      Page/Lesson08
  .Name          .Contacts ←────────→ ListBox.ItemsSource
  .Age           .SelectedContact ←→ ListBox.SelectedItem
  .Role          .HasNoSelection ──→ TextBlock.Visibility
```

### 17.2 各层职责

| 层 | 职责 | 不能做什么 | 例子 |
|----|------|-----------|------|
| **Model** | 纯数据 | 不包含 UI 逻辑 | `Contact { Name, Age, Role }` |
| **ViewModel** | 持有数据+状态，通知 UI | 不直接操作控件 | `ContactListViewModel { Contacts, SelectedContact }` |
| **View** | 界面展示 | 不包含业务逻辑 | XAML + 最少 C# |

### 17.3 数据流

```
用户操作控件（View）
    ↓ Binding 自动同步
ViewModel 属性更新
    ↓ INotifyPropertyChanged 通知
View 自动刷新
```

### 17.4 设置 DataContext 的几种方式

```csharp
// 方式 1：代码中设置（本教程采用）
public Lesson04_Binding()
{
    InitializeComponent();
    this.DataContext = new PersonViewModel();
}

// 方式 2：XAML 中设置
<Page.DataContext>
    <local:PersonViewModel/>
</Page.DataContext>

// 方式 3：通过 Window 传递
MainWindow.DataContext = new MainViewModel();
// 所有 Page 自动继承
```

---

## 18. 常见问题与调试

### 18.1 绑定不生效

**症状**：TextBlock 一片空白，明明绑定了。

**排查步骤**：
1. 检查 DataContext 是否设置了（没有设就是 null）
2. 检查属性名拼写（区分大小写！`UserName` ≠ `username`）
3. 检查属性的 getter 是否 public
4. 看 Visual Studio 的 Output 窗口 → 有绑定错误日志

```
// Output 窗口典型错误：
System.Windows.Data Error: 40 : BindingExpression path error:
'UserNam' property not found on 'object' 'PersonViewModel'
```

### 18.2 列表添加元素后不刷新

**原因**：用了 `List<T>` 而不是 `ObservableCollection<T>`。

**修复**：换成 `ObservableCollection<T>`。

### 18.3 页面初始化时空引用

**原因**：XAML 中设置的 `IsChecked="True"` 等属性在 InitializeComponent 阶段触发事件，此时部分控件还未就绪。

**修复**：事件处理器开头加 `if (!IsLoaded) return;`

### 18.4 ContentTemplate 不显示内容

**原因**：ControlTemplate 中忘了放 `ContentPresenter`。

**修复**：在 ControlTemplate 里加上 `<ContentPresenter/>`。

### 18.5 样式/资源找不到

**原因**：资源定义在子控件的 Resources 里，但父控件在引用它。

**修复**：把资源定义提升到更外层（Page.Resources 或 Application.Resources）。

### 18.6 控件被裁剪

**原因**：外层容器尺寸不够。

**修复**：
- 用 `ScrollViewer` 包裹以获得滚动条
- 设置 `HorizontalAlignment="Stretch"` 或移除固定 Width
- 用 `*` 比例代替固定像素

---

## 附录：本教程文件索引

| 课时 | 文件 | 核心知识点 |
|------|------|-----------|
| 1 | `Lesson01_Hello` | x:Name, TextBlock, TextBox, Button, Click, code-behind |
| 2 | `Lesson02_Layout` | StackPanel, Grid, DockPanel, WrapPanel, 附加属性 |
| 3 | `Lesson03_Controls` | CheckBox, RadioButton, ComboBox, ListBox, Slider, ProgressBar |
| 4 | `Lesson04_Binding` | Binding(ElementName), DataContext, INotifyPropertyChanged |
| 5 | `Lesson05_Styles` | Style, Setter, ControlTemplate, Trigger, Resources |
| 6 | `Lesson06_Events` | Click, MouseEnter/Leave/Move, KeyDown, 路由事件冒泡 |
| 7 | `Lesson07_Calculator` | 综合：Grid + Style + Click + 状态管理 |
| 8 | `Lesson08_ListBinding` | ObservableCollection, ItemsSource, DataTemplate, 主从模式, IValueConverter |
