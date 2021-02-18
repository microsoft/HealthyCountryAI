namespace HealthyHabitat.Views
{
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (sender is Path)
            {
                ((Path)sender).Opacity = 100;
            }

            base.OnDragOver(e);
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            if (sender is Path)
            {
                ((Path)sender).Opacity = 0;
            }

            base.OnDragLeave(e);
        }
    }
}
