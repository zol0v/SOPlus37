using SOPlus37.Infrastructure;
using SOPlus37.Services;
using SOPlus37.ViewModels;
using System.Windows;

namespace SOPlus37
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var nav = new NavigationStore();
            var auth = new AuthService();

            nav.CurrentViewModel = new SubscriberLoginViewModel(auth, nav);

            var mainVm = new MainViewModel(nav);

            var window = new MainWindow
            {
                DataContext = mainVm
            };

            window.Show();
        }
    }
}
