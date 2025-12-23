using CommunityToolkit.Mvvm.ComponentModel;

namespace SOPlus37.Infrastructure
{
    public class SectionStore : ObservableObject
    {
        private ViewModelBase _currentSection;

        public ViewModelBase CurrentSection
        {
            get => _currentSection;
            set => SetProperty(ref _currentSection, value);
        }
    }
}
