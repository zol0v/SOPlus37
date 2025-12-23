using CommunityToolkit.Mvvm.Input;
using SOPlus37.Infrastructure;
using SOPlus37.Services;
using System.Threading.Tasks;

namespace SOPlus37.ViewModels
{
    public class AdminLoginViewModel : ViewModelBase
    {
        private readonly AuthService _auth;
        private readonly NavigationStore _nav;

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
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
        public IRelayCommand BackToSubscriberLoginCommand { get; }

        public AdminLoginViewModel(AuthService auth, NavigationStore nav)
        {
            _auth = auth;
            _nav = nav;

            LoginCommand = new AsyncRelayCommand(LoginAsync);
            BackToSubscriberLoginCommand = new RelayCommand(() =>
                _nav.CurrentViewModel = new SubscriberLoginViewModel(_auth, _nav));
        }

        private async Task LoginAsync()
        {
            Error = null;

            var result = await _auth.LoginAsync(Username, Password, loginAsAdmin: true);
            if (!result.Success)
            {
                Error = result.Error;
                return;
            }

            _nav.CurrentViewModel = new AdminShellViewModel(_nav, _auth, result.Admin.AdminID, result.Admin.Username);
        }
    }
}
