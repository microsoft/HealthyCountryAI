namespace HealthyHabitat
{
    using System.Windows;
    using GalaSoft.MvvmLight.Ioc;
    using MvvmDialogs;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            SimpleIoc.Default.Register<IDialogService>(
                () => new DialogService(frameworkDialogFactory: new CustomFrameworkDialogFactory()));
        }
    }
}
