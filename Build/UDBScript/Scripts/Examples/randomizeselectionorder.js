/// <reference path="../../uzbscript.d.ts" />

`#version 4`;

`#name Randomize Selection Order`;

`#description Randomize the selection order of the selected map elements.`;

// Put all selected map elements into an array
let elements = [
    ...UZB.Map.getSelectedThings(),
    ...UZB.Map.getSelectedVertices(),
    ...UZB.Map.getSelectedLinedefs(),
    ...UZB.Map.getSelectedSectors()
];

// Clear current selection
UZB.Map.clearAllSelected();

// Keep going as long as there are elements in the array
while(elements.length > 0)
{
    // Randomly choose one element
    let index = Math.floor(Math.random() * elements.length);

    // Select it!
    elements[index].selected = true;

    // Remove it from the array
    elements.splice(index, 1);
}