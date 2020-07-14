using Dissertation.Modeling.Model;
using Dissertation.Modeling.Model.Basics;

namespace Dissertation.Modeling.UI
{
    public enum Direction
    {
        Top, Front
    }

    public interface ICanvasPointWrapper
    {
        void SetCoordinateLocation(Direction direction, Vector coordinateLocation, bool show);
    }
}
