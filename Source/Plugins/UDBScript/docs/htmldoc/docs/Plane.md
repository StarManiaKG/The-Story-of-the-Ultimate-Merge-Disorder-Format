# Plane

## Constructors

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### Plane(normal: object, offset: double)
Creates a new `Plane` from a normal and an offset. The normal vector has to be `Vector3D`, `Array`s of 3 numbers, or an object with x, y, and z properties.

```js
let plane1 = new UDB.Plane(new Vector3D(0.0, -0.707, 0.707), 32);
let plane2 = new UDB.Plane([ 0.0, -0.707, 0.707 ], 32);
```
#### Parameters
* normal: Normal vector of the plane
* offset: Distance of the plane from the origin

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### Plane(p1: object, p2: object, p3: object, up: bool)
Creates a new `Plane` from 3 points. The points have to be `Vector3D`, `Array`s of 3 numbers, or an object with x, y, and z properties.

```js
let plane1 = new UDB.Plane(new Vector3D(0, 0, 0), new Vector3D(64, 0, 0), new Vector3D(64, 64, 32), true);
let plane2 = new UDB.Plane([ 0, 0, 0 ], [ 64, 0, 0 ], [ 64, 64, 32 ], true);
```
#### Parameters
* p1: First point
* p2: Second point
* p3: Thrid point
* up: `true` if plane is pointing up, `false` if pointing down
## Properties

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### a
The `a` value of the plane equation. This is the `x` value of the normal vector.

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### b
The `b` value of the plane equation. This is the `y` value of the normal vector.

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### c
The `c` value of the plane equation. This is the `z` value of the normal vector.

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### d
The `d` value of the plane equation. This is the same as the `offset` value.

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### normal
The plane's normal vector.

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### offset
The distance of the plane along the normal vector.
## Methods

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### closestOnPlane(p: object)
Returns the point that's closest to the given point on the `Plane`. The given point can be a `Vector3D` or an `Array` of three numbers.

```js
const plane = new UDB.Plane([ 0, 0, 0 ], [ 32, 0, 0 ], [ 32, 32, 16 ], true);
UDB.log(plane.closestOnPlane([ 16, 16, 32 ])); // Prints '16, 25.6, 12.8'
```
#### Parameters
* p: Point to get the closest position from
#### Return value
Point as `Vector3D` on the plane closest to the given point

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### distance(p: object)
Computes the distance between the `Plane` and a point. The given point can be a `Vector3D` or an `Array` of three numbers. A result greater than 0 means the point is on the front of the plane, less than 0 means the point is behind the plane.

```js
const plane = new UDB.Plane([ 0, 0, 0 ], [ 32, 0, 0 ], [ 32, 32, 16 ], true);
UDB.log(plane.distance([ 16, 16, 32 ])); // Prints '21.466252583998'
```
#### Parameters
* p: Point to compute the distnace to
#### Return value
Distance between the `Plane` and the point as `number`

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### getIntersection(from: object, to: object)
Checks if the line between `from` and `to` intersects the plane.

It returns an `Array`, where the first element is a `bool` vaue indicating if there is an intersector, and the second element is the position of the intersection on the line between the two points.


```js
const plane = new UDB.Plane([ 0, 0, 1 ], 0);
const [intersecting, u] = plane.getIntersection([0, 0, 32], [0, 0, -32]);
UDB.log(`${intersecting} / ${u}`); // Prints "true / 0.5"
```
#### Parameters
* from: `Vector3D` of the start of the line
* to: `Vector3D` of the end of the line
#### Return value
*missing*

---
<span style="float:right;font-weight:normal;font-size:66%">Version: 5</span>
### getZ(p: object)
Returns the position on the z axis of the plane for the given point. The given point can be a `Vector2D` or an `Array` of two numbers.

```js
const plane = new UDB.Plane([ 0, 0, 0 ], [ 32, 0, 0 ], [ 32, 32, 16 ], true);
UDB.log(plane.getZ([ 16, 16 ])); // Prints '8'
```
#### Parameters
* p: Point to get the z position from
#### Return value
*missing*
