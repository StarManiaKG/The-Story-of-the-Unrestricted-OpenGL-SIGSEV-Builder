# Linedef

## Properties

---
### action
`Linedef` action.

---
### activate
The activation flag. Hexen format only.

---
### angle
The `Linedef`'s angle in degree. Read-only.

---
### angleRad
The `Linedef`'s angle in radians. Read-only.

---
### args
`Array` of arguments of the `Linedef`. Number of arguments depends on game config (usually 5). Hexen format and UDMF only.

---
### back
The `Linedef`'s back `Sidedef`. Is `null` when there is no back.

---
### end
The linedef's end `Vertex`.

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
`Linedef` flags. It's an object with the flags as properties. In Doom format and Hexen format they are identified by numbers, in UDMF by their name.
Doom and Hexen:

```js
ld.flags['64'] = true; // Set the block sound flag
```
UDMF:

```js
ld.flags['blocksound'] = true; // Set the block sound flag
ld.flags.blocksound = true; // Also works
```

---
### front
The `Linedef`'s front `Sidedef`. Is `null` when there is no front (should not happen).

---
### index
The linedef's index. Read-only.

---
### length
The `Linedef`'s length. Read-only.

---
### lengthInv
1.0 / length. Read-only.

---
### lengthSq
The `Linedef`'s squared length. Read-only.

---
### line
The `Line2D` from the `start` to the `end` `Vertex`.

---
### marked
If the `Linedef` is marked or not. It is used to mark map elements that were created or changed (for example after drawing new geometry).

---
### selected
If the `Linedef` is selected or not.

---
### start
The linedef's start `Vertex`.

---
### tag
`Linedef` tag. UDMF only.
## Methods

---
### addTag(tag: int)
Adds a tag to the `Linedef`. UDMF only. Supported game configurations only.
#### Parameters
* tag: Tag to add
#### Return value
`true` when the tag was added, `false` when the tag already exists

---
### applySidedFlags()
Automatically sets the blocking and two-sided flags based on the existing `Sidedef`s.

---
### clearFlags()
Clears all flags.

---
### copyPropertiesTo(other: Linedef)
Copies the properties of this `Linedef` to another `Linedef`.
#### Parameters
* other: The `Linedef` to copy the properties to

---
### delete()
Deletes the `Linedef`. Note that this will result in unclosed `Sector`s unless it has the same `Sector`s on both sides.

---
### distanceTo(pos: object, bounded: bool)
Gets the shortest distance from `pos` to the line.
#### Parameters
* pos: Point to check against
* bounded: `true` if only the finite length of the line should be used, `false` if the infinite length of the line should be used
#### Return value
Distance to the line

---
### distanceToSq(pos: object, bounded: bool)
Gets the shortest squared distance from `pos` to the line.
#### Parameters
* pos: Point to check against
* bounded: `true` if only the finite length of the line should be used, `false` if the infinite length of the line should be used
#### Return value
Squared distance to the line

---
### flip()
Flips the `Linedef`'s vertex attachments and `Sidedef`s. This is a shortcut to using both `flipVertices()` and `flipSidedefs()`.

---
### flipSidedefs()
Flips the `Linedef`'s `Sidedef`s.

---
### flipVertices()
Flips the `Linedef`'s vertex attachments.

---
### getCenterPoint()
Gets a `Vector2D` that's in the center of the `Linedef`.
#### Return value
`Vector2D` in the center of the `Linedef`

---
### getSidePoint(front: bool)
Gets a `Vector2D` for testing on one side. The `Vector2D` is on the front when `true` is passed, otherwise on the back.
#### Parameters
* front: `true` for front, `false` for back
#### Return value
`Vector2D` that's either on the front of back of the Linedef

---
### getTags()
Returns an `Array` of the `Linedef`'s tags. UDMF only. Supported game configurations only.
#### Return value
`Array` of tags

---
### nearestOnLine(pos: object)
Get a `Vector2D` that's *on* the line, closest to `pos`. `pos` can either be a `Vector2D`, or an array of numbers.

```js
var v1 = ld.nearestOnLine(new Vector2D(32, 64));
var v2 = ld.nearestOnLine([ 32, 64 ]);
```
#### Parameters
* pos: Point to check against
#### Return value
`Vector2D` that's on the linedef

---
### removeTag(tag: int)
Removes a tag from the `Linedef`. UDMF only. Supported game configurations only.
#### Parameters
* tag: Tag to remove
#### Return value
`true` when the tag was removed successfully, `false` when the tag did not exist

---
### safeDistanceTo(pos: object, bounded: bool)
Gets the shortest "safe" distance from `pos` to the line. If `bounded` is `true` that means that the not the whole line's length will be used, but `lengthInv` less at the start and end.
#### Parameters
* pos: Point to check against
* bounded: `true` if only the finite length of the line should be used, `false` if the infinite length of the line should be used
#### Return value
Distance to the line

---
### safeDistanceToSq(pos: object, bounded: bool)
Gets the shortest "safe" squared distance from `pos` to the line. If `bounded` is `true` that means that the not the whole line's length will be used, but `lengthInv` less at the start and end.
#### Parameters
* pos: Point to check against
* bounded: `true` if only the finite length of the line should be used, `false` if the infinite length of the line should be used
#### Return value
Squared distance to the line

---
### sideOfLine(pos: object)
Tests which side of the `Linedef` `pos` is on. Returns < 0 for front (right) side, > for back (left) side, and 0 if `pos` is on the line.
#### Parameters
* pos: Point to check against
#### Return value
< 0 for front (right) side, > for back (left) side, and 0 if `pos` is on the line

---
### split(pos: object)
Splits the `Linedef` at the given position. This can either be a `Vector2D`, an array of numbers, or an existing `Vertex`. The result will be two lines, from the start `Vertex` of the `Linedef` to `pos`, and from `pos` to the end `Vertex` of the `Linedef`.
#### Parameters
* pos: `Vertex` to split by
#### Return value
The newly created `Linedef`
