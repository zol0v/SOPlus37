using CommunityToolkit.Mvvm.Input;
using SOPlus37.Infrastructure;
using SOPlus37.Services;
using System.Threading.Tasks;

namespace SOPlus37.ViewModels
{
    public class SubscriberLoginViewModel : ViewModelBase
    {
        private readonly AuthService _auth;
        private readonly NavigationStore _nav;

        private string _phone;
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _error;
        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public IRelayCommand LoginCommand { get; }
        public IRelayCommand GoToAdminLoginCommand { get; }

        public SubscriberLoginViewModel(AuthService auth, NavigationStore nav)
        {
            _auth = auth;
            _nav = nav;

            LoginCommand = new AsyncRelayCommand(LoginAsync);
            GoToAdminLoginCommand = new RelayCommand(() =>
                _nav.CurrentViewModel = new AdminLoginViewModel(_auth, _nav));
        }

        private async Task LoginAsync()
        {
            Error = null;

            var result = await _auth.LoginAsync(Phone, Password, loginAsAdmin: false);
            if (!result.Success)
            {
                Error = result.Error;
                return;
            }

            _nav.CurrentViewModel = new SubscriberShellViewModel(_nav, _auth, Phone);

        }
    }
}
