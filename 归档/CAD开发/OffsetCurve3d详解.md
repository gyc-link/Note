---
date: 2026-05-13
tags:
  - CAD二次开发
status: draft
---

### OffsetCurve3d

#### API

An object of this class **stores a pointer to the base curve** of which it is an offset. This means that any modification of the base curve **also modifies** the offset curve. Since this curve is an exact offset, it may be **self-intersecting** even though the base curve is not self-intersecting

>a non-self-intersecting offset curve can be created by calling **Curve3d.GetTrimmedOffset.**

The positive direction of offset at each point of the base curve is perpendicular to the tangent vector at that point.
- So if the tangent vector at a point on the base curve is (x,y), then the positive direction of offset will be (-y,x).
- **The positive direction of offset** at each point of the base curve is the **cross product** of the specified normal vector with the tangent vector at that point.

>Objects of this class may also be constructed with an offset distance of 0, in which case the offset curve is simply a replica of the base curve.

Functions that modify the offset curve such as `TransformBy(), GetReverseParameterCurve(), and SetInterval()`, do not modify the base curve.

If GetReverseParameterCurve() is called for an offset curve, then the direction of the resulting offset curve will be opposite that of the base curve.

If SetInterval() is called for an offset curve, then the offset curve represents an offset of the **specified interval** of the base curve.
- This is useful for creating multiple offset curves, each of which represents a separate interval of the base curve. 
- For instance if three offset curves are constructed from the same base curve whose interval is `[0,1]`, then SetInterval() could be called for each of these offset curves to set their intervals to something like `[.1,.3]`, `[.5,.6]`, and `[.7,.9]`. 
- If the offset distance was set to 0, then these offset curves would each represent **a different trimmed portion of the base curve**. Any of these offset curves could then be transformed or reversed without modifying the other offset curves or the base curve.

>可以做分段视图

If the offset curve is constructed with an offset distance that is not 0, then **the continuity** of the offset curve will be **one less than** the continuity of the base curve. So if the base curve has continuity in the second derivative (but not the third), then the offset curve only has continuity in the first derivative. If the base curve is discontinuous in the first derivative, then the offset curve may not be continuous at all. Therefore, the base curve should at least **be continuous in the first derivative** to ensure that the offset curve represents a valid curve.

>会降低连续性

#### compare to GetOffsetCurves

##### line、polyline、spline

![](../../pic/Pasted%20image%2020260514110425.png)

##### arc

![](../../pic/Pasted%20image%2020260514150812.png)
#### 偏移方向

**正方向定义：**

偏移正方向 = 指定法向量 × 切向量（**叉积**）

- 若切向量为 (x,y)，正方向为 **(-y,x)**（逆时针旋转 90°），即垂直于切向量

**实体的offsetDist 正负值**（参见 [[../2026.5.11~12|offset 正负值]]）：

- 负值通常表示"变得更小"（如圆弧半径变小）
- 对没有"变小"概念的曲线，负值可能被解释为 WCS 坐标值减小的方向
- 此规则**不强制**，自定义实体可自行解释正负号

## 今日问题


#cad开发-flashcards 

![](../../pic/Pasted%20image%2020260513142112.png)
?
样条曲线向内偏移时，新曲线的半径会不断减小，如果收缩得太快，两边得线条直接交叉穿过了彼此。
![](../../pic/Pasted%20image%2020260513143112.png)
ge级别的offset API解释：If the offset curve is constructed with an offset distance that is not 0, then **the continuity** of the offset curve will be **one less than** the continuity of the base curve.
<!--SR:!2026-12-30,140,250--> 


 