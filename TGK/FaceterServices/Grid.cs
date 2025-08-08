using TGK.Geometry;
using static TGK.Geometry.DirectionOfRotation;

namespace TGK.FaceterServices;

public sealed class Grid
{
    internal enum Corner
    {
        Invalid,

        LeftBottom,

        RightBottom,

        RightTop,

        LeftTop
    }

    public sealed class Cell
    {
        public Uv LeftBottom { get; }

        public Uv RightBottom => new Uv(RightTop.U, LeftBottom.V);

        public Uv RightTop { get; }

        public Uv LeftTop => new Uv(LeftBottom.U, RightTop.V);

        public Cell? East { get; internal set; }

        public Cell? South { get; internal set; }

        public Cell? West { get; internal set; }

        public Cell? North { get; internal set; }

        public Cell? NorthEast
        {
            get
            {
                if (North != null) return North.East;
                return East?.North;
            }
        }

        public Cell? NorthWest
        {
            get
            {
                if (North != null) return North.West;
                return West?.North;
            }
        }

        public Cell? SouthEast
        {
            get
            {
                if (South != null) return South.East;
                return East?.South;
            }
        }

        public Cell? SouthWest
        {
            get
            {
                if (South != null) return South.West;
                return West?.South;
            }
        }

        public bool Ignore { get; set; }

        public bool CompletelySurrounded => East is { Ignore: false } && South is { Ignore: false } && West is { Ignore: false }
            && North is { Ignore: false };

        internal Cell(Uv leftBottom, Uv rightTop)
        {
            LeftBottom = leftBottom;
            RightTop = rightTop;
        }

        internal Corner GetCornerWhichIsNotCompletelySurrounded()
        {
            if (West == null || West.Ignore) return Corner.LeftBottom;
            if (North == null || North.Ignore) return Corner.LeftTop;
            if (South == null || South.Ignore) return Corner.RightTop;
            if (East == null || East.Ignore) return Corner.RightBottom;
            throw new InvalidOperationException("Cell is completely surrounded.");
        }

        internal Uv GetPosition(Corner corner)
        {
            switch (corner)
            {
                case Corner.LeftBottom:
                    return LeftBottom;

                case Corner.RightBottom:
                    return RightBottom;

                case Corner.RightTop:
                    return RightTop;

                case Corner.LeftTop:
                    return LeftTop;

                default:
                    throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
            }
        }

        public override string ToString()
        {
            return $"{LeftBottom} → {RightTop}";
        }
    }

    readonly double _rowHeight;

    readonly List<Cell> _cells = [];

    double _right;

    readonly double _top;

    bool _alreadyCalled;

    public Grid(Uv leftBottom, double rowHeight)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rowHeight);

        _rowHeight = rowHeight;
        _right = leftBottom.U;
        _top = leftBottom.V;
    }

    public Cell AddColumn(double right)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(right, _right);

        var leftBottom = new Uv(_right, _top);
        var rightTop = new Uv(right, _top + _rowHeight);
        var cell = new Cell(leftBottom, rightTop);
        _right = right;
        if (_cells.Count > 0)
        {
            Cell lastCell = _cells[^1];
            cell.West = lastCell;
            lastCell.East = cell;
        }
        _cells.Add(cell);
        return cell;
    }

    /// <summary>
    /// Returns the polygons found in the grid.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="NullReferenceException"></exception>
    public List<Polygon> GetPolygons()
    {
        // Since we change the Ignore flag on the cells, we can only call this method once.
        if (_alreadyCalled) throw new InvalidOperationException("GetPolygons can only be called once.");
        _alreadyCalled = true;

        List<Uv> vertices = [];
        Corner startingCorner = Corner.Invalid;
        Corner corner = Corner.Invalid;
        Cell? startingCell = null;
        Cell? cell = null;
        var polygons = new List<Polygon>();
        var polygonCells = new HashSet<Cell>();
        while (true)
        {
            if (startingCorner == Corner.Invalid)
            {
                // We have to start from a cell on the edge, i.e., a cell that doesn't have any four neighbors.
                startingCell = cell = _cells.FirstOrDefault(c => c is { Ignore: false, CompletelySurrounded: false });
                if (cell == null) return polygons;

                // Start a new polygon.
                polygonCells.Add(cell);
                startingCorner = corner = cell.GetCornerWhichIsNotCompletelySurrounded();
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (startingCorner)
                {
                    case Corner.LeftBottom:
                        vertices.Add(cell.LeftBottom);
                        break;

                    case Corner.RightBottom:
                        vertices.Add(cell.RightBottom);
                        break;

                    case Corner.RightTop:
                        vertices.Add(cell.RightTop);
                        break;

                    case Corner.LeftTop:
                        vertices.Add(cell.LeftTop);
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
            if (cell == null) throw new NullReferenceException($"{nameof(cell)} is null.");
            (cell, corner) = MoveToNextCorner(cell, corner, vertices);
            polygonCells.Add(cell);
            if (cell == startingCell && corner == startingCorner)
            {
                // We have reached the end of the polygon.
                var polygon = new Polygon(vertices, Clockwise, isSimple: true);
                polygons.Add(polygon);
                foreach (Cell polygonCell in polygonCells) polygonCell.Ignore = true;
                polygonCells.Clear();

                // Start a new polygon.
                vertices = [];
                startingCell = null;
                startingCorner = Corner.Invalid;
            }
            else
                vertices.Add(cell.GetPosition(corner));
        }
    }

    static (Cell, Corner) MoveToNextCorner(Cell cell, Corner corner, List<Uv> vertices)
    {
        ArgumentNullException.ThrowIfNull(cell);
        ArgumentNullException.ThrowIfNull(vertices);

        // Since we want to generate holes, we turn clockwise.

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (corner)
        {
            case Corner.LeftBottom:
                if (cell.SouthWest is { Ignore: false })
                {
                    // Go down.
                    vertices.Add(cell.SouthWest.RightBottom);
                    return (cell.SouthWest, Corner.RightBottom);
                }
                if (cell.West is { Ignore: false })
                {
                    // Go left.
                    return (cell.West, Corner.LeftBottom);
                }
                // Go up.
                return (cell, Corner.LeftTop);

            case Corner.LeftTop:
                if (cell.NorthWest is { Ignore: false })
                {
                    // Go left.
                    return (cell.NorthWest, Corner.LeftBottom);
                }
                if (cell.North is { Ignore: false })
                {
                    // Go up.
                    return (cell.North, Corner.LeftTop);
                }
                // Go right.
                return (cell, Corner.RightTop);

            case Corner.RightTop:
                if (cell.NorthEast is { Ignore: false })
                {
                    // Go up.
                    return (cell.NorthEast, Corner.RightTop);
                }
                if (cell.East is { Ignore: false })
                {
                    // Go right.
                    return (cell.East, Corner.RightTop);
                }
                // Go down.
                return (cell, Corner.RightBottom);

            case Corner.RightBottom:
                if (cell.SouthEast is { Ignore: false })
                {
                    // Go right.
                    return (cell.SouthEast, Corner.RightBottom);
                }
                if (cell.South is { Ignore: false })
                {
                    // Go down.
                    return (cell.South, Corner.RightBottom);
                }

                // Go left.
                return (cell, Corner.LeftBottom);

            default:
                throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
        }
    }
}