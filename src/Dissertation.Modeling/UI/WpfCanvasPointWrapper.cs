using System.Windows;
using System.Windows.Controls;

namespace Dissertation.Modeling.UI
{
    public class WpfCanvasPointWrapper : ICanvasPointWrapper
    {
        private readonly UIElement _Element;

        public WpfCanvasPointWrapper(UIElement element)
        {
            _Element = element;
        }

        public void SetCoordinateLocation(Direction direction, Model.Basics.Vector coordinateLocation, bool show)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (show)
                {
                    _Element.Opacity = 1;
                }
                else
                {
                    _Element.Opacity = 0.3;
                }
                switch (direction)
                {
                    case Direction.Top:
                    {
                        Canvas.SetLeft(_Element, coordinateLocation.Y);
                        Canvas.SetTop(_Element, coordinateLocation.X);
                        break;
                    }
                    case Direction.Front:
                    {
                        Canvas.SetLeft(_Element, coordinateLocation.Y);
                        Canvas.SetTop(_Element, coordinateLocation.Z);
                        break;
                    }
                }
            });
        }
    }
}
