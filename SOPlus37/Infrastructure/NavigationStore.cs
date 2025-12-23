using CommunityToolkit.Mvvm.ComponentModel;

namespace SOPlus37.Infrastructure
{
    public class NavigationStore : ObservableObject
    {
        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }
    }
}
