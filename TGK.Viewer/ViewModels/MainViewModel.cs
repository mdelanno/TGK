using CommunityToolkit.Mvvm.Input;
using Cyotek.Drawing.BitmapFont;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D11;
using System.Windows;
using System.Windows.Media.Media3D;
using TGK.FaceterServices;
using TGK.Geometry;
using TGK.Geometry.Curves;
using TGK.Topology;
using TGK.Viewer.Services;
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
    const double CURVE_UPDATE_THROTTLE_SECONDS = 1.0;

    readonly BitmapFont _flamaFont;

    readonly IView _view;

    readonly ISelectionService _selectionService;

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

    LineNode? _curveLineNode;

    readonly List<Edge> _curveEdges = [];

    double _lastZoom = double.NaN;

    DateTime _lastCurveUpdateTime = DateTime.MinValue;

    public ModelTreeViewModel ModelTree { get; }

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

    public Camera ParametricSpaceCamera { get; }

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

    public Face? SelectedFace
    {
        get => _selectedFace;
        set => SetProperty(ref _selectedFace, value);
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

    public MainViewModel(IView view, ISelectionService selectionService)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(selectionService);

        _view = view;
        _selectionService = selectionService;
        ModelTree = new ModelTreeViewModel(_selectionService);

        Camera = InitializeModelSpaceCamera();
        ParametricSpaceCamera = InitializeParametricSpaceCamera();

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
        ClearSelectionCommand = new RelayCommand(() => ModelTree.ClearSelectionCommand.Execute(null));
        SelectEntityCommand = new RelayCommand<System.Windows.Point?>(SelectEntityAt3D);

        // Subscribe to selection changes
        _selectionService.SelectionChanged += OnSelectionChanged;
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

    void OnSelectionChanged(IEnumerable<BRepEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        UpdateSelectionVisuals();
        ParametricSpaceRootSceneNode.Clear();
        Face? selectedFace = entities.OfType<Face>().FirstOrDefault();
        if (selectedFace != null)
        {
            AddFaceToParametricSpace(selectedFace);
            _view.ParametricSpaceZoomExtents();
        }
    }

    void Zoom1_1()
    {
        if (Camera is OrthographicCamera orthographicCamera)
            orthographicCamera.Width = _view.ViewportWidth;
        else
            throw new InvalidOperationException("Can not set zoom for non-orthographic camera.");
    }

    OrthographicCamera InitializeModelSpaceCamera()
    {
        var lookDirection = new Vector3D(-1, -1, -1);
        lookDirection.Normalize();
        var camera = new OrthographicCamera
        {
            Position = new Point3D(1, 1, 1),
            LookDirection = lookDirection,
            UpDirection = new Vector3D(0, 0, 1),
            NearPlaneDistance = -1000,  // Safe default values until model is loaded
            FarPlaneDistance = 1000
        };
        camera.Changed += CameraOnChanged;
        return camera;
    }

    static OrthographicCamera InitializeParametricSpaceCamera()
    {
        // Vue de dessus (plan XY) — regarder vers le bas selon l'axe Z
        var camera = new OrthographicCamera
        {
            Position = new Point3D(0, 0, 10), // Position au-dessus du plan XY
            LookDirection = new Vector3D(0, 0, -1), // Regarder vers le bas
            UpDirection = new Vector3D(0, 1, 0), // Y vers le haut
            Width = 10, // Largeur initiale
            NearPlaneDistance = 0.1,
            FarPlaneDistance = 100
        };
        return camera;
    }

    void UpdateClippingPlanes()
    {
        if (Camera is not OrthographicCamera orthographicCamera)
            return;

        var boundingBox = _view.CalculateModelSpaceBoundingBox();

        if (boundingBox.Size.Length() == 0.0)
        {
            // Conservative values for empty model
            orthographicCamera.NearPlaneDistance = -100;
            orthographicCamera.FarPlaneDistance = 100;
            return;
        }

        // Simple conservative approach: use model center and size
        var modelCenter = new Xyz(boundingBox.Center.X, boundingBox.Center.Y, boundingBox.Center.Z);
        var cameraPosition = new Xyz(orthographicCamera.Position.X, orthographicCamera.Position.Y, orthographicCamera.Position.Z);
        var lookDirection = new Xyz(orthographicCamera.LookDirection.X, orthographicCamera.LookDirection.Y, orthographicCamera.LookDirection.Z).ToUnit();

        // Distance from camera to model center along view direction
        var vectorToCenter = modelCenter - cameraPosition;
        double centerDistance = vectorToCenter.DotProduct(lookDirection);

        // Use very conservative margins: model size on each side plus extra safety
        double modelSize = boundingBox.Size.Length();
        double safetyMargin = Math.Max(modelSize * 2.0, 10.0);
        
        double nearPlane = centerDistance - safetyMargin;
        double farPlane = centerDistance + safetyMargin;

        orthographicCamera.NearPlaneDistance = nearPlane;
        orthographicCamera.FarPlaneDistance = farPlane;
    }

    void CameraOnChanged(object? sender, EventArgs e)
    {
        double currentZoom = Zoom;
        bool zoomChanged = !double.IsNaN(_lastZoom) && !currentZoom.IsAlmostEqualTo(_lastZoom, 1e-6);

        OnPropertyChanged(nameof(Zoom));

        // Update curve detail level if zoom changed
        if (zoomChanged && _solid != null && _curveEdges.Count > 0)
        {
            DateTime now = DateTime.Now;
            if ((now - _lastCurveUpdateTime).TotalSeconds >= CURVE_UPDATE_THROTTLE_SECONDS)
            {
                UpdateCurveDetailLevel();
                _lastCurveUpdateTime = now;
            }
        }

        _lastZoom = currentZoom;
    }

    void CreateVertex()
    {
        _solid = new Solid();
        _solid.AddVertex(new Xyz(1, 2, 3));

        _geometryChanged = true;

        HideParametricSpacePane();
        Update();
        _view.ModelSpaceZoomExtents();
    }

    void CreateEdges()
    {
        _solid = new Solid();
        Vertex start = _solid.AddVertex(new Xyz(1, 2, 3));
        Vertex end = _solid.AddVertex(new Xyz(6, 5, 4));
        _solid.AddEdge(start, end);

        var circle = new Circle(new Xyz(12, 10, 7), Xyz.ZAxis, 2.5);
        _solid.AddCircularEdge(circle);

        _geometryChanged = true;
        HideParametricSpacePane();
        Update();
        _view.ModelSpaceZoomExtents();
    }

    void HideParametricSpacePane()
    {
        _lastParametricSpacePaneColumnWidth = ParametricSpacePaneColumnWidth;
        ParametricSpacePaneColumnWidth = new GridLength(0, GridUnitType.Pixel);
    }

    void ShowParametricSpacePane()
    {
        ParametricSpacePaneColumnWidth = _lastParametricSpacePaneColumnWidth is { IsStar: true, Value: > 0 } ? _lastParametricSpacePaneColumnWidth : new GridLength(1, GridUnitType.Star);
    }

    void CreateFace()
    {
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
        _view.ModelSpaceZoomExtents();
    }

    void CreateBox()
    {
        _solid = Solid.CreateBox(size: 10);

        _geometryChanged = true;
        ShowParametricSpacePane();
        Update();
        _view.ModelSpaceZoomExtents();
    }

    void CreateSphere()
    {
        _solid = Solid.CreateSphere(radius: 10);

        _geometryChanged = true;
        ShowParametricSpacePane();
        Update();
        _view.ModelSpaceZoomExtents();
    }

    void CreateCylinder()
    {
        _solid = Solid.CreateCylinder(radius: 10, height: 20);
        _geometryChanged = true;

        ShowParametricSpacePane();
        Update();
        _view.ModelSpaceZoomExtents();
    }

    void CreateCone()
    {
        _solid = new Solid();
        throw new NotImplementedException();

        ShowParametricSpacePane();
        Update();
        _view.ModelSpaceZoomExtents();
    }

    void CreateTorus()
    {
        _solid = new Solid();
        throw new NotImplementedException();

        ShowParametricSpacePane();
        Update();
        _view.ModelSpaceZoomExtents();
    }

    void LineLineIntersection()
    {
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
        _view.ModelSpaceZoomExtents();
    }

    void LinePlaneIntersection()
    {
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
        _view.ModelSpaceZoomExtents();
    }

    void Update()
    {
        if (_geometryChanged)
        {
            _geometryChanged = false;
            OnGeometryChanged();
        }

        UpdateModelSpace();
        UpdateParametricSpace();
        ModelTree.UpdateTree(_solid);
        
        // Update clipping planes when model changes
        UpdateClippingPlanes();
    }

    void UpdateParametricSpace()
    {
        ParametricSpaceRootSceneNode.Clear();

        // Afficher la face sélectionnée dans l'espace paramétrique
        Face? selectedFace = _selectionService.SelectedEntities.OfType<Face>().FirstOrDefault();
        if (selectedFace != null) AddFaceToParametricSpace(selectedFace);
        _view.ParametricSpaceZoomExtents();
    }

    void AddFaceToParametricSpace(Face face)
    {
        ArgumentNullException.ThrowIfNull(face);

        try
        {
            AddFaceContourToParametricSpace(face);
            if (ShowFaces) AddFaceTriangulationToParametricSpace(face);
        }
        catch (NotImplementedException)
        {
            // Some surfaces have not yet been implemented for parametric projection
        }
    }

    void AddFaceContourToParametricSpace(Face face)
    {
        ArgumentNullException.ThrowIfNull(face);

        var mesh = new Mesh(ChordHeightInWorldUnits);
        List<Node> boundaryNodes = mesh.ProjectFaceBoundaryToParameterSpace(face);

        if (boundaryNodes.Count < 2) return;

        var builder = new LineBuilder();
        for (int i = 0; i < boundaryNodes.Count; i++)
        {
            int nextIndex = (i + 1) % boundaryNodes.Count;

            Uv currentUv = boundaryNodes[i].ParametricSpacePosition;
            Uv nextUv = boundaryNodes[nextIndex].ParametricSpacePosition;

            builder.AddLine(new Vector3((float)currentUv.U, (float)currentUv.V, 0.001f), new Vector3((float)nextUv.U, (float)nextUv.V, 0.001f));
        }

        var material = new LineMaterialCore
        {
            LineColor = Color.Red
        };

        var contourNode = new LineNode
        {
            Material = material,
            Geometry = builder.ToLineGeometry3D()!,
            HitTestThickness = 5,
            DepthBias = -100
        };

        ParametricSpaceRootSceneNode.AddNode(contourNode);
    }

    void AddFaceTriangulationToParametricSpace(Face face)
    {
        ArgumentNullException.ThrowIfNull(face);

        // Utiliser la méthode Mesh pour obtenir la triangulation et les nœuds en espace paramétrique
        var mesh = new Mesh(ChordHeightInWorldUnits);
        List<Node> boundaryNodes = mesh.ProjectFaceBoundaryToParameterSpace(face);

        if (boundaryNodes.Count < 3) return;

        // Utiliser les utilitaires de triangulation pour obtenir les indices des triangles
        var adapter = new NodeListAdapter();
        adapter.Set(boundaryNodes);
        int[] triangleIndices = TriangulationUtils.EarClipping(adapter);

        if (triangleIndices.Length == 0) return;

        var builder = new LineBuilder();

        // Dessiner les arêtes des triangles en utilisant directement les positions paramétriques
        for (int i = 0; i < triangleIndices.Length; i += 3)
        {
            int i1 = triangleIndices[i];
            int i2 = triangleIndices[i + 1];
            int i3 = triangleIndices[i + 2];

            Uv uv1 = boundaryNodes[i1].ParametricSpacePosition;
            Uv uv2 = boundaryNodes[i2].ParametricSpacePosition;
            Uv uv3 = boundaryNodes[i3].ParametricSpacePosition;

            Vector3 v1 = new Vector3((float)uv1.U, (float)uv1.V, 0.0f);
            Vector3 v2 = new Vector3((float)uv2.U, (float)uv2.V, 0.0f);
            Vector3 v3 = new Vector3((float)uv3.U, (float)uv3.V, 0.0f);

            // Dessiner les 3 arêtes du triangle
            builder.AddLine(v1, v2);
            builder.AddLine(v2, v3);
            builder.AddLine(v3, v1);
        }

        var material = new LineMaterialCore
        {
            LineColor = new Color4(0.7f, 0.7f, 0.7f, 1.0f), // Gris clair explicite
            Thickness = 1.0f
        };

        var triangulationNode = new LineNode
        {
            Material = material,
            Geometry = builder.ToLineGeometry3D()!,
            HitTestThickness = 1
        };

        ParametricSpaceRootSceneNode.AddNode(triangulationNode);
    }

    void UpdateModelSpace()
    {
        ModelSpaceRootSceneNode.Clear();
        _faceNormals = null;
        _curveLineNode = null;
        _curveEdges.Clear();
        _lastZoom = Zoom; // Réinitialiser le zoom de référence après reconstruction
        _lastCurveUpdateTime = DateTime.MinValue; // Permettre immédiatement la prochaine mise à jour des courbes
        ParametricSpaceRootSceneNode.Clear();

        _modelSpaceLabel = null;
        if (ShowVerticesNames) AddNamesToScene();
        if (ShowVertices) AddVerticesToScene();
        if (ShowEdges) AddEdgesToScene();
        if (ShowFaces) AddFacesToScene();
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

        var straightLineBuilder = new LineBuilder();
        var curveLineBuilder = new LineBuilder();
        var straightEdges = new List<Edge>();
        var curveEdges = new List<Edge>();

        foreach (Edge edge in _solid.Edges)
        {
            if (edge.IsPole) continue;

            switch (edge.Curve)
            {
                case null:
                    {
                        var start = edge.StartVertex.Position.ToVector3();
                        var end = edge.EndVertex.Position.ToVector3();
                        straightLineBuilder.AddLine(start, end);
                        straightEdges.Add(edge);
                        break;
                    }

                case Circle circle:
                    {
                        bool isFullCircle = edge.StartVertex.Position.IsAlmostEqualTo(edge.EndVertex.Position);
                        IList<Xyz> strokePoints;
                        if (isFullCircle)
                            strokePoints = circle.GetStrokePoints(ChordHeightInWorldUnits);
                        else
                        {
                            double startParameter = circle.GetParameterAtPoint(edge.StartVertex.Position);
                            double endParameter = circle.GetParameterAtPoint(edge.EndVertex.Position);
                            strokePoints = circle.GetStrokePoints(ChordHeightInWorldUnits, startParameter, endParameter);
                        }
                        curveLineBuilder.Add(isClosed: isFullCircle, strokePoints.Select(p => p.ToVector3()).ToArray());
                        curveEdges.Add(edge);
                        break;
                    }
            }
        }

        // Créer le LineNode pour les lignes droites
        if (straightEdges.Count > 0)
        {
            var straightLineMaterial = new LineMaterialCore
            {
                LineColor = Color.Gray
            };
            var straightLineNode = new LineNode
            {
                Material = straightLineMaterial,
                Geometry = straightLineBuilder.ToLineGeometry3D()!,
                HitTestThickness = 5
            };

            // Associer le node aux edges pour la sélection
            foreach (Edge edge in straightEdges)
            {
                edge.Tag = straightLineNode;
            }

            ModelSpaceRootSceneNode.AddNode(straightLineNode);
        }

        // Créer le LineNode pour les courbes
        if (curveEdges.Count > 0)
        {
            var curveLineMaterial = new LineMaterialCore
            {
                LineColor = Color.Gray
            };
            _curveLineNode = new LineNode
            {
                Material = curveLineMaterial,
                Geometry = curveLineBuilder.ToLineGeometry3D()!,
                HitTestThickness = 5
            };

            // Stocker la liste des courbes pour les mises à jour de niveau de détail
            _curveEdges.Clear();
            _curveEdges.AddRange(curveEdges);

            // Associer le node aux edges pour la sélection
            foreach (Edge edge in curveEdges)
            {
                edge.Tag = _curveLineNode;
            }

            ModelSpaceRootSceneNode.AddNode(_curveLineNode);
        }
        else
        {
            _curveLineNode = null;
            _curveEdges.Clear();
        }
    }

    void UpdateCurveDetailLevel()
    {
        if (_curveLineNode == null || _curveEdges.Count == 0) return;

        var curveLineBuilder = new LineBuilder();

        foreach (Edge edge in _curveEdges)
        {
            switch (edge.Curve)
            {
                case Circle circle:
                    {
                        bool isFullCircle = edge.StartVertex.Position.IsAlmostEqualTo(edge.EndVertex.Position);
                        IList<Xyz> strokePoints;
                        if (isFullCircle)
                            strokePoints = circle.GetStrokePoints(ChordHeightInWorldUnits);
                        else
                        {
                            double startParameter = circle.GetParameterAtPoint(edge.StartVertex.Position);
                            double endParameter = circle.GetParameterAtPoint(edge.EndVertex.Position);
                            strokePoints = circle.GetStrokePoints(ChordHeightInWorldUnits, startParameter, endParameter);
                        }
                        curveLineBuilder.Add(isClosed: isFullCircle, strokePoints.Select(p => p.ToVector3()).ToArray());
                        break;
                    }
            }
        }

        // Mettre à jour uniquement la géométrie du LineNode existant
        _curveLineNode.Geometry = curveLineBuilder.ToLineGeometry3D()!;
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


    void SelectEntityAt3D(System.Windows.Point? mousePosition)
    {
        if (mousePosition == null || _solid == null)
        {
            ModelTree.ClearSelectionCommand.Execute(null);
            return;
        }

        IList<HitTestResult>? hits = _view.HitTest(mousePosition.Value);
        if (hits == null || hits.Count == 0)
        {
            ModelTree.ClearSelectionCommand.Execute(null);
            return;
        }

        bool entityFound = false;
        foreach (HitTestResult hit in hits)
        {
            if (hit.ModelHit is SceneNode node)
            {
                BRepEntity? tgkEntity = FindTgkEntityFromNode(node, hit.Tag);
                if (tgkEntity != null)
                {
                    ModelTree.SelectEntity(tgkEntity);
                    var treeItem = ModelTree.FindItem(tgkEntity);
                    if (treeItem != null)
                        _view.ExpandParentNodesForSelectedItem(treeItem);
                    entityFound = true;
                    break;
                }
            }
        }

        if (!entityFound)
            ModelTree.ClearSelectionCommand.Execute(null);
    }

    BRepEntity? FindTgkEntityFromNode(SceneNode node, object? hitTag)
    {
        foreach ((Vertex v, int i) in _solid!.Vertices.Select((v, i) => (v, i)))
        {
            if (v.Tag == node && i == (int)hitTag!) return v;
        }

        foreach (Edge edge in _solid.Edges)
        {
            if (edge.Tag == node) return edge;
        }

        foreach (Face face in _solid.Faces)
        {
            if (face.Tag == node) return face;
        }

        return null;
    }

    bool IsEntitySelected(BRepEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return _selectionService.SelectedEntities.Contains(entity);
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
            if (edge.IsPole) continue;
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