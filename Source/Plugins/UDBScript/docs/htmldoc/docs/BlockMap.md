# BlockMap

A blockmap is used to retrieve a collection of localized map elements (things, linedefs, sectors, vertices). It can help to significantly speed up costly computations that would otherwise be applied to a large portion of the map elements. The blockmap divides the map into rectangular blocks and computes which map elements are fully or partially in each block. Then you can query the blockmap about only some of those blocks, and perform any further actions only on the map elements that are in those blocks.

If you for example wanted to find out which sector is at the (0, 0) position you could write something like this without using a blockmap:

```js
UDB.Map.getSectors().findIndex((s, i) => {
	if(s.intersect([ 0, 0 ]))
	{
		UDB.log(`Found ${s} after ${i} tries.`)
		return true;
	}
});
```
This loops through all sectors of the map and uses the `intersect` method to test if the point is inside the sector. While `intersect` is quite fast on its own, doing it potentially thousands of times adds up quickly, especially if you have to loop through all sectors multiple times.
A pretty extreme example for this is the map Bastion of Chaos. The map contains nearly 32500 sectors, and the sector at (0, 0) is number 25499. That means that the above script has to run `intersect` on 25499 sectors, even on those that are not remotely near the (0, 0) position.

Using a blockmap the code could look like this:

```js
const blockmap = new UDB.BlockMap();

blockmap.getBlockAt([ 0, 0 ]).getSectors().findIndex((s, i) => {
	if (s.intersect([0, 0]))
	{
		UDB.log(`Found ${s} after ${i} tries.`)
		return true;
	}
});
```
As you can see the code is quite similar, the difference being that a blockmap is created, and `UDB.Map` is replaced by `blockmap.getBlockAt([ 0, 0 ])`, the latter only getting a single block from the blockmap, that only contains the map elements that are in this block. Taking Bastion of Chaos as an example again, this code finds the sector after only 20 checks, instead of the 25499 checks in the first code example.

!!! note
    Creating a blockmap has a small overhead, since it has to compute which map elements are in which blocks. This overhead, however, is quickly compensated by the time saved by not looping through irrelevant map elements. You can decrease this overhead by using a `BlockMap` constructor that only adds certain map element types to the blockmap.
## Constructors

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### BlockMap()
Creates a blockmap that includes linedefs, things, sectors, and vertices.

```js
// Create a blockmap that includes all linedefs, things, sectors, and vertices
const blockmap = new UDB.BlockMap();
```

---
### BlockMap(lines: bool, things: bool, sectors: bool, vertices: bool)
Creates a blockmap that only includes certain map element types.

```js
// Create a blockmap that only includes sectors
const blockmap = new UDB.BlockMap(false, false, true, false);
```
#### Parameters
* lines: If linedefs should be added or not
* things: If thigs should be added or not
* sectors: If sectors should be added or not
* vertices: If vertices should be added or not
## Methods

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### getBlockAt(pos: object)
Gets the `BlockEntry` at a point. The given point can be a `Vector2D` or an `Array` of two numbers.

```js
const blockmap = new UDB.BlockMap();
const blockentry = blockmap.getBlockAt([ 64, 128 ]);
```
#### Parameters
* pos: The point to get the `BlockEntry` of
#### Return value
The `BlockEntry` on the given point

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### getLineBlocks(v1: object, v2: object)
Gets a `BlockMapQueryResult` for the blockmap along a line between two points. The given points can be `Vector2D`s or an `Array`s of two numbers.

```js
const blockmap = new UDB.BlockMap();
const result = blockmap.getLineBlocks([ 0, 0 ], [ 512, 256 ]);
```
#### Parameters
* v1: The first point
* v2: The second point
#### Return value
The `BlockMapQueryResult` for the line between the two points

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### getRectangleBlocks(x: int, y: int, width: int, height: int)
Gets a `BlockMapQueryResult` for the blockmap in a rectangle.

```js
const blockmap = new UDB.BlockMap();
const result = blockmap.getRectangleBlocks(0, 0, 512, 256);
```
#### Parameters
* x: X position of the top-left corner of the rectangle
* y: Y position of the top-left corner of the rectangle
* width: Width of the rectangle
* height: Height of the rectangle
#### Return value
*missing*
