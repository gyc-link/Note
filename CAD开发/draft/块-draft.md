---
date: 2026-06-05
tags:
  - CAD二次开发
  - review
status: refined
---

## 引入

CAD 绘图中经常遇到一组图元反复出现——门、窗、螺丝、图框。每次都重画一遍既慢又容易不一致。如果门的设计改了，难道要逐一去改图纸上每个门的副本？

块解决的就是这个问题：**把一组图元打包成一个"组件"，定义一次，到处使用；改一次定义，所有引用自动更新。**

>[!important] 关键
>块 = 模板（BlockTableRecord）+ 实例（BlockReference）。
>模板决定"块长什么样"，实例决定"块放在哪、转多少度、缩多大"。


---

## 基础概念

### 块表 BlockTable

块表是 CAD 九大固定符号表之一，存储所有块定义。通过 `db.BlockTableId` 访问。

![[../dot/块表.svg]]

>[!info]
>CAD 创建时会自动生成三条 `BlockTableRecord`：`*MODEL_SPACE`、`*PAPER_SPACE`、`*PAPER_SPACE0`。这三条记录的 Name 可以写死。

### 块表记录 BlockTableRecord（块定义）

>[!help]- 什么是 BlockTableRecord
>块定义，代表一个块的内容本体——里面存了什么实体、长什么样。模型空间和布局空间本质也是块表记录。

- 模型空间的 Name 固定为 `*MODEL_SPACE`
- `BlockTableRecord` 继承了 `IEnumerable`，本身是一个实体容器
- 块表记录的 ObjectId 通过块表的索引器获得：`bt[BlockTableRecord.ModelSpace]` 或 `bt["块名"]`

> **来源**：[CAD二次开发基础](../CAD二次开发基础.md)

### 块参照 BlockReference

>[!help]- 什么是 BlockReference
>块定义的实例，代表某次插入。通过 `BlockTableRecord` 属性记录它引用的块定义的 ObjectId。

创建时需要提供：**插入点** + **块定义的 ObjectId**。添加到块表记录时可以设置：

| 属性 | 含义 |
|------|------|
| `Position` | 插入点 |
| `Rotation` | 旋转角度 |
| `ScaleFactors` | 缩放比例 |
| `Normal` | 法向 |
| `Layer / Color / Linetype` | 实体通用属性 |

### 块定义与块参照的关系

```
BlockTableRecord = 模板 / 图块内容本体
BlockReference   = 实例 / 这个图块内容的一次使用
```

>[!important] 修改块定义 → 所有引用该定义的 BlockReference 自动更新
>底层机制：数据变了 → 标记相关图形过期 → 重绘时重新生成图形（并非逐像素直接修改）。这也是 CAD 中有 `Regen`、`UpdateScreen`、缓存失效这些概念的原因。

---

## 三种块操作

### 新建块

1. 新建一个 `BlockTableRecord`
2. 把组成块的实体加进去（类似往模型空间添加实体）
3. 把块表记录加入 `BlockTable`
4. 之后就可以通过 `BlockReference` 去插入和使用它

### 修改块定义

本质是修改对应 `BlockTableRecord` 里的内容，操作方式类似于往模型空间里添加/修改实体。修改后所有引用该定义的 `BlockReference` 自动更新。

### 使用块

本质是在别的块里或模型空间里插入一个 `BlockReference`。插入后该块表记录的块定义就多了一个引用实例。

>[!NOTE] 标准操作模板
>```csharp
>// 1. 打开块表（读）
>var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
>// 2. 打开目标空间（写）
>var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
>// 3. 创建块参照
>var br = new BlockReference(insertPoint, blockDefId);
>// 4. 添加到块表记录
>btr.AppendEntity(br);
>tr.AddNewlyCreatedDBObject(br, true);
>```


---

## 属性块

属性块本质上还是块，只是在块定义里除了普通几何实体，还可以放一个或多个**数据标签**。比如一扇门除了几何线条，还带着"门牌号 M-07"、"宽度 900"这些可变信息。

![[../dot/属性块.svg]]

| | AttributeDefinition | AttributeReference |
|---|---|---|
| **角色** | 模板中的"字段说明" | 插入后真正保存的属性值 |
| **所在位置** | 块记录 `BlockTableRecord` 里（含 model space） | 块参照 `BlockReference` 里 |
| **内容** | tag（标签名）、value（默认值）、prompt（提示语）、是否常量 | 实际属性值，如 `Tag=DOOR_NO, TextString=M-07` |
| **基类** | `DBText` | `DBText` |

### 关键约束

>[!warning] AttributeReference 的添加限制
>`AttributeReference` 虽派生于 `DBText`，但**不能**像普通实体一样通过 `AppendEntity` 加入模型空间或块表记录。
>- 必须通过 `BlockTableRecord.AttributeCollection.AppendAttribute` 添加
>- `AppendEntity` 会拒绝属性参照，报错：`eIllegalEntityType`
>- 必须先有 `AttributeDefinition`，后才能有 `AttributeReference`

CAD 快捷命令：`ATTDEF` 或 `ATT`。


---

## 块的四种类型

CAD 中并非所有块都是用户创建的。按来源和用途分为四类：

![[../dot/四种块类型.svg]]

### 普通块

用户创建的命名块。识别方法——排除法：`IsFromExternalReference`、`IsLayout`、`IsAnonymous` 都为 false。

### 外部参照块（Xref）

把外部 DWG 文件链接到当前图形。Xref 在图形中也表现为 `BlockTableRecord` + `BlockReference`，但数据来自外部文件，只是"引用"。

>[!important] 块 vs Xref 的本质区别
>- **块**：数据**复制进**当前数据库，源文件改了不起作用
>- **Xref**：数据来自外部文件，外部文件更新后当前图形自动同步

关键属性：
- `BlockTableRecord.IsFromExternalReference` → 判断是否是 Xref
- `BlockTableRecord.PathName` → 获取外部参照文件路径

>[!info] Xref 的图层特征
>Xref 文件的图层在当前图形中变为 `XrefName|LayerName` 格式（竖线分隔），是判断实体是否来源于 Xref 的常用方法。

### 布局块（Layout Block）

用 `IsLayout` 识别。CAD 加载时自动生成，可新建 n 个，模拟真实纸张打印页面。`*Model_Space` 是特殊的布局块。

典型用法：在布局块中插入图框块（规范出图），再添加 Viewport（视口框）以不同比例查看模型空间。
- 视口框 → Paper Space 的对象
- 通过视口框看到的模型线条 → Model Space 的对象

### 匿名块（Anonymous Block）

`IsAnonymous` = true。CAD 内部自动生成，用于实现内部机制——**填充**、关联标注等。名字一般是 `*U123` 形式。


---

## 底层实现

### BlockTableRecord 构造函数

无参构造 `new BlockTableRecord()` 不是单纯创建 C# 对象——它会通过 `acHeapAlloc`（CAD 自己的 C++ 堆内存分配函数）申请 **24 字节**内存，并调用 `AcDbBlockTableRecord` 的构造函数创建底层 C++ 对象。

- `ac` = AutoCAD 相关前缀
- `heap` = 堆
- `alloc` = allocate = 分配内存

>[!NOTE] 24 字节意味着什么
>int 占 4 字节，24 字节最多放 6 个 int。这揭示了底层：`BlockTableRecord` 在 C++ 侧是一个固定大小的结构体，C# 侧是对它的封装。

内部构造函数 `internal BlockTableRecord(IntPtr unmanagedPointer, bool autoDelete)` 则**不创建**新底层对象，而是把已存在的底层对象指针包装成 C# 对象。有参构造通过 `:base(...)` 逐层将"底层 C++ 对象指针"和"是否自动释放"交给基类管理。

### 符号表继承关系

`SymbolTable` 抽象类提供统一的 `Has()` 方法，检索符号表中是否包含指定名称的记录。`BlockTable` 继承该方法，可直接检查块记录是否存在。

> **来源**：[2026.4.24-flashcards](../../flashcards/2026.4.24-flashcards.md)、[2026.4.3-flashcards](../../flashcards/2026.4.3-flashcards.md)

---

## 块与嵌套

### FullSubentityPath

当实体嵌套在块中时，需要唯一标识"它在哪里"。`FullSubentityPath` 结构体就是做这件事的：

```
块内的圆: [BlockReferenceId, CircleId]
块内的块内的线: [外层BlockReferenceId, 内层BlockReferenceId, LineId]
```

由两部分组成：
- **Object ID 路径**：从外到内的 `BlockReference` 链，最后指向实体自身
- **Subentity ID**：子实体在所属实体内部的标识（如某条边、某个面）

在 `PointMonitor` 事件中，`e.Context.GetPickedEntities()` 返回的正是 `FullSubentityPath`，可以追踪从最外层对象到最终被命中对象的完整路径。

> **来源**：[三维实体](../三维实体.md)、[扩展数据、扩展字典、有名对象字典](../扩展数据、扩展字典、有名对象字典.md)

---

## 与其他概念对比

### 块 vs 外部参照

| | 块 | Xref |
|---|---|---|
| 数据位置 | 复制进当前数据库 | 引用外部文件 |
| 源文件更新 | 不影响 | 自动同步 |
| 识别方式 | 排除法 | `IsFromExternalReference` |

### 块 vs 组（Group）

组同样可以包含多个实体，但允许**单独控制**成员的可见性、颜色、图层等；块实例更偏向"整体引用"——块内的实体不能单独修改属性。组存储在 `GroupDictionary` 中（通过 `Database.GroupDictionaryId` 获取）。

> **来源**：[文档和图形数据库](../文档和图形数据库.md)、[扩展数据、扩展字典、有名对象字典](../扩展数据、扩展字典、有名对象字典.md)

---

## 数据库间的块传递

### Wblock

`Database.Wblock()` 从已有数据库创建新数据库，或将指定实体复制到另一个数据库。使用后必须手工销毁返回的新数据库。

### Insert

`Database.Insert()` 将源数据库中的实体复制到目标数据库：

- `Insert(string blockName, Database dataBase, bool retain)` — 源数据库模型空间实体 → 目标数据库新建块表记录
- `Insert(string destinationBlockName, string sourceBlockName, Database dataBase, bool retain)` — 源数据库有名块表记录 → 目标数据库有名块表记录。目标没有则创建，有则取代

> **来源**：[文档和图形数据库](../文档和图形数据库.md#2.3%20插入数据库)

---

## 补充

### 非数据库驻留实体

`new Line()` 时它只在内存里，没加到 `BlockTableRecord` 之前数据库根本不认识它。一旦加到数据库（如 `btr.AppendEntity(line)`），它就获得了 ObjectId。

> **来源**：[中-英](../../flashcards/中-英.md)

### Annotative 块

CAD 中 Annotative 对象会自动根据视口比例调整大小——标注、文字、**块**都可以是 annotative 的。

> **来源**：[中-英](../../flashcards/中-英.md)

### 可缩放块

块参照支持 `ScaleFactors` 属性进行各向缩放。

> **来源**：[2026.5.4](../每天的零散记录/2026.5.4.md)

## 闪卡

#cad开发-flashcards 

块定义与块参照是什么关系？分别对应什么类？
?
- 块定义：模板，存放一组组成块的实体
    - 对应：`BlockTableRecord`
- 块参照：实例，把某个块定义插入到图中某处
    - 对应：`BlockReference`
- 二者关系可以理解成：
    - BlockTableRecord = 图块内容本体
    - BlockReference = 这个图块内容的一次使用
<!--SR:!2026-11-19,142,292-->

`BlockReference` 是怎么知道自己引用了哪个块定义的？
?
- `BlockReference` 内部通过 BlockTableRecord 的 ObjectId 来记录它引用的块定义
- 创建块参照时，通常至少要提供：
    - 插入点
    - 块定义的 ObjectId
<!--SR:!2026-11-13,138,292-->


修改块定义、使用块、新建块，分别在做什么？
?
**1. 修改块定义**
- 本质：修改对应的 `BlockTableRecord` 里的内容
- 结果：所有引用该定义的 `BlockReference` **自动更新**
**2. 使用块**
- 本质：在别的块里或模型空间里**插入一个 `BlockReference`**
- 插入后，可以设置，可以理解为实例化参数：
    - `Position`
    - `Rotation`
    - `ScaleFactors`
    - `Normal`
    - `Layer / Color / Linetype`
**3. 新建块**
- 先新建一个 `BlockTableRecord`
- 再把组成块的实体加进去
- 然后把这个块表记录加入 `BlockTable`
- 之后就可以通过 `BlockReference` 去插入和使用它
<!--SR:!2026-11-03,134,292-->

什么是属性块？属性定义和属性参照的相关类是什么？分别放在哪里？都派生于哪个类？
?
属性块本质上还是块，只是块定义里除了普通几何实体，还可以放一个或多个数据标签。
属性定义：`AttributeDefinition`
放在 块记录（`BlockTableRecord`）里（包括model space），相当于模板中的“字段说明”，如标签tag、值value（default value）、提示prompt（）
属性参照：`AttributeReference`
放在 块参照（`BlockReference`）里，相当于某次插入后真正保存的属性值
都派生于dbtext类
虽然AttributeReference派生于DBText，但是并不能像普通实体一样被加入到模型空间里，或者块表记录里，需要调用块表记录的AttributeCollection属性，调用它的AppendAttribute函数才能添加。
因为属性参照本质是为属性定义提供自定义数据，先要有属性定义，后才能有属性参照。另外，块表记录的AppendEntity函数会拒绝属性参照作为实体添加进来，报错信息：eIllegalEntityType
<!--SR:!2026-10-24,63,252-->

怎么找到cad中的普通块？
排除==1;;IsFromExternalReference、IsLayout、IsAnonymous==
外部参照块是对==1;;外部图纸==的引用、布局块是cad加载时自动生成的三个块中的另外两个默认的块，这个块可以新建n个，作用是==1;;模拟真实纸张打印页面的空间==，通常是在布局块中插入一个图框块（规范出图，记录项目名称，日期等信息），然后在布局块中添加一个==1;;Viewport;;视口框==，这个视口框可以看到model space上的模型,并能设置不同显示比例,，匿名块是==1;;cad实现内部机制自动生成的块==。
<!--SR:!2026-11-17,105,257-->

模型空间model space这条块表记录(blocktablerecord)的Name属性固定为==`*MODEL_SPACE`==
<!--SR:!2027-03-31,226,270-->

符号表/符号记录表对应的类是什么？之间的关系是什么？分别管理什么？
?
- `SymbolTable，SymbolRecordTable`
- 符号表是一种容器对象，保存对应的符号表记录。如块表包含模型空间，图纸空间和用户创建的块定义，以上的块表记录则包含了图形数据库的实体。层表包含了所有的图层，一个图层对应一条层表记录，层表记录图层的属性设置。但是块表记录继承了IEnumerable接口，是个容器。
- 符号表管分类，符号记录管具体内容
<!--SR:!2026-12-05,130,224-->

SymbolTable 抽象类提供统一的 ==Has ()== 方法，用于检索当前符号表中是否包含指定名称的符号表记录。子类（LayerTable / BlockTable）继承该方法，可直接用于检查图层、块记录是否存在。
<!--SR:!2026-11-08,155,284-->