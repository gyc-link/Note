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

## 2026.7 — equalVector 与几何判定

### equalVector

- 当数值十分小的时候，可以认为 `equalVector` 跟角度是一个概念；
- CAD 判断两个向量**垂直**，使用 `equalVector > cos(v1, v2)`；
- CAD 判断两个向量**平行**，使用 `equalVector > |v1/|v1| - v2/|v2||`，或者两个向量的单位向量之和的模；
- 这里的两个向量要认为是从原点出发的两个向量。因为浮点误差的存在，两条平行的线，从它们身上取两个向量，这两个向量的单位向量不会完全一样；如果它们的终点指向同一侧，数值上终点也不会完全相同。

<svg viewBox="-1.5 -1.5 3 3" width="100%" height="100%" xmlns="http://w3.org">
  <defs>
    <!-- 坐标轴箭头样式 -->
    <marker id="arrow" viewBox="0 0 10 10" refX="5" refY="5" markerWidth="6" markerHeight="6" orient="auto-start-reverse">
      <path d="M 0 1 L 10 5 L 0 9 z" fill="#666" />
    </marker>
  </defs>

  <!-- 坐标轴 -->
  <line x1="-1.3" y1="0" x2="1.3" y2="0" stroke="#666" stroke-width="0.015" marker-end="url(#arrow)" />
  <line x1="0" y1="1.3" x2="0" y2="-1.3" stroke="#666" stroke-width="0.015" marker-end="url(#arrow)" />
  <text x="1.35" y="0.05" font-size="0.1" font-family="sans-serif" fill="#333">x</text>
  <text x="0.05" y="-1.35" font-size="0.1" font-family="sans-serif" fill="#333">y</text>

  <!-- 单位圆基底线 -->
  <circle cx="0" cy="0" r="1" stroke="#ccc" stroke-width="0.015" fill="none" />

  <!-- 橙色高亮：对应的弧长 AP (r=1 时弧长 = θ) -->
  <path d="M 1 0 A 1 1 0 0 0 0.7071 -0.7071" fill="none" stroke="#FF8C00" stroke-width="0.04" />

  <!-- 几何线条 -->
  <!-- 半径 OP -->
  <line x1="0" y1="0" x2="0.7071" y2="-0.7071" stroke="#228B22" stroke-width="0.02" />
  <!-- 基准线 OA -->
  <line x1="0" y1="0" x2="1" y2="0" stroke="#666" stroke-width="0.015" />
  <!-- 正弦线 PM -->
  <line x1="0.7071" y1="0" x2="0.7071" y2="-0.7071" stroke="#007BFF" stroke-width="0.02" stroke-dasharray="0.04,0.03" />
  <!-- 红色高亮：弦长 AP -->
  <line x1="1" y1="0" x2="0.7071" y2="-0.7071" stroke="#DC3545" stroke-width="0.03" />

  <!-- 角度 θ 弧线 -->
  <path d="M 0.25 0 A 0.25 0.25 0 0 0 0.1768 -0.1768" fill="none" stroke="#007BFF" stroke-width="0.02" />

  <!-- 关键点标记 -->
  <!-- 原点 O -->
  <circle cx="0" cy="0" r="0.025" fill="#333" />
  <text x="-0.12" y="0.12" font-size="0.08" font-family="sans-serif" fill="#333">O</text>

  <!-- A点 -->
  <circle cx="1" cy="0" r="0.025" fill="#333" />
  <text x="1.03" y="0.12" font-size="0.08" font-family="sans-serif" fill="#333">A(1,0)</text>

  <!-- P点 -->
  <circle cx="0.7071" cy="-0.7071" r="0.025" fill="#333" />
  <text x="0.75" y="-0.75" font-size="0.08" font-family="sans-serif" fill="#333">P(cosθ, sinθ)</text>

  <!-- 垂足 M -->
  <circle cx="0.7071" cy="0" r="0.02" fill="#333" />
  <text x="0.72" y="0.1" font-size="0.07" font-family="sans-serif" fill="#666">M</text>

  <!-- 文字标注 -->
  <!-- θ角 -->
  <text x="0.22" y="-0.08" font-size="0.08" font-family="sans-serif" fill="#007BFF" font-style="italic">θ</text>
  <!-- 半径 -->
  <text x="0.25" y="-0.4" font-size="0.08" font-family="sans-serif" fill="#228B22">r = 1</text>
  <!-- 正弦值 -->
  <text x="0.74" y="-0.33" font-size="0.08" font-family="sans-serif" fill="#007BFF">sinθ</text>
  <!-- 弦长 -->
  <text x="0.9" y="-0.45" font-size="0.08" font-family="sans-serif" fill="#DC3545" font-weight="bold">弦长 AP</text>
  <!-- 弧长 -->
  <text x="1.02" y="-0.35" font-size="0.08" font-family="sans-serif" fill="#FF8C00" font-weight="bold">弧长 AP (s=θ)</text>
</svg>
<svg viewBox="-1.5 -1.5 3 3" width="100%" height="100%" xmlns="http://w3.org">
  <defs>
    <!-- 箭头样式 -->
    <marker id="arrow" viewBox="0 0 10 10" refX="5" refY="5" markerWidth="6" markerHeight="6" orient="auto-start-reverse">
      <path d="M 0 1 L 10 5 L 0 9 z" fill="#666" />
    </marker>
  </defs>

  <!-- 1. 基准线与完美垂直参考（灰色/虚线） -->
  <!-- 水平基准面/基准线 -->
  <line x1="-1.2" y1="1" x2="1.2" y2="1" stroke="#333" stroke-width="0.02" />
  <text x="1.25" y="1.03" font-size="0.08" font-family="sans-serif" fill="#333">基准面</text>

  <!-- 理论上的完美垂直线（灰色虚线） -->
  <line x1="0" y1="1" x2="0" y2="-1.1" stroke="#999" stroke-width="0.015" stroke-dasharray="0.04,0.03" />
  
  <!-- 完美垂直的直角符号 -->
  <path d="M 0 0.85 L 0.15 0.85 L 0.15 1" fill="none" stroke="#999" stroke-width="0.015" />

  <!-- 2. 实际带有误差角度的测量线（实线） -->
  <!-- 实际测量线（偏转 12 度，顺时针旋转，计算得端点在 -1.1*sin(12) 附近） -->
  <!-- 旋转中心在 (0,1) -> 顶部端点 X = 0 + 2.1*sin(12°) ≈ 0.436, Y = 1 - 2.1*cos(12°) ≈ -1.054 -->
  <line x1="0" y1="1" x2="0.436" y2="-1.054" stroke="#DC3545" stroke-width="0.025" />

  <!-- 3. 误差角度 θ 标注 -->
  <!-- 在靠近顶部位置绘制圆弧表示偏转角 θ -->
  <!-- 圆心(0,1), 半径R=1.5, 从完美垂直(-90°)到实际线(-78°) -->
  <path d="M 0 -0.5 A 1.5 1.5 0 0 1 0.312 -0.467" fill="none" stroke="#007BFF" stroke-width="0.02" />
  
  <!-- 4. 文字与符号标注 -->
  <!-- 实际线标注 -->
  <text x="0.48" y="-0.95" font-size="0.08" font-family="sans-serif" fill="#DC3545" font-weight="bold">实际线条</text>
  
  <!-- 完美垂直线标注 -->
  <text x="-0.45" y="-0.95" font-size="0.08" font-family="sans-serif" fill="#999">理论垂直线</text>

  <!-- 误差角度 θ 文本 -->
  <text x="0.12" y="-0.6" font-size="0.09" font-family="sans-serif" fill="#007BFF" font-weight="bold" font-style="italic">θ (误差角)</text>

  <!-- 关键交点（垂足） -->
  <circle cx="0" cy="1" r="0.025" fill="#333" />
</svg>

可以看出，`ev` 判断平行和垂直的两个条件**不是一致的**，容易出现满足垂直但不满足平行的情况。或者说不满足平行时，有一些情况应该也不满足垂直，但却被判断为垂直了。

### 三点法构造圆

两边端点可以确定，但是终点的点到底选哪一个？

### 其他几何/坐标系结论

- **角度 + 法向量 + 标高** 能确定一个**不带原点的局部坐标系**。
- `Vector2d` 的角度初始默认值是 4.712，即初始方向朝向 y 轴负方向。

---

## 2026.6~8 — CAD API 细节

- [2026.6] `Point3d.DistanceTo` 内部要开方，开销较大。频繁比较距离时优先比较**距离的平方**。
- [2026.6] `Split` 之后，线段的 interval 上下界变成 0、1；而计算交点拿到的参数是**全局曲线的绝对几何长度**。切分后曲线变成有限实体。
- [2026.6] CAD 绘图本身没有单位，但插入外部图块时 CAD 会读取**插入比例**：如果当前图纸单位是毫米，插入一个英寸单位的图块，CAD 会把它放大 25.4 倍。
- [2026.6] CAD 中做面域操作时，如果两个点之间距离小于 10⁻⁶，判定为同一个点。
- [2026.6] `CurveCurveIntersector3d` 的容差用法：

```csharp
var userTol = 0.001;
var userTolerance = new Tolerance(userTol, userTol);
var pt = new CurveCurveIntersector3d(curve1, curve2, Vector3d.ZAxis, userTolerance);
var temp1 = pt.OverlapCount();
var temp2 = pt.NumberOfIntersectionPoints;
```

```
-----
    ------  ①

-----
   ----------  ②
```

显然情况 ② 比情况 ① 重叠的部分多。如果这块重叠的长度小于计算精度，则 overlap 算不出来，很合理；能算出交点，不怎么合理但能理解。

- [2026.6] **包围盒的用途之一是粗筛**：先用包围盒把永远不会碰撞的线段分开，减少无意义的两两比较次数。
- [2026.6] `DBText` 的 `AlignmentPoint` 只在**对齐方式不是左对齐**时才启用。左对齐时单行文本的位置只能通过 `dbText.Position` 控制，此时给 `AlignmentPoint` 赋值会报错。
- [2026.6] 原则上能不修改原图就不修改。没办法才改，但也要先看有没有权限。
- [2026.7] CAD 中射线的 `PointOnLine` 返回的是**起点**，本应返回任意一个点；而无限长的直线，这个属性返回的也是一个固定点——应该是数据库中记录的那个点，也就是画这条无限长直线时用到的点。
- [2026.7] `CompositeCurve3d`：多段线、矩形、多边形的 `GeCurve` 都是它。它有一个方法能得到组成这根复合曲线的所有曲线；反过来是否有方法能把曲线组合起来，待查。它的定义要求：组成复合曲线的所有曲线**端到端相连**，并且**有界**。
- [2026.7] 利用 Brep 从面域上提取的 `EdgeCollection`，每个元素返回 `ExternalCurve3d`。
- [2026.7] `acad.pgp`：External Command and Command Alias Definitions（外部命令与命令别名定义文件）。
- [2026.7] `typeof` 得到类型的 `Type` 对象，随即可以得到程序集的路径。相关入口：`GetCuiFileName`、`HelpManager.cs`。
- [2026.8] CAD 实体的 `Explode` 函数只是把炸开的碎片**收集起来返回**，并不删除原对象。

### Tekla Structure

- CAD 的块：只是把一堆线条"捆绑"在一起。
- Tekla 自定义组件：是把一堆 3D 零件**带有逻辑、带有公式地"缝合"在主结构上**。
- 在矩形轴线属性中，可以选择是否将对象绑定到轴线。如果绑定，移动轴线时对象会随之移动。

---

## 闪卡

#cad开发-flashcards

如果两个直线段重叠的话，curvecurveintersector的NumberOfIntersectionPoints==1;;也是0==，需要看overlapCount方法得到的int值，看有==1;;几段重叠区间==。
<!--SR:!2026-11-09,125,290-->

为什么`Group` 要先入库，再往`Group` 里面加实体？
?
当实体被加入组时，组会作为**永久反应器**附加到实体上。反应器系统需要组拥有合法的 `ObjectId`，否则无法完成反向关联。
此外，先把新建的组 group 类添加到组字典中，再把 group 入库。
<!--SR:!2026-10-21,42,240-->


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
