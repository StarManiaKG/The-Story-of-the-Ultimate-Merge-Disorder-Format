/// <reference path="../../../uzbscript.d.ts" />

`#version 4`;
`#name Triangulate Sectors`;
`#description Triangulates the selected or highlighted sectors into new sectors. Note that the triangulation will not "be beautiful", and that the sectors with islands may cause problems.`;

let sectors = UZB.Map.getSelectedOrHighlightedSectors();

if(sectors.length == 0)
    UZB.die('No sectors selected or highlighted');

// Draw all triangles. Remember to add the first point at the end so that the drawing will be closed
sectors.forEach(s => s.getTriangles().forEach(t => UZB.Map.drawLines([...t, t[0]])));