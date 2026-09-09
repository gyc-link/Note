---
date: 2026-04-01
tags:
  - "#CAD二次开发"
  - review
status: evergreen
sr-due: 2026-08-25
sr-interval: 84
sr-ease: 270
---
#cad开发-flashcards 
### 基本内容
wrapper
	- 底层：**C++ ObjectARX**
	- 上层：**.NET 封装（Wrapper 包装模式）**
	- 核心容器：**Database（图形数据库）** → 所有 DWG 数据都在这里
.NET开发需要引用的程序集
	-![](../pic/Pasted%20image%2020260327173254.png)
	操作图形数据-acdbmgd.dll
	用户交互/注册命令-acmgd.dll
	几何/基础功能-accoremgd.dll
### 图形数据库四大根容器

1. **层表 LayerTable**
    
    - 存：所有图层定义（名称、颜色、线型、开关）
    - 访问：`db.LayerTableId` 
    
2. **块表 BlockTable**
    
    - 存：所有块定义
    - 包含：*Model_Space、*Paper_Space、自定义块
    - 访问：`db.BlockTableId`
    
3. **其他符号表**
    
    - 文字样式、标注样式、线型、视图、UCS 等
    
4. **NamedObjectDictionary**
    
    - 存：非图形对象（布局、组、多线样式、自定义数据）
    - 万能容器，键值对存储
    

cad所有符号表

| **符号表名称**          | **存储的内容**             |
| ------------------ | --------------------- |
| **LayerTable**     | 图层定义                  |
| **BlockTable**     | 块定义（模型空间和布局空间本质也是块）   |
| **LinetypeTable**  | 线型定义                  |
| **TextStyleTable** | 文字样式                  |
| **ViewTable**      | 命名视图                  |
| **UCSTable**       | 用户坐标系                 |
| **RegAppTable**    | 注册应用程序名（用于扩展数据 XData） |
| **DimStyleTable**  | 标注样式                  |
| **VportTable**     | 视口配置                  |

CAD 的符号表是一个以==名称==为索引的数据库容器。
<!--SR:!2026-11-24,113,290-->

### 事务 Transaction

#### 1. 为什么必须用事务

- CAD 数据库**所有读写必须在事务里**
- 自动管理打开 / 关闭对象
- 异常自动回滚，不炸图(数据库的原子性)
- 一次性操作

#### 2. 标准模板

块表的objectid通过数据库的属性获得 db.BlockTableId
块表记录的objectid通过块表的索引器获得 
由于启动cad时会自动创建三条`blocktablerecord`，分别对应模型空间和两个布局。因此，这三条记录的Name可以写死。 
```csharp
var db = HostApplicationServices.WorkingDatabase;

using (var tr = db.TransactionManager.StartTransaction())
{
    try
    {
        // 1. 打开块表/层表等（读）
        var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

        // 2. 打开模型空间（写）
        var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

        // 3. 创建实体：线/圆/文字...
        var line = new Line(new Point3d(0, 0, 0), new Point3d(100, 100, 0));

        // 4. 添加到模型空间
        //btr添加后就返回ObjectID
        btr.AppendEntity(line);    
        tr.AddNewlyCreatedDBObject(line, true);

        // 5. 提交
        tr.Commit();
    }
    catch
    {
        tr.Abort();
        throw;
    }
}
```
#### 3. 补充
- 事务可以嵌套
	- **“内层 Commit = 暂存，外层 Commit = 最终生效”。**
	- **嵌套事务是按栈来管理的，必须按后进先出（LIFO）关闭。**  嵌套事务必须按与创建相反的顺序 `Commit` 或 `Abort`。也就是先关最内层，再关外层。并且如果最外层事务中止，那么所有层里的更改都会被撤销。

---

### Transaction类的GetObject 函数

#### GetObject函数 
获取驻留在cad数据库中的对象
```csharp
tr.GetObject(
    id,                // ObjectId
    mode,              // 打开模式
    openErased,        // 是否打开已删除对象
    forceOpenOnLockedLayer // 强制打开锁定层
)
```
- **OpenMode**：
    
    - `ForRead` 只读
    - `ForWrite` 可写
    - `ForNotify` 通知
---
### ObjectId，Handle

- **ObjectId**
    - 会话内唯一，图形被添加到数据库中时才会产生
    - 重启 CAD 会变
- **Handle**
    - 随 DWG 保存，永久不变
    - 用于持久化记录、跨会话定位
- 互转：
    - `objectId.Handle`
    - `db.GetObjectId(handle)`
---
### DBObject 与 Entity

- **DBObject**
    - 所有数据库对象基类
- **Entity**
    - 所有**有图形显示**的对象基类
    - Line、Circle、Polyline、Text、MText 等

---

### UCS 与 WCS 坐标系

如果用户修改了 UCS：
![](../pic/Pasted%20image%2020260601152639.png)
> 补充①  ed.GetPoint()拿到的是 ==1;;UCS坐标系下的点==
> 补充②  几乎所有的实体类构建均使用 ==1;;WCS坐标系==
> 补充③  Jig中，jigPrompts.AcquirePoint()从鼠标屏幕处得到的点是 ==1;;WCS坐标系==
> 补充④  Polyline.AddVertexAt()，使用的坐标是 ==1;;OCS坐标系==
> 补充⑤ 有 Normal/组码210 的实体，底层数据库里是 ==1;;OCS坐标系==；API 层通常自动转换成 ==1;;WCS坐标系==
<!--SR:!2026-10-04,82,271-->

`ed.CurrentUserCoordinateSystem` 获得的是 ==1;;UCS（当前用户坐标系）到 WCS（世界坐标系）的转换矩阵==。使用 `Inverse` 进行逆计算后得到从 ==1;;WCS== 转 ==1;;UCS== 的转换矩阵。
<!--SR:!2026-12-07,128,291-->

#### GeometricExtents

Accesses the corner points (in WCS coordinates) of a box (**with edges parallel to the WCS X, Y, and Z axes**) that encloses the 3D extents of the entity.


==1;;视图;;view==指==1;;图形窗口显示的内容==，使用editor的==1;;getcurrentview==获得当前视图，使用==1;;setcurrentview==更新视图,设置视图的中心点、宽、高。
<!--SR:!2026-10-08,115,297-->

==1;;viewport;;视口==，cad有两种视口，==1;;ViewportTableRecord和Viewport==。
前者表示模型空间的视口（平铺视口）
视口记录表特殊在于，==1;;不同的记录可以有相同的Name==（其他的记录在添加或者创建新记录时，==1;;都会在符号表中查询是否存在这条record==）。==1;;相同Name的视口记录被作为一组记录==，因此可以实现左右分屏、四宫格。
后者表示图纸空间的视口（浮动视口）==1;;floating== viewports in Paper space
<!--SR:!2026-10-25,46,257-->

db.Clayer是什么::当前数据库下图层的ObjectId
<!--SR:!2026-09-19,121,290-->

怎么通过图层名查找层表记录，类似于字典
?
- 通过键-值的方法 `lt[layername]`得到层表记录的objectid
<!--SR:!2027-07-22,316,290-->

CAD中，0 层，Defpoints 层，当前层 ，包含对象的图层，部参照依赖层，隐形引用，不能删除，说一下理由，如果要删除图层，由于有不能删除的图层，需要进行什么操作（说操作就行，函数不记得没事）？另外图层名需要合法（cad提供了函数检查）
?
- **0 层**：它是数据库的基石，永远不能删除。
- **Defpoints 层**：由标注自动生成的层，存放标注定义的关键点，无法删除。
- **当前层 (Current Layer)**：你正在使用的层不能删（需先切换到别的层）。
- **包含对象的图层**：只要层里有一根线、一个点，甚至一个看不见的“空白块”，它就不能被常规删除。
- **外部参照依赖层**：来自 Xref（外部参照）图纸的图层。
- 隐形引用，快定义和样式关联
- 因此使用层表的`GenerateUsageData`函数更新图层的引用状态，确保层表记录的isused是最新的数据。
<!--SR:!2026-09-28,65,210-->