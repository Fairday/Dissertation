using MaterialDesignThemes.Wpf;
using System.Windows;

namespace Dissertation.Modeling.UI
{
    public static class TabControlBehaviors
    {
        public static PackIconKind GetTabItemIcon(DependencyObject obj)
        {
            return (PackIconKind)obj.GetValue(TabItemIconProperty);
        }

        public static void SetTabItemIcon(DependencyObject obj, int value)
        {
            obj.SetValue(TabItemIconProperty, value);
        }

        public static readonly DependencyProperty TabItemIconProperty =
            DependencyProperty.RegisterAttached("TabItemIcon", typeof(PackIconKind), typeof(TabControlBehaviors), new PropertyMetadata(PackIconKind.Settings));

        public static CornerRadius GetTabItemCornerRadius(DependencyObject obj)
        {
            return (CornerRadius)obj.GetValue(TabItemCornerRadiusProperty);
        }

        public static void SetTabItemCornerRadius(DependencyObject obj, CornerRadius value)
        {
            obj.SetValue(TabItemCornerRadiusProperty, value);
        }

        public static readonly DependencyProperty TabItemCornerRadiusProperty =
            DependencyProperty.RegisterAttached("TabItemCornerRadius", typeof(CornerRadius), typeof(TabControlBehaviors), new PropertyMetadata(new CornerRadius(14)));
    }
}
