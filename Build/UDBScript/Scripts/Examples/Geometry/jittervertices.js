/// <reference path="../../../uzbscript.d.ts" />

`#version 4`;

`#name Jitter Vertices`;

`#description Jitters the selected vertices. If no vertices are selected all vertices in the map are jittered. Does not make sure that the resulting geometry is still valid.`;

`#scriptoptions

min
{
    description = "Minimum jitter";
    default = 0;
    type = 1; // Integer
}

max
{
    description = "Maximum jitter";
    default = 16;
    type = 1; // Integer
}
`;

// Gets a random value between min and max, then randomly make it negative
function getRandomValue(min, max)
{
    return (Math.floor(Math.random() * (max-min)) + min) * (Math.random() < 0.5 ? 1 : -1);
}

// Get selected vertices
let vertices = UZB.Map.getSelectedVertices();

// No vertices selected? Get all vertices!
if(vertices.length == 0)
    vertices = UZB.Map.getVertices();

// Jitter each vertex
vertices.forEach(v => {
    v.position += [
        getRandomValue(UZB.ScriptOptions.min, UZB.ScriptOptions.max),
        getRandomValue(UZB.ScriptOptions.min, UZB.ScriptOptions.max),
    ];
});  