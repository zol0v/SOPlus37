using SOPlus37.Infrastructure;

namespace SOPlus37.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public NavigationStore NavigationStore { get; }

        public ViewModelBase CurrentViewModel => NavigationStore.CurrentViewModel;

        public MainViewModel(NavigationStore navigationStore)
        {
            NavigationStore = navigationStore;
            NavigationStore.PropertyChanged += (_, __) => OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
