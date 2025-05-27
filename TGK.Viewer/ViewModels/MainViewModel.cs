using CommunityToolkit.Mvvm.Input;
using Cyotek.Drawing.BitmapFont;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System.Windows.Media.Media3D;
using TGK.Geometry;
using TGK.Geometry.Curves;
using TGK.Topology;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using DiffuseMaterial = HelixToolkit.Wpf.SharpDX.DiffuseMaterial;
using EffectsManager = HelixToolkit.SharpDX.Core.EffectsManager;
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

    bool _showIsoCurve = true;

    bool _showNames;

    readonly TextureModel _fontTexture;

    BillboardText3D? _modelSpaceLabel;

    double _chordHeight = 0.1;

    bool _showFaceNormals;

    public bool ShowVertices
    {
        get => _showVertices;
        set
        {
            if (SetProperty(ref _showVertices, value)) RefreshScene();
        }
    }

    public bool ShowEdges
    {
        get => _showEdges;
        set
        {
            if (SetProperty(ref _showEdges, value)) RefreshScene();
        }
    }

    public bool ShowFaces
    {
        get => _showFaces;
        set
        {
            if (SetProperty(ref _showFaces, value)) RefreshScene();
        }
    }

    public bool ShowIsoCurve
    {
        get => _showIsoCurve;
        set
        {
            if (SetProperty(ref _showIsoCurve, value)) RefreshScene();
        }
    }

    public bool ShowNames
    {
        get => _showNames;
        set
        {
            if (SetProperty(ref _showNames, value)) RefreshScene();
        }
    }

    public bool ShowFaceNormals
    {
        get => _showFaceNormals;
        set
        {
            if (SetProperty(ref _showFaceNormals, value)) RefreshScene();
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

    public EffectsManager EffectsManager { get; } = new DefaultEffectsManager();

    public SceneNodeGroupModel3D RootSceneNode { get; } = new();

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
            if (SetProperty(ref _chordHeight, value)) RefreshScene();
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

        RefreshScene();
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

        var circle = new Circle(new Xyz(12, 10, 7), 2.5, Xyz.ZAxis);
        _solid.AddEdge(circle);

        RefreshScene();
        _view.ZoomExtents();
    }

    void CreateFace()
    {
        _solid = null;
        ShowFaces = true;

        _solid = new Solid();
        Vertex v0 = _solid.AddVertex(new Xyz(-10, -10, 0));
        Vertex v1 = _solid.AddVertex(new Xyz(10, -10, 0));
        Vertex v2 = _solid.AddVertex(new Xyz(10, 10, 0));
        Vertex v3 = _solid.AddVertex(new Xyz(-10, 10, 0));
        _solid.AddPlanarFace([v0, v1, v2, v3]);

        RefreshScene();
        _view.ZoomExtents();
    }

    void CreateBox()
    {
        _solid = null;
        ShowFaces = true;

        _solid = new Solid();
        Face face = _solid.AddPlanarFace([new Xyz(-10, -10, -5), new Xyz(10, -10, -5), new Xyz(10, 10, -5), new Xyz(-10, 10, -5)]);
        _solid.Extrude(face, new Xyz(0, 0, 20));

        RefreshScene();
        _view.ZoomExtents();
    }

    void CreateSphere()
    {
        _solid = null;
        ShowFaces = true;

        throw new NotImplementedException();

        RefreshScene();
        _view.ZoomExtents();
    }

    void CreateCylinder()
    {
        _solid = null;
        ShowFaces = true;

        _solid = new Solid();
        Face face = _solid.AddCircularFace(Xyz.Origin, 10.0, Xyz.ZAxis);
        _solid.Extrude(face, new Xyz(0, 0, 20));

        RefreshScene();
        _view.ZoomExtents();
    }

    void CreateCone()
    {
        _solid = null;
        ShowFaces = true;

        _solid = new Solid();
        throw new NotImplementedException();

        RefreshScene();
        _view.ZoomExtents();
    }

    void CreateTorus()
    {
        _solid = null;
        ShowFaces = true;

        _solid = new Solid();
        throw new NotImplementedException();

        RefreshScene();
        _view.ZoomExtents();
    }

    void LineLineIntersection()
    {
        _solid = null;
        ShowVertices = true;
        ShowEdges = true;

        _solid = new Solid();
        Vertex start0 = _solid.AddVertex(Xyz.Origin);
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

        RefreshScene();
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

        RefreshScene();
        _view.ZoomExtents();
    }

    void RefreshScene()
    {
        RootSceneNode.Clear();

        if (_solid is null)
            return;

        _modelSpaceLabel = null;
        if (ShowNames) AddNamesToScene();
        if (ShowVertices) AddVerticesToScene();
        if (ShowEdges) AddEdgesToScene();
        if (ShowFaces) AddFacesToScene();
    }

    void AddVerticesToScene()
    {
        if (_solid == null) throw new NullReferenceException($"{nameof(_solid)} is null.");

        var positions = new Vector3Collection(_solid.Vertices.Count);
        foreach (Vertex vertex in _solid.Vertices)
            positions.Add(new Vector3((float)vertex.Position.X, (float)vertex.Position.Y, (float)vertex.Position.Z));
        var pointGeometry = new PointGeometry3D
        {
            Positions = positions
        };
        var pointMaterial = new PointMaterialCore
        {
            PointColor = Color4.Black,
            Figure = PointFigure.Rect,
            Height = 5,
            Width = 5
        };
        var pointNode = new PointNode
        {
            Material = pointMaterial,
            Geometry = pointGeometry
        };
        RootSceneNode.AddNode(pointNode);

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

        var builder = new LineBuilder();
        foreach (Edge edge in _solid.Edges)
        {
            switch (edge.Curve)
            {
                case null:
                    {
                        var start = edge.Start!.Position.ToVector3();
                        var end = edge.End!.Position.ToVector3();
                        builder.AddLine(start, end);
                        break;
                    }

                case Circle circle:
                    double chordHeightInWorldUnits = ChordHeight / Zoom;
                    IList<Xyz> strokePoints = circle.GetStrokePoints(chordHeightInWorldUnits);
                    builder.Add(isClosed: true, strokePoints.Select(p => p.ToVector3()).ToArray());
                    break;
            }
        }
        var material = new LineMaterialCore
        {
            LineColor = Color.Blue
        };
        var node = new LineNode
        {
            Material = material,
            Geometry = builder.ToLineGeometry3D()!
        };
        RootSceneNode.AddNode(node);
    }

    void AddFacesToScene()
    {
        if (_solid == null) throw new NullReferenceException($"{nameof(_solid)} is null.");

        var builder = new MeshBuilder();
        foreach (Face face in _solid.Faces)
        {
            IEnumerable<Vertex> vertices = face.GetVertices();
            // TODO Handle non-straight edges.
            if (vertices.Any())
            {
                var points = vertices.Select(v => v.Position.ToVector3()).ToList();
                // TODO Use our own triangle builder.
                builder.AddPolygon(points);
            }

            if (ShowFaceNormals)
            {
                Xyz point = face.GetPointOnFace();
                var p0 = point.ToVector3();
                Vector3 p1 = p0 + (face.GetNormal(point) * 5).ToVector3();
                builder.AddArrow(p0, p1, 0.5);
            }
        }
        var material = new DiffuseMaterial
        {
            DiffuseColor = Color.LightBlue
        };
        var node = new MeshNode
        {
            Geometry = builder.ToMesh()!,
            Material = material
        };
        RootSceneNode.AddNode(node);
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
            //DepthBias = int.MinValue // To see the text in front of the geometry
        };
        RootSceneNode.AddNode(billboardNode);
    }
}