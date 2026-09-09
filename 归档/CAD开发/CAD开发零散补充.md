---
date: 2026-06-13
tags:
  - CAD二次开发
status: draft
---

> 从每天零散记录中归集到 CAD 开发的零散知识点。各条目保留原始日期，方便回溯。

---

## 2026.4.15 — 下料、predicate、立即查询

下料 = 按图纸把原材料切分成零件毛坯。
1. **套料（Nesting）**：把各种形状的板件像"拼图"一样摆在一张标准矩形大钢板上，尽量减少边角料浪费。
2. **编程**：把 CAD 路径转化为数控切割机（等离子或激光）能识别的代码。
3. **切割**：机器沿着多段线走一遍，钢板变成想要的零件。

把 predicate 直接记成：**"筛选条件"**、**"判断函数"**、**"返回真假值的 lambda"**

立即执行查询 vs 延迟查询——如果后面会对实体进行删除，延迟查询更危险（事务关闭时才出问题）。

## 2026.4.17 — 程序设计思路、面域创建

程序设计的两种设计方式：
1. 大纲思路下的每一步实现
2. 具体实现功能的拼接

后一步骤的输入与前一步的输出之间的联系，要求不要太高——后一步拿到输入后，不需要考虑额外的东西，是上一步都处理好了的。

`Debug.Assert(x != null);`

**创建面域**：对于当前场景只能创建出来一个。实体层常用 `Entity.IntersectWith`，几何层的一般曲线求交常用 `CurveCurveIntersector2d`。

面域曲线要求：不能是被加入到数据库中且被打开为写的曲线。闭合曲线对于新加入的没这个要求。

grip point 是实体通过 `GetGripPoints` / `subGetGripPoints` 主动提供给 AutoCAD 的。

## 2026.4.20 — 视图、视口、包围盒（块部分已并入 块.md）

**View**：视图指图形窗口显示的内容。使用 Editor 的 `GetCurrentView` 获得当前视图，用 `SetCurrentView` 更新视图。可以设置视图的中心点和宽、高。用边界框控制视图时必须考虑避免退化边界。

**包围盒**：`GeometricExtents` 属性返回 `Extents3d` 结构，获得 WCS 坐标系下包围盒的相关参数。`db.UpdateExt` 函数遍历模型空间所有模型得到 `EXTMIN`/`EXTMAX`（图形范围外包矩形的角点）。`Limmin/Limmax` 是给当前空间设的理论绘图边界。

**Viewport**：

```
`ViewportTableRecord` 和 `Viewport` 的关键区别是 `TileMode` 的值。`TileMode==1` 是 `ViewportTableRecord`（平铺视口配置），`TileMode==0` 是 `Viewport`（纸空间浮动视口实体）。与其他符号表记录不同的是，ViewportTable 可以容纳多个同名的符号表记录（因为配置不同）。
```


## 2026.4.22 — 曲线偏移 GetOffsetCurves（继承部分已并入 Csharp/类和继承.md）

`GetOffsetCurves` 函数：Creates concentric circles, parallel lines, and parallel curves。生成与原曲线保持指定偏移距离的结果对象。

多段线的偏移：如果对一个梯形的多段线进行偏移操作，为了保证等距，偏移后梯形的各个边不可避免发生比例变化，甚至会发生一条边退化和相邻两边发生自交的情况。CAD 内核会在多段线偏移时各分段分别偏移，进而导致线之间产生相交，需要进行 trim 修剪、补缝的操作，形成新边界。

## 2026.4.23 — 面域布尔运算

面域的布尔运算调用成功后，需要自己去验证结果。不要把调用成功当成几何成立——比如交集就要判断是否真的有交集，会不会是 null。

正式计算和试探判断要断开。布尔运算会改变原对象，在不希望改变源对象的情况下使用 clone。

## 2026.4.24 — BlockTableRecord 构造、C#封装继承不一致（符号表部分已并入 文档和图形数据库.md）

**BlockTableRecord 构造函数**：无参构造 `new BlockTableRecord()` 不是单纯创建 C# 对象——它会通过 `acHeapAlloc`（CAD 自己的 C++ 堆内存分配函数）申请 24 字节内存并调用底层 C++ 构造函数。内部构造函数 `internal BlockTableRecord(IntPtr unmanagedPointer, bool autoDelete)` 则是把已存在的底层对象指针包装成 C# 对象。

**C# 封装与 C++ 继承关系不一致**：AutoCAD .NET API 是对 ObjectARX 的托管封装，不是对 ObjectARX 继承体系的机械复制。C++ 中 `AcDbViewTable` 继承 `AcDbAbstractViewTable`，但 C# 封装中 `ViewTable` 直接继承 `SymbolTable`。写 C# 时以 C# 类声明为准。

## 2026.4.25 — DBText 对齐设置

默认情况下 `DBText` 的 `Position` 是文字的**插入基点**（默认左对齐）。设置对齐：
```csharp
Text.HorizontalMode = TextHorizontalMode.TextCenter;
Text.VerticalMode = TextVerticalMode.TextVerticalMid;
Text.AlignmentPoint = center;
```
设置 AlignmentPoint 之后 Position 不需要再设置。

如果 DBText 不是独立数据库实体而被别的实体当内部对象使用，可能不会经历正常 close 流程，可用 `AdjustAlignment(db)` 手动提前触发对齐调整。

## 2026.4.21 — 边界描述法 B-Rep、AABB、NURBS 概念

边界描述法（B-Rep boundary representation）的核心思想：不是用内部填了什么来描述一个形体，而是用它的边界是什么来描述。

怎么用 B-Rep 构建一个面域？
先准备边界线（必须共面、组成闭合环、端点连接、不能有明显的缝隙、重叠、乱交叉等），接着确定拓扑关系（哪条边连着哪条边、边的方向、组成了几个闭合环、区分内环和外环），最后交给内核生成面域。

AABB（Axis-Aligned Bounding Box）轴对齐包围盒，平行于坐标轴，是一种粗筛选。八叉树可以理解成：把三维空间不断切成 8 块的一种空间划分结构。

样条曲线不是只靠一个简单公式整段描述，而是通过控制点、节点等信息定义出来的一类平滑曲线。NURBS = Non-Uniform Rational B-Spline：节点向量不等间隔（参数轴上节点不要求平均分布），有理指每个控制点可以带权重。

> 样条曲线的详细内容见 [[样条曲线 NURBS 详解]]

---

## 闪卡

#cad开发-flashcards

offset输入参数的正负值 是不是代表着方向？分两个API考虑
?
- If the offsetDist value is negative, it is usually interpreted as being an offset to make a smaller curve
- 如果是圆弧，就是偏移后得圆弧半径比之前的圆弧半径要小。
- If the negative value has no meaning in terms of making the curve smaller, a negative offsetDist may be interpreted as an offset in the direction of smaller _X,Y,Z_ WCS coordinates.This is not enforced, so custom entities can interpret the sign of the offsetDist value however they want.
- 对于其他没有变得跟小这个意义的曲线来说，偏移方向是随机的。
- **The positive direction of offset** at each point of the base curve is the **cross product** of the specified normal vector with the tangent vector at that point.对于ge级别的curveoffset，偏移正方向如上
<!--SR:!2027-01-12,146,252-->

PromptSelectResult 初始化与使用逻辑：
状态初始化：构造函数优先调用 commoninit 函数。该函数通过 switch-case 解析传入的整数参数 retcode。当 retcode 为 5100 时，成员变量 m_status 被赋值为枚举类型 ==1;;PromptStatus.OK==。
属性赋值：只有在 retcode 为 5100 的前提下，程序才会==1;;通过实例化 SelectionSet 抽象类的子类对象，为 value 属性赋具体值==。
核心结论：只有当 PromptStatus 的状态为 ==1;;OK== 时，PromptSelectResult 的 value 属性才会==1;;持有有效值==。
这是API强制要求的写法，不然直接引用value值可能会引发空引用错误。
<!--SR:!2026-12-11,141,270-->

---

## 收件箱归集

- [2026.5.26] CAD 从 2013 后有命令行版本，可以省去加载图形界面的时间
- [2026.5.18] `SPE`（SPline Edit）编辑样条；`Revcloud`（Revision Cloud）云线，圈出修改/新增区域
- [2026.5.15~17] `CurveCurveIntersector3d` 返回的参数值取决于参与求交的具体曲线对象类型和内部参数化方式
- [2026.5.26] `BasePoint` 是交互过程中的参考基准点，当 UseBasePoint 设置为 true 时，CAD 从基准点到鼠标光标之间拉出一根"橡皮筋线"
- 射线（Ray）没有 `EndPoint`，读取会抛异常
- Document、DataBase 都有 TransactionManager，前者用于当前正在操作的 CAD 文档，后者用于操作后台打开（未在编辑器显示）的 DWG 文件，不涉及 UI 和文档锁定
- 尺寸标注：CAD 基本标注类型包括线性、径向、角度、坐标、弧长。抽象基类 `Dimension` 管标注文字、标注样式等共有属性。一般调派生类的默认构造函数后设属性（`ArcDimension` 除外，无默认构造函数）。尺寸公差没有专门类，通过文本堆叠处理
- 工字钢的 **R 角**（内弧半径）是指其腹板与翼缘连接处的圆弧半径，设计目的是减少应力集中。加劲板的角绝对不能是直角——如果是 90 度直角，加劲板就顶在工字钢的 R 角上，贴不进去，也没法焊接
- AutoLISP、.NET 插件和 ObjectARX 的区别：

| 类型 | 文件 | 语言 | 加载方式 | 定位 |
| --- | --- | --- | --- | --- |
| AutoLISP | `.lsp` / `.fas` / `.vlx` | Lisp | `APPLOAD` | 脚本小工具 |
| .NET 插件 | `.dll` | C# / VB.NET | `NETLOAD` | 现代 CAD 插件开发 |
| ObjectARX / ADSRX | `.arx` / `.dbx` | C++ | `APPLOAD` / 自动加载 | 底层高性能插件 |
- [2026.7.3] 画圆弧时起点和终点对换，画出来两条圆弧——形状一样，只是一条逆时针画、一条顺时针画。但 API 读到的 `StartPoint` 和 `EndPoint` 是一样的，分不出方向。**原因**：Arc 内部永远存成从 `startAngle` 到 `endAngle` 逆时针，不保留绘制方向。**影响**：用 `OffsetCurve3d` 做偏移时，偏移方向由法向量与切向的叉积决定，而切向跟着参数化方向走——不管顺时针还是逆时针画，存入后都是逆时针，切向一样，偏移结果也一样
