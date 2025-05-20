using CommunityToolkit.Mvvm.Input;
using Cyotek.Drawing.BitmapFont;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System.Windows.Media.Media3D;
using TGK.Geometry;
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

    public RelayCommand CreateVertexCommand { get; }

    public RelayCommand CreateEdgeCommand { get; }

    public RelayCommand CreateFaceCommand { get; }

    public RelayCommand CreateBoxCommand { get; }

    public RelayCommand CreateSphereCommand { get; }

    public RelayCommand CreateCylinderCommand { get; }

    public RelayCommand CreateConeCommand { get; }

    public RelayCommand CreateTorusCommand { get; }

    public EffectsManager EffectsManager { get; } = new DefaultEffectsManager();

    public SceneNodeGroupModel3D RootSceneNode { get; } = new();

    public Camera Camera { get; }

    public MainViewModel(IView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        _view = view;

        Camera = InitializeCamera();

        _flamaFont = new BitmapFont();
        _flamaFont.Load(@"Fonts\Flama.fnt");
        _fontTexture = TextureModel.Create(@"Fonts\Flama.png")!;

        CreateVertexCommand = new RelayCommand(CreateVertex);
        CreateEdgeCommand = new RelayCommand(CreateEdge);
        CreateFaceCommand = new RelayCommand(CreateFace);
        CreateBoxCommand = new RelayCommand(CreateBox);
        CreateSphereCommand = new RelayCommand(CreateSphere);
        CreateCylinderCommand = new RelayCommand(CreateCylinder);
        CreateConeCommand = new RelayCommand(CreateCone);
        CreateTorusCommand = new RelayCommand(CreateTorus);
    }

    static OrthographicCamera InitializeCamera()
    {
        var lookDirection = new Vector3D(-1, -1, -1);
        lookDirection.Normalize();
        var camera = new OrthographicCamera
        {
            Position = new Point3D(1, 1, 1),
            LookDirection = lookDirection,
            UpDirection = new Vector3D(0, 0, 1),
            NearPlaneDistance = -1000,
            FarPlaneDistance = 1000,
        };
        return camera;
    }

    void CreateVertex()
    {
        _solid = null;
        ShowVertices = true;

        _solid = new Solid();
        _solid.AddVertex(new Xyz(1, 2, 3));

        RefreshScene();
    }

    void CreateEdge()
    {
        _solid = null;
        ShowEdges = true;

        _solid = new Solid();
        Vertex start = _solid.AddVertex(new Xyz(1, 2, 3));
        Vertex edge = _solid.AddVertex(new Xyz(6, 5, 4));
        _solid.AddEdge(start, edge);

        RefreshScene();
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
        _solid.AddFace([v0, v1, v2, v3]);

        RefreshScene();
    }

    void CreateBox()
    {
        _solid = null;
        ShowFaces = true;

        _solid = new Solid();
        Vertex v0 = _solid.AddVertex(new Xyz(-10, -10, -5));
        Vertex v1 = _solid.AddVertex(new Xyz(10, -10, -5));
        Vertex v2 = _solid.AddVertex(new Xyz(10, 10, -5));
        Vertex v3 = _solid.AddVertex(new Xyz(-10, 10, -5));
        Face face = _solid.AddFace([v0, v1, v2, v3]);
        _solid.Extrude(face, new Xyz(0, 0, 20));

        RefreshScene();
    }

    void CreateSphere()
    {
        throw new NotImplementedException();
    }

    void CreateCylinder()
    {
        throw new NotImplementedException();
    }

    void CreateCone()
    {
        throw new NotImplementedException();
    }

    void CreateTorus()
    {
        throw new NotImplementedException();
    }

    void CreateHelix()
    {
        throw new NotImplementedException();
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
        _view.ZoomExtents();
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
            var start = edge.Start.Position.ToVector3();
            var end = edge.End.Position.ToVector3();
            builder.AddLine(start, end);
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
            builder.AddPolygon(face.GetVertices().Select(v => v.Position.ToVector3()).ToList());
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