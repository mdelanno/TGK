# TGK

TGK is the acronym for **Tiny Geometric Kernel**. It is a Boundary Representation (BRep) engine written in C# that can be used in CAD/CAM applications.

A viewer based on WPF/DirectX is included.

![A screenshot of the viewer](https://github.com/mdelanno/TGK/blob/master/ViewerScreenshot.png?raw=true)

Similar to [OpenCascade](https://github.com/Open-Cascade-SAS/OCCT), but much smaller and simpler.

## License

MIT

## Goals 

To be doable, I had to limit the scope of the project:

- manifold geometry only: two faces share an edge ;
- only one class for points and vectors ;
- no support for shells, only solids ;
- no concept of loops ;
- no 2D projections (to produce 2D drawings) ;
- no fillets, no chamfers ;
- no lofting, no sweeping ;
- no helix primitives.

## Capabilities

Things which are done are in _italics_.

- _points and vectors in 3D space_ ;
- 3D transformations: translation, rotation, scaling ;
- 3D primitives: _box_, sphere, cone, torus. A cylinder can be represented with a cone ;
- extrusion, revolution ;
- 3D curves : line, circle, ellipse, NURBS ;
- 3D surfaces : _plane_, sphere, cone, torus, NURBS ;
- intersections between 3D curves ;
- intersections between 3D curves and surfaces ;
- intersections between 3D surfaces ;
- boolean operations: intersection, union and difference ;
- faceting of 3D curves, 3D surfaces and 3D solids ;
- STEP import/export ;
- STL export.