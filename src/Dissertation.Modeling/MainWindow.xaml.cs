using System.Windows;

namespace Dissertation.Modeling
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(TopViewLayout, FrontViewLayout, ViewPort3D);
        }
    }
}
