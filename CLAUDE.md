# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TGK (Tiny Geometric Kernel) is a Boundary Representation (BRep) engine written in C# for CAD/CAM applications. It includes a WPF/DirectX-based viewer and focuses on manifold geometry with simplified topology.

## Build Commands

- Build solution: `dotnet build`
- Run tests: `dotnet test`
- Run viewer: `dotnet run --project TGK.Viewer`

## Project Structure

The solution consists of three main projects:

### TGK (Core Library)
- **Topology**: BRep entities (Solid, Face, Edge, Vertex) - the main boundary representation structure
- **Geometry**: 3D mathematical foundations
  - `Xyz`: Core 3D point/vector struct (single class for points and vectors)
  - **Curves**: Line, Circle, and curve intersection logic
  - **Surfaces**: Plane, Sphere, Cylinder, Torus, NurbSurface with intersection capabilities
- **FaceterServices**: Mesh generation and triangulation (converts BRep to triangle mesh)
  - `Mesh`: Main faceting class that converts Solids to triangulated representation
  - `TriangulationUtils`: Ear clipping algorithm for face triangulation
- **Dxf**: DXF file format import/export

### TGK.Tests
- Uses NUnit testing framework
- Includes Verify.NUnit for approval testing with .verified.dxf files
- Test files mirror the main project structure

### TGK.Viewer  
- WPF application using HelixToolkit for 3D rendering
- MVVM pattern with CommunityToolkit.Mvvm
- MainViewModel manages the 3D scene and model tree

## Architecture Principles

- **Single geometry class**: `Xyz` struct serves as both points and vectors
- **Manifold-only**: Two faces share exactly one edge
- **No shells**: Only complete solids are supported
- **No loops**: Simplified topology without loop concepts
- **BRep hierarchy**: Solid → Face → EdgeUse → Edge → Vertex

## Key Patterns

- All BRep entities inherit from `BRepEntity` with Id and Tag properties
- Surface classes inherit from abstract `Surface` with intersection and parameter space methods  
- Argument validation at method start: check null references and ranges
- Use `var` only when type is obvious (e.g., `var myObject = new MyClass()`)
- Avoid abbreviations in naming

## Testing

- Tests verify geometric operations and maintain .verified.dxf files for visual verification
- VerifyUtils.cs provides helpers for DXF-based approval testing
- Run single test: `dotnet test --filter "TestName"`

## Dependencies

- .NET 8.0 (specified in global.json)
- Core library has no external dependencies
- Viewer uses HelixToolkit.SharpDX for 3D rendering
- Tests use NUnit and Verify.NUnit frameworks