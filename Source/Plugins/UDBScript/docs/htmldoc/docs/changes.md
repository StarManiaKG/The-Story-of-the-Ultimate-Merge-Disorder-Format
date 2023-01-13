# Changes
This site lists all changes between different API version of UDBScript

## Version 5

- Added `Plane` class
- Added `BlockMap`, `BlockEntry`, and `BlackMapQueryResult` classes
- `Sector` class
    - Added `getLabelPositions` method to get the position of sector labels (where tags, effects etc. are displayed)
- Added support for JavaScript BigInt for UDMF fields. This means it's not necessary anymore to use `UniValue` to assign integers to new UDMF fields. Instead it can be done like this: `sector.fields.my_int_field = 1n;`
- Added type information file (udbscript.d.ts)

## Version 4

- Moved all classes, object, and methods into the `UDB` namespace (everything has to be prefixed wiht `UDB.`)
- Added methods to report progress for long running scripts and script log output. See [Communicating with the user](gettingstarted.md#communicating-with-the-user) for more information

## Version 3

- Exported the classes `Linedef`, `Sector`, `Sidedef`, `Thing`, and `Vertex`, so that they can be used with `instanceof`
- `Map` class
    - The `getSidedefsFromSelectedLinedefs()` method now correctly only returns the `Sidedef`s of selected `Linedef`s in visual mode (and not also the highlighted one)
    - Added a new `getSidedefsFromSelectedOrHighlightedLinedefs()` method as the equivalent to the other `getSelectedOrHighlighted*()` methods
- `Sector` class
    - Added new `floorSelected`, `ceilingSelected`, `floorHighlighted`, and `ceilingHighlighted` properties. Those are mostly useful in visual mode, since they always return true when the `Sector` is selected or highlighted in the classic modes. The properties are read-only
- `Sidedef` class
    - Added new `upperSelected`, `middleSelected`, `lowerSelected`, `upperHighlighted`, `middleHighlighted`, and `lowerHighlighted` properties. Those are mostly useful in visual mode, since they always return true when the parent `Linedef` is selected or highlighted in the classic modes. The properties are read-only

## Version 2

- `Pen` built-in library
    - The methods of the `Pen` class now return the instance of the Pen class to allow method chaining