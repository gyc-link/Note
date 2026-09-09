ObjectId有GetObject函数，防止嵌套事务，功能与事务的GetObject一样。
在一个事务里去调用ObjectId的GetObject函数，相当于一个语法糖，函数里会自动获取当前**事务**的引用。没有访问事务的情况下，==不能调用==。报错 unhandled access violation
该函数获得object最多只需要传入**三个**（**打开模式，数据库中删除的，锁定层**）参数，因为已经有id了。
对于**没有添加到数据库**的图形，没有objectid。 `trans.AddNewlyCreatedDBObject(ent, true);`返回ObjectId
如果事务abort，则objectid将失效。
数据库可能有多个，对应多个dwg文件。虽然objectid在当前数据库中唯一，多个数据库中可能会有重复的id
对在数据库中的图形对象，进行编辑操作需要是写的状态。`GetObject(openmodel.forwrite)`
反过来，将object强制转换成entity，则objectid是entity的属性 `ent.ObjectId`
ObjectId的属性里有database，即可以通过id获得这个图形所在的数据库。

### Transformby
- 对实体进行移动，旋转的几何变换。
	- 接收Matrix3d类型变量作为参数
	![](../pic/Pasted%20image%2020260327182839.png)
	- Matrix3d中的displacement函数，接收vector作为参数，可以用`point3d`的`getvectorto`函数，如下：  `Vector3d vector = Pt0.GetVectorTo(Pt1);`向量朝向 `pt0->pt1`
	- 是一个虚拟函数，Entity的派生类可以实现或者override这个函数


“打开”是==数据库对象的一种事务性状态==，指通过事务或底层api获取一个数据库驻留对象的访问权。

未添加进数据库中的对象只是一个普通的内存对象，对他进行操作不涉及数据库。

数据库有持久化的特性，内存是暂存的。-->数据库会通过内部的非托管机制持有对象的实际数据，但托管包装器（如 `Line` 对象）仍受 GC 管理；通常你会在事务中打开并获取引用，事务提交后如果你不再使用该引用，它也可能被回收，但数据库本身还保留着对象的非托管数据。








