 ﻿using CommunityToolkit.Mvvm.Input;
using Cyotek.Drawing.BitmapFont;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D11;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media.Media3D;
using TGK.FaceterServices;
using TGK.Geometry;
using TGK.Geometry.Curves;
using TGK.Topology;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using Color = SharpDX.Color;
using DiffuseMaterial = HelixToolkit.Wpf.SharpDX.DiffuseMaterial;
using EffectsManager = HelixToolkit.SharpDX.Core.EffectsManager;
using HitTestResult = HelixToolkit.SharpDX.Core.HitTestResult;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;
using PointFigure = HelixToolkit.SharpDX.Core.PointFigure;

namespace TGK.Viewer.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    readonly BitmapFont _flamaFont;

    readonly IView _view;


    Solid? _solid;

    bool _showVertices = true;

    bool _showEdges = true;

    bool _showFaces = true;

    bool _showIsoCurves = true;

    bool _showVerticesNames;

    readonly TextureModel _fontTexture;

    BillboardText3D? _modelSpaceLabel;

    double _chordHeight = 0.1;

    bool _showFaceNormals;

    bool _showBoundingBox;

    GridLength _parametricSpacePaneColumnWidth = new(0, GridUnitType.Pixel);

    GridLength _lastParametricSpacePaneColumnWidth = new(0, GridUnitType.Pixel);

    Face? _selectedFace;

    bool _showWireFrame;

    MeshNode? _faceNormals;

    bool _geometryChanged;

    Edge? _selectedEdge;

    public ObservableCollection<ModelTreeItem> ModelTreeItems { get; } = [];
    
    public ObservableCollection<ModelTreeItem> SelectedTreeItems { get; } = [];

    public bool ShowVertices
    {
        get => _showVertices;
        set
        {
            if (SetProperty(ref _showVertices, value)) Update();
        }
    }

    public bool ShowEdges
    {
        get => _showEdges;
        set
        {
            if (SetProperty(ref _showEdges, value)) Update();
        }
    }

    public bool ShowFaces
    {
        get => _showFaces;
        set
        {
            if (SetProperty(ref _showFaces, value)) Update();
        }
    }

    public bool ShowIsoCurves
    {
        get => _showIsoCurves;
        set
        {
            if (SetProperty(ref _showIsoCurves, value)) Update();
        }
    }

    public bool ShowVerticesNames
    {
        get => _showVerticesNames;
        set
        {
            if (SetProperty(ref _showVerticesNames, value)) Update();
        }
    }

    public bool ShowFaceNormals
    {
        get => _showFaceNormals;
        set
        {
            if (SetProperty(ref _showFaceNormals, value))
            {
                if (_showFaceNormals)
                    AddFaceNormalsNode();
                else
                {
                    _faceNormals?.RemoveSelf();
                    _faceNormals = null;
                }
            }
        }
    }

    void AddFaceNormalsNode()
    {
        if (_solid == null || _solid.Faces.Count == 0) return;

        var builder = new MeshBuilder();
        var material = new DiffuseMaterial
        {
            DiffuseColor = Color.Orange
        };
        foreach (Face face in _solid!.Faces)
        {
            Xyz point = face.GetPointOnFace();
            var p0 = point.ToVector3();
            double area = face.CalculateArea(0.1);
            double arrowLength = area / 25;
            Vector3 p1 = p0 + (face.GetNormal(point) * arrowLength).ToVector3();
            builder.AddArrow(p0, p1, arrowLength / 8);
        }
        _faceNormals = new MeshNode
        {
            Geometry = builder.ToMesh()!,
            Material = material
        };
        ModelSpaceRootSceneNode.AddNode(_faceNormals);
    }

    void OnGeometryChanged()
    {
        _faceNormals?.RemoveSelf();
        if (ShowFaceNormals)
            AddFaceNormalsNode();
    }

    public RelayCommand CreateVertexCommand { get; }

    public RelayCommand CreateEdgesCommand { get; }

    public RelayCommand CreateFaceCommand { get; }

    public RelayCommand CreateBoxCommand { get; }

    public RelayCommand CreateSphereCommand { get; }

    public RelayCommand CreateCylinderCommand { get; }

    public RelayCommand CreateConeCommand { get; }

    public RelayCommand CreateTorusCommand { get; }

    public RelayCommand LineLineIntersectionCommand { get; }

    public RelayCommand LinePlaneIntersectionCommand { get; }

    public RelayCommand<System.Windows.Point?> SelectEntityCommand { get; }

    public EffectsManager EffectsManager { get; } = new DefaultEffectsManager();

    public SceneNodeGroupModel3D ModelSpaceRootSceneNode { get; } = new();

    public SceneNodeGroupModel3D ParametricSpaceRootSceneNode { get; } = new();

    public Camera Camera { get; }

    public double Zoom
    {
        get
        {
            if (Camera is OrthographicCamera orthographicCamera)
                return _view.ViewportWidth / orthographicCamera.Width;
            return double.NaN;
        }
    }

    public RelayCommand Zoom1_1Command { get; }

    /// <summary>
    /// Chord height in screen pixels.
    /// </summary>
    public double ChordHeight
    {
        get => _chordHeight;
        set
        {
            if (SetProperty(ref _chordHeight, value)) Update();
        }
    }

    double ChordHeightInWorldUnits => ChordHeight / Zoom;

    public bool ShowBoundingBox
    {
        get => _showBoundingBox;
        set => SetProperty(ref _showBoundingBox, value);
    }

    public RelayCommand CloseParametricSpacePaneCommand { get; }

    public RelayCommand ClearSelectionCommand { get; }

    public GridLength ParametricSpacePaneColumnWidth
    {
        get => _parametricSpacePaneColumnWidth;
        set => SetProperty(ref _parametricSpacePaneColumnWidth, value);
    }

    public ObservableCollection<Face> Faces { get; } = [];

    public Face? SelectedFace
    {
        get => _selectedFace;
        set => SetProperty(ref _selectedFace, value);
    }

    public ObservableCollection<Edge> Edges { get; } = [];

    public Edge? SelectedEdge
    {
        get => _selectedEdge;
        set => SetProperty(ref _selectedEdge, value);
    }

    public bool ShowWireFrame
    {
        get => _showWireFrame;
        set
        {
            if (SetProperty(ref _showWireFrame, value))
            {
                Update();
            }
        }
    }

    public MainViewModel(IView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        _view = view;

        Camera = InitializeCamera();

        _flamaFont = new BitmapFont();
        _flamaFont.Load(@"Fonts\Flama.fnt");
        _fontTexture = TextureModel.Create(@"Fonts\Flama.png")!;

        CreateVertexCommand = new RelayCommand(CreateVertex);
        CreateEdgesCommand = new RelayCommand(CreateEdges);
        CreateFaceCommand = new RelayCommand(CreateFace);
        CreateBoxCommand = new RelayCommand(CreateBox);
        CreateSphereCommand = new RelayCommand(CreateSphere);
        CreateCylinderCommand = new RelayCommand(CreateCylinder);
        CreateConeCommand = new RelayCommand(CreateCone);
        CreateTorusCommand = new RelayCommand(CreateTorus);

        LineLineIntersectionCommand = new RelayCommand(LineLineIntersection);
        LinePlaneIntersectionCommand = new RelayCommand(LinePlaneIntersection);

        Zoom1_1Command = new RelayCommand(Zoom1_1);

        CloseParametricSpacePaneCommand = new RelayCommand(HideParametricSpacePane);
        ClearSelectionCommand = new RelayCommand(ClearAllSelections);
        SelectEntityCommand = new RelayCommand<System.Windows.Point?>(SelectEntityAt3D);
    }

    void Zoom1_1()
    {
        if (Camera is OrthographicCamera orthographicCamera)
            orthographicCamera.Width = _view.ViewportWidth;
        else
            throw new InvalidOperationException("Can not set zoom for non-orthographic camera.");
    }

    OrthographicCamera InitializeCamera()
    {
        var lookDirection = new Vector3D(-1, -1, -1);
        lookDirection.Normalize();
        var camera = new OrthographicCamera
        {
            Position = new Point3D(1, 1, 1),
            LookDirection = lookDirection,
            UpDirection = new Vector3D(0, 0, 1),
            NearPlaneDistance = -1000,
            FarPlaneDistance = 1000
        };
        camera.Changed += CameraOnChanged;
        return camera;
    }

    void CameraOnChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Zoom));
    }

    void CreateVertex()
    {
        _solid = null;
        ShowVertices = true;

        _solid = new Solid();
        _solid.AddVertex(new Xyz(1, 2, 3));

        _geometryChanged = true;

        HideParametricSpacePane();
        Update();
        _view.ZoomExtents();
    }

    void CreateEdges()
    {
        _solid = null;
        ShowEdges = true;

        _solid = new Solid();
        Vertex start = _solid.AddVertex(new Xyz(1, 2, 3));
        Vertex end = _solid.AddVertex(new Xyz(6, 5, 4));
        _solid.AddEdge(start, end);

        var circle = new Circle(new Xyz(12, 10, 7), Xyz.ZAxis, 2.5);
        _solid.AddCircularEdge(circle);

        _geometryChanged = true;
        HideParametricSpacePane();
        Update();
        _view.ZoomExtents();
    }

    void HideParametricSpacePane()
    {
        _lastParametricSpacePaneColumnWidth = ParametricSpacePaneColumnWidth;
        ParametricSpacePaneColumnWidth = new GridLength(0, GridUnitType.Pixel);
    }

    void ShowParametricSpacePane()
    {
        ParametricSpacePaneColumnWidth = _lastParametricSpacePaneColumnWidth is { IsStar: true, Value: > 0 }
            ? _lastParametricSpacePaneColumnWidth
            : new GridLength(1, GridUnitType.Star);
    }

    void CreateFace()
    {
        _solid = null;
        ShowFaces = true;
        ShowWireFrame = true;

        _solid = new Solid();

        // Draw a T-shape face
        _solid.AddPlanarFace([
            Xyz.Zero,
            new Xyz(1, 0, 0),
            new Xyz(1, 4, 0),
            new Xyz(3, 4, 0),
            new Xyz(3, 5, 0),
            new Xyz(-2, 5, 0),
            new Xyz(-2, 4, 0),
            new Xyz(0, 4, 0)
        ]);

        _geometryChanged = true;
        ShowParametricSpacePane();
        Update();
        _view.ZoomExtents();
    }

    void CreateBox()
    {
        _solid = null;
        ShowFaces = true;
        ShowWireFrame = false;

        _solid = Solid.CreateBox(size: 10);

        _geometryChanged = true;
        ShowParametricSpacePane();
        Update();
        _view.ZoomExtents();
    }

    void CreateSphere()
    {
        _solid = null;
        ShowFaces = true;
        ShowWireFrame = true;

        _solid = Solid.CreateSphere(radius: 10);

        _geometryChanged = true;
        ShowParametricSpacePane();
        Update();
        _view.ZoomExtents();
    }

    void CreateCylinder()
    {
        _solid = null;
        ShowFaces = true;

        _solid = Solid.CreateCylinder(radius: 10, height: 20);
        _geometryChanged = true;

        ShowParametricSpacePane();
        Update();
        _view.ZoomExtents();
    }

    void CreateCone()
    {
        _solid = null;
        ShowFaces = true;

        _solid = new Solid();
        throw new NotImplementedException();

        ShowParametricSpacePane();
        Update();
        _view.ZoomExtents();
    }

    void CreateTorus()
    {
        _solid = null;
        ShowFaces = true;

        _solid = new Solid();
        throw new NotImplementedException();

        ShowParametricSpacePane();
        Update();
        _view.ZoomExtents();
    }

    void LineLineIntersection()
    {
        _solid = null;
        ShowVertices = true;
        ShowEdges = true;

        _solid = new Solid();
        Vertex start0 = _solid.AddVertex(Xyz.Zero);
        Vertex end0 = _solid.AddVertex(new Xyz(24, 24, 24));
        Edge edge0 = _solid.AddEdge(start0, end0);
        Vertex start1 = _solid.AddVertex(new Xyz(24, 0, 0));
        Vertex end1 = _solid.AddVertex(new Xyz(0, 24, 24));
        Edge edge1 = _solid.AddEdge(start1, end1);
        var intersector = new CurveCurveIntersector(edge0.GetCurve(), edge1.GetCurve());
        foreach (CurveCurveIntersectionResult result in intersector.GetIntersections())
        {
            switch (result)
            {
                case PointCurveCurveIntersectionResult pointIntersection:
                    {
                        _solid.AddVertex(pointIntersection.Point);
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        _geometryChanged = true;

        HideParametricSpacePane();
        Update();
        _view.ZoomExtents();
    }

    void LinePlaneIntersection()
    {
        _solid = null;
        ShowVertices = true;
        ShowEdges = true;
        ShowFaces = true;

        _solid = new Solid();

        Face face = _solid.AddPlanarFace([new Xyz(-10, -10, 0), new Xyz(10, -10, 0), new Xyz(10, 10, 0), new Xyz(-10, 10, 0)]);
        Vertex start = _solid.AddVertex(new Xyz(0, -6, -10));
        Vertex end = _solid.AddVertex(new Xyz(2, 3, 10));
        Edge edge = _solid.AddEdge(start, end);
        var intersector = new CurveSurfaceIntersector(edge.GetCurve(), face.Surface);
        foreach (CurveSurfaceIntersectionResult result in intersector.GetIntersections())
        {
            switch (result)
            {
                case PointCurveSurfaceIntersectionResult pointIntersection:
                    {
                        _solid.AddVertex(pointIntersection.Point);
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        _geometryChanged = true;

        HideParametricSpacePane();
        Update();
        _view.ZoomExtents();
    }

    void Update()
    {
        if (_solid is null)
        {
            ModelTreeItems.Clear();
            return;
        }

        if (_geometryChanged)
        {
            _geometryChanged = false;
            OnGeometryChanged();
        }

        UpdateModelSpace();
        UpdateParametricSpace();
        UpdateModelTree();
    }

    void UpdateParametricSpace()
    {
        ReloadFaces();
    }

    void UpdateModelSpace()
    {
        ModelSpaceRootSceneNode.Clear();
        _faceNormals = null;
        ParametricSpaceRootSceneNode.Clear();

        _modelSpaceLabel = null;
        if (ShowVerticesNames) AddNamesToScene();
        if (ShowVertices) AddVerticesToScene();
        if (ShowEdges) AddEdgesToScene();
        if (ShowFaces) AddFacesToScene();
    }

    void ReloadFaces()
    {
        if (_solid == null) return;
        foreach (Face face in _solid.Faces)
        {
            if (!Faces.Contains(face))
            {
                int index = _solid.Faces.TakeWhile(f => f.Id < face.Id).Count();
                Faces.Insert(index, face);
            }
        }
        for (int i = Faces.Count - 1; i >= 0; i--)
        {
            if (!_solid.Faces.Contains(Faces[i]))
                Faces.RemoveAt(i);
        }
        SelectedFace = Faces.Count > 0 ? Faces[0] : null;
    }

    void AddVerticesToScene()
    {
        if (_solid == null) throw new NullReferenceException($"{nameof(_solid)} is null.");

        var positions = new Vector3Collection(_solid.Vertices.Count);
        var colors = new Color4Collection(_solid.Vertices.Count);
        
        foreach (Vertex vertex in _solid.Vertices)
        {
            positions.Add(new Vector3((float)vertex.Position.X, (float)vertex.Position.Y, (float)vertex.Position.Z));
            colors.Add(IsEntitySelected(vertex) ? Color.Blue : Color4.Black);
        }
        
        var pointGeometry = new PointGeometry3D
        {
            Positions = positions,
            Colors = colors
        };
        var pointMaterial = new PointMaterialCore
        {
            Figure = PointFigure.Rect,
            Height = 5,
            Width = 5,
            EnableColorBlending = true,
            BlendingFactor = 1,
            PointColor = Color4.White
        };
        var pointNode = new PointNode
        {
            Material = pointMaterial,
            Geometry = pointGeometry,
            DepthBias = -100, // To see the points in front of the triangle faces
            HitTestThickness = 5
        };
        foreach (Vertex vertex in _solid.Vertices)
        {
            vertex.Tag = pointNode;
        }
        ModelSpaceRootSceneNode.AddNode(pointNode);

        if (_modelSpaceLabel != null)
        {
            foreach (Vertex vertex in _solid.Vertices)
            {
                var modelSpaceTextInfo = new TextInfo(vertex.ToString(), vertex.Position.ToVector3())
                {
                    HorizontalAlignment = BillboardHorizontalAlignment.Center,
                    VerticalAlignment = BillboardVerticalAlignment.Center,
                    Offset = new Vector2(15f, 15f)
                };
                _modelSpaceLabel.TextInfo!.Add(modelSpaceTextInfo);
            }
        }
    }

    void AddEdgesToScene()
    {
        if (_solid == null) throw new NullReferenceException($"{nameof(_solid)} is null.");

        foreach (Edge edge in _solid.Edges)
        {
            var builder = new LineBuilder();
            switch (edge.Curve)
            {
                case null:
                    {
                        var start = edge.StartVertex.Position.ToVector3();
                        var end = edge.EndVertex.Position.ToVector3();
                        builder.AddLine(start, end);
                        break;
                    }

                case Circle circle:
                    IList<Xyz> strokePoints = circle.GetStrokePoints(ChordHeightInWorldUnits);
                    builder.Add(isClosed: true, strokePoints.Select(p => p.ToVector3()).ToArray());
                    break;
            }
            var material = new LineMaterialCore
            {
                LineColor = IsEntitySelected(edge) ? Color.Blue : Color.Gray
            };
            var node = new LineNode
            {
                Material = material,
                Geometry = builder.ToLineGeometry3D()!,
                HitTestThickness = 5
            };
            edge.Tag = node;
            ModelSpaceRootSceneNode.AddNode(node);
        }
    }

    void AddFacesToScene()
    {
        if (_solid == null) throw new NullReferenceException($"{nameof(_solid)} is null.");

        foreach (Face face in _solid.Faces)
        {
            var builder = new MeshBuilder();
            var mesh = new Mesh(ChordHeightInWorldUnits);
            var tempSolid = new Solid();
            tempSolid.Faces.Add(face);
            mesh.AddSolid(tempSolid);
            builder.Positions!.AddRange(mesh.Positions.Select(p => p.ToVector3()));
            foreach (int[] indices in mesh.TriangleIndices.Values) builder.TriangleIndices!.AddRange(indices);
            builder.Normals!.AddRange(mesh.Normals.Select(n => n.ToVector3()));
            var material = new DiffuseMaterial
            {
                DiffuseColor = IsEntitySelected(face) ? Color.Blue : Color.LightBlue
            };
            var node = new MeshNode
            {
                Geometry = builder.ToMesh()!,
                Material = material,
                RenderWireframe = ShowWireFrame,
                CullMode = CullMode.Back
            };
            face.Tag = node;
            ModelSpaceRootSceneNode.AddNode(node);
        }
    }

    void AddNamesToScene()
    {
        _modelSpaceLabel = new BillboardText3D(_flamaFont, _fontTexture);
        var billboardMaterial = new BillboardMaterialCore
        {
            Type = BillboardType.MultipleText,
            FixedSize = true,
        };
        var billboardNode = new BillboardNode
        {
            Geometry = _modelSpaceLabel,
            Material = billboardMaterial,
            DepthBias = int.MinValue // To see the text in front of the geometry
        };
        ModelSpaceRootSceneNode.AddNode(billboardNode);
    }

    void UpdateModelTree()
    {
        // Sauvegarder les entités sélectionnées
        var selectedEntities = SelectedTreeItems.Select(item => item.Data).ToHashSet();
        
        ModelTreeItems.Clear();
        
        if (_solid == null) 
        {
            // Si pas de solide, vider aussi les sélections
            SelectedTreeItems.Clear();
            return;
        }

        var newSelectedItems = new List<ModelTreeItem>();

        var rootItem = new ModelTreeItem("Model");
        ModelTreeItems.Add(rootItem);

        if (_solid.Vertices.Count > 0)
        {
            var verticesItem = new ModelTreeItem($"Vertices ({_solid.Vertices.Count})");
            rootItem.Children.Add(verticesItem);

            foreach (Vertex vertex in _solid.Vertices)
            {
                var vertexItem = new ModelTreeItem($"Vertex {vertex.Id}: {vertex.Position}", vertex);
                verticesItem.Children.Add(vertexItem);
                
                // Si cette entité était sélectionnée, l'ajouter à la nouvelle liste
                if (selectedEntities.Contains(vertex))
                {
                    newSelectedItems.Add(vertexItem);
                    vertexItem.IsSelected = true;
                }
            }
        }

        if (_solid.Edges.Count > 0)
        {
            var edgesItem = new ModelTreeItem($"Edges ({_solid.Edges.Count})");
            rootItem.Children.Add(edgesItem);

            foreach (Edge edge in _solid.Edges)
            {
                string edgeDescription = edge.Curve switch
                {
                    null => $"Line: {edge.StartVertex.Position} → {edge.EndVertex.Position}",
                    Circle circle => $"Circle: Center={circle.Center}, Radius={circle.Radius:F2}",
                    _ => edge.Curve.GetType().Name
                };
                var edgeItem = new ModelTreeItem($"Edge {edge.Id}: {edgeDescription}", edge);
                edgesItem.Children.Add(edgeItem);
                
                // Si cette entité était sélectionnée, l'ajouter à la nouvelle liste
                if (selectedEntities.Contains(edge))
                {
                    newSelectedItems.Add(edgeItem);
                    edgeItem.IsSelected = true;
                }
            }
        }

        if (_solid.Faces.Count > 0)
        {
            var facesItem = new ModelTreeItem($"Faces ({_solid.Faces.Count})");
            rootItem.Children.Add(facesItem);

            foreach (Face face in _solid.Faces)
            {
                string surfaceDescription = face.Surface.GetType().Name;
                var faceItem = new ModelTreeItem($"Face {face.Id}: {surfaceDescription}", face);
                facesItem.Children.Add(faceItem);
                
                // Si cette entité était sélectionnée, l'ajouter à la nouvelle liste
                if (selectedEntities.Contains(face))
                {
                    newSelectedItems.Add(faceItem);
                    faceItem.IsSelected = true;
                }
            }
        }
        
        // Synchroniser SelectedTreeItems avec newSelectedItems
        SynchronizeSelectedItems(newSelectedItems);
    }
    
    void SynchronizeSelectedItems(List<ModelTreeItem> newSelectedItems)
    {
        // Supprimer les éléments qui ne sont plus sélectionnés
        for (int i = SelectedTreeItems.Count - 1; i >= 0; i--)
        {
            if (!newSelectedItems.Contains(SelectedTreeItems[i]))
            {
                SelectedTreeItems.RemoveAt(i);
            }
        }
        
        // Ajouter les nouveaux éléments sélectionnés
        foreach (ModelTreeItem newItem in newSelectedItems)
        {
            if (!SelectedTreeItems.Contains(newItem))
            {
                SelectedTreeItems.Add(newItem);
            }
        }
        
        // Mettre à jour les visuels après la synchronisation
        UpdateSelectionVisuals();
    }

    void ClearAllSelections()
    {
        foreach (ModelTreeItem item in ModelTreeItems)
        {
            ClearSelectionRecursively(item);
        }
        SelectedTreeItems.Clear();
        UpdateSelectionVisuals();
    }

    void SelectEntityAt3D(System.Windows.Point? mousePosition)
    {
        if (mousePosition == null || _solid == null) 
        {
            ClearAllSelections();
            return;
        }

        // Perform hit testing on the 3D viewport
        IList<HitTestResult>? hits = _view.HitTest(mousePosition.Value);
        if (hits.Count == 0) 
        {
            ClearAllSelections();
            return;
        }

        // Find the first hit that corresponds to a TGK entity
        bool entityFound = false;
        foreach (HitTestResult hit in hits)
        {
            if (hit.ModelHit is SceneNode node)
            {
                // Find the corresponding TGK entity
                object? tgkEntity = FindTgkEntityFromNode(node, hit.Tag);
                if (tgkEntity != null)
                {
                    SelectEntity(tgkEntity);
                    entityFound = true;
                    break;
                }
            }
        }

        // If no TGK entity was found, clear selection
        if (!entityFound)
        {
            ClearAllSelections();
        }
    }

    object? FindTgkEntityFromNode(SceneNode node, object? hitTag)
    {
        // Check if this node corresponds to a vertex
        foreach ((Vertex v, int i) in _solid!.Vertices.Select((v, i) => (v, i)))
        {
            if (v.Tag == node && i == (int)hitTag!) return v;
        }

        // Check if this node corresponds to an edge
        foreach (Edge edge in _solid.Edges)
        {
            if (edge.Tag == node) return edge;
        }

        // Check if this node corresponds to a face
        foreach (Face face in _solid.Faces)
        {
            if (face.Tag == node) return face;
        }

        return null;
    }

    void SelectEntity(object entity)
    {
        // Clear all current selections first
        ClearAllSelections();

        // Find the corresponding ModelTreeItem
        ModelTreeItem? itemToSelect = FindModelTreeItem(entity);
        if (itemToSelect != null)
        {
            // Add the new item to selection
            SelectedTreeItems.Add(itemToSelect);
            
            // Update visual selection in the tree
            itemToSelect.IsSelected = true;
            
            // Expand parent nodes to make the selected item visible
            _view.ExpandParentNodesForSelectedItem(itemToSelect);
            
            // Force update of visual selection in 3D view
            UpdateSelectionVisuals();
        }
    }

    ModelTreeItem? FindModelTreeItem(object entity)
    {
        foreach (ModelTreeItem rootItem in ModelTreeItems)
        {
            ModelTreeItem? found = FindModelTreeItemRecursively(rootItem, entity);
            if (found != null) return found;
        }
        return null;
    }

    static ModelTreeItem? FindModelTreeItemRecursively(ModelTreeItem item, object entity)
    {
        if (item.Data == entity) return item;

        foreach (ModelTreeItem child in item.Children)
        {
            ModelTreeItem? found = FindModelTreeItemRecursively(child, entity);
            if (found != null) return found;
        }

        return null;
    }

    static void ClearSelectionRecursively(ModelTreeItem item)
    {
        item.IsSelected = false;
        foreach (ModelTreeItem child in item.Children)
        {
            ClearSelectionRecursively(child);
        }
    }

    bool IsEntitySelected(BRepEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return SelectedTreeItems.Any(item => item.Data == entity);
    }

    void UpdateSelectionVisuals()
    {
        UpdateVerticesColors();
        UpdateEdgeColors();
        UpdateFaceColors();
    }

    void UpdateVerticesColors()
    {
        if (_solid == null || !ShowVertices) return;

        // Trouver le PointNode des vertices dans la scène
        PointNode? pointNode = ModelSpaceRootSceneNode.GroupNode!.Items!.OfType<PointNode>().FirstOrDefault();

        if (pointNode == null) return;
        var geometry = (PointGeometry3D)pointNode.Geometry!;
        foreach ((Vertex vertex, int i) in _solid.Vertices.Select((vertex, index) => (vertex, index)))
        {
            geometry.Colors![i] = IsEntitySelected(vertex) ? Color.Blue : Color4.Black;
        }
        geometry.UpdateColors();
    }

    void UpdateEdgeColors()
    {
        if (_solid == null || !ShowEdges) return;

        foreach (Edge edge in _solid.Edges)
        {
            if (((MaterialGeometryNode)edge.Tag!).Material is LineMaterialCore material)
            {
                material.LineColor = IsEntitySelected(edge) ? Color.Blue : Color.Gray;
            }
        }
    }

    void UpdateFaceColors()
    {
        if (_solid == null || !ShowFaces) return;

       foreach (Face face in _solid.Faces)
        {
            if (((MaterialGeometryNode)face.Tag!).Material is DiffuseMaterialCore material)
            {
                material.DiffuseColor = IsEntitySelected(face) ? Color.Blue : Color.LightBlue;
            }
        }
    }
}