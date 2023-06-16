# Thing

## Properties

---
### action
`Thing` action. Hexen and UDMF only.

---
### angle
Angle of the `Thing` in degrees, see https://doomwiki.org/wiki/Angle.

---
### angleRad
Angle of the `Thing` in radians.

---
### args
`Array` of arguments of the `Thing`. Number of arguments depends on game config (usually 5). Hexen format and UDMF only.

---
### fields
UDMF fields. It's an object with the fields as properties.

```js
s.fields.comment = 'This is a comment';
s.fields['comment'] = 'This is a comment'; // Also  works
s.fields.xscalefloor = 2.0;
t.fields.score = 100;
```
It is also possible to define new fields:

```js
s.fields.user_myboolfield = true;
```
There are some restrictions, though:

* it only works for fields that are not in the base UDMF standard, since those are handled directly in the respective class
* it does not work for flags. While they are technically also UDMF fields, they are handled in the `flags` field of the respective class (where applicable)
* JavaScript does not distinguish between integer and floating point numbers, it only has floating point numbers (of double precision). For fields where UDB knows that they are integers this it not a problem, since it'll automatically convert the floating point numbers to integers (dropping the fractional part). However, if you need to specify an integer value for an unknown or custom field you have to work around this limitation:
Version 5 and later:
You can use a `BigInt`. This is done by appending a `n` to the number. Note that this is just a convenient way to define whole numbers, it still only supports 32 bit integers:

```js
s.fields.user_myintfield = 25n; // Sets the 'user_myintfield' field to an integer value of 25
```
In version 4 and earlier you have to use the `UniValue` class:

```js
s.fields.user_myintfield = new UDB.UniValue(0, 25); // Sets the 'user_myintfield' field to an integer value of 25
```
To remove a field you have to assign `null` to it:

```js
s.fields.user_myintfield = null;
```

---
### flags
`Thing` flags. It's an object with the flags as properties. In Doom format and Hexen format they are identified by numbers, in UDMF by their name.
Doom and Hexen:

```js
t.flags["8"] = true; // Set the ambush flag
```
UDMF:

```js
t.flags['ambush'] = true; // Set the ambush flag
t.flags.ambush = true; // Also works
```

---
### index
Index of the `Thing`. Read-only.

---
### marked
If the `Thing` is marked or not. It is used to mark map elements that were created or changed (for example after drawing new geometry).

---
### pitch
Pitch of the `Thing` in degrees. Only valid for supporting game configurations.

---
### position
Position of the `Thing`. It's an object with `x`, `y`, and `z` properties. The latter is only relevant in Hexen format and UDMF.
The `x`, `y`, and `z` accept numbers:

```js
t.position.x = 32;
t.position.y = 64;
```
It's also possible to set all fields immediately by assigning either a `Vector2D`, `Vector3D`, or an array of numbers:

```js
t.position = new UDB.Vector2D(32, 64);
t.position = new UDB.Vector3D(32, 64, 128);
t.position = [ 32, 64 ];
t.position = [ 32, 64, 128 ];
```

---
### roll
Roll of the `Thing` in degrees. Only valid for supporting game configurations.

---
### selected
If the `Thing` is selected or not.

---
### tag
`Thing` tag. UDMF only.

---
### type
Type of the `Thing`.
## Methods

---
### clearFlags()
Clears all flags.

---
### copyPropertiesTo(t: Thing)
Copies the properties from this `Thing` to another.
#### Parameters
* t: The `Thing` to copy the properties to

---
### delete()
Deletes the `Thing`.

---
### distanceTo(pos: object)
Gets the distance between this `Thing` and the given point. The point can be either a `Vector2D` or an array of numbers.

```js
t.distanceToSq(new UDB.Vector2D(32, 64));
t.distanceToSq([ 32, 64 ]);
```
#### Parameters
* pos: Point to calculate the distance to.
#### Return value
Distance to `pos`

---
### distanceToSq(pos: object)
Gets the squared distance between this `Thing` and the given point.
The point can be either a `Vector2D` or an array of numbers.

```js
t.distanceToSq(new UDB.Vector2D(32, 64));
t.distanceToSq([ 32, 64 ]);
```
#### Parameters
* pos: Point to calculate the squared distance to.
#### Return value
Distance to `pos`

---
### getSector()
Determines and returns the `Sector` the `Thing` is in.
#### Return value
The `Sector` the `Thing` is in

---
### snapToAccuracy()
Snaps the `Thing`'s position to the map format's accuracy.

---
### snapToGrid()
Snaps the `Thing`'s position to the grid.
