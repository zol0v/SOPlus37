using CommunityToolkit.Mvvm.Input;
using SOPlus37.DAL;
using SOPlus37.Infrastructure;
using SOPlus37.Services;

namespace SOPlus37.ViewModels
{
    public class AdminShellViewModel : ViewModelBase
    {
        private readonly NavigationStore _appNav;
        private readonly AuthService _auth;

        private readonly int _adminId;

        private readonly MobileOperatorDbContext _db;
        private readonly AdminTariffsService _tariffsService;
        private readonly AdminServicesService _servicesService;
        private readonly AdminActionsService _actionsService;
        private readonly AdminSubscribersService _subscribersService;

        private readonly AdminSubscribersPdfReportService _subscribersPdfService;

        public string AdminName { get; }

        private string _pageTitle = "Панель администратора";
        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        private string _pageSubtitle = "Управление данными";
        public string PageSubtitle
        {
            get => _pageSubtitle;
            set => SetProperty(ref _pageSubtitle, value);
        }

        private ViewModelBase _currentPageViewModel;
        public ViewModelBase CurrentPageViewModel
        {
            get => _currentPageViewModel;
            set => SetProperty(ref _currentPageViewModel, value);
        }

        public IRelayCommand GoTariffsCommand { get; }
        public IRelayCommand GoServicesCommand { get; }
        public IRelayCommand GoSubscribersCommand { get; }
        public IRelayCommand GoActionsCommand { get; }
        public IRelayCommand LogoutCommand { get; }

        public AdminShellViewModel(NavigationStore appNav, AuthService auth, int adminId, string adminName)
        {
            _appNav = appNav;
            _auth = auth;
            _adminId = adminId;

            AdminName = string.IsNullOrWhiteSpace(adminName) ? "Администратор" : adminName;

            _db = new MobileOperatorDbContext();
            _tariffsService = new AdminTariffsService(_db);
            _servicesService = new AdminServicesService(_db);
            _actionsService = new AdminActionsService(_db);
            _subscribersService = new AdminSubscribersService(_db);

            _subscribersPdfService = new AdminSubscribersPdfReportService();

            GoTariffsCommand = new RelayCommand(GoTariffs);
            GoServicesCommand = new RelayCommand(GoServices);
            GoSubscribersCommand = new RelayCommand(GoSubscribers);
            GoActionsCommand = new RelayCommand(GoActions);

            LogoutCommand = new RelayCommand(() =>
                _appNav.CurrentViewModel = new AdminLoginViewModel(_auth, _appNav));

            GoTariffs();
        }

        private void GoTariffs()
        {
            PageTitle = "Тарифы";
            PageSubtitle = "Просмотр и добавление тарифов";
            CurrentPageViewModel = new AdminTariffsViewModel(_tariffsService, _adminId);
        }

        private void GoServices()
        {
            PageTitle = "Услуги";
            PageSubtitle = "Просмотр и добавление услуг";
            CurrentPageViewModel = new AdminServicesViewModel(_servicesService, _adminId);
        }

        private void GoSubscribers()
        {
            PageTitle = "Абоненты";
            PageSubtitle = "Статистика и список абонентов";
            CurrentPageViewModel = new AdminSubscribersViewModel(_subscribersService, _subscribersPdfService, AdminName);
        }

        private void GoActions()
        {
            PageTitle = "Действия";
            PageSubtitle = "Операции администратора";
            CurrentPageViewModel = new AdminActionsViewModel(_actionsService);
        }
    }
}
