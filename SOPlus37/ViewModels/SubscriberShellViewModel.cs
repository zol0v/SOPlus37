using CommunityToolkit.Mvvm.Input;
using SOPlus37.DAL;
using SOPlus37.Infrastructure;
using SOPlus37.Services;

namespace SOPlus37.ViewModels
{
    public class SubscriberShellViewModel : ViewModelBase
    {
        private readonly NavigationStore _appNav;
        private readonly AuthService _auth;
        private readonly string _phoneNumber;

        private readonly IDialogService _dialog;

        private readonly SubscriberAccountService _accountService;
        private readonly SubscriberBalanceService _balanceService;
        private readonly SubscriberCallsService _callsService;
        private readonly CallsPdfReportService _callsPdfService;
        private readonly SubscriberSmsService _smsService;
        private readonly SmsPdfReportService _smsPdfService;
        private readonly SubscriberTariffService _tariffService;
        private readonly SubscriberServicesService _servicesService;
        private readonly SubscriberActionsService _actionsService;

        public string SubscriberPhone => _phoneNumber;

        private string _pageTitle = "Личный кабинет";
        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        private string _pageSubtitle = "Добро пожаловать";
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

        public IRelayCommand GoDashboardCommand { get; }
        public IRelayCommand GoBalanceCommand { get; }
        public IRelayCommand GoCallsCommand { get; }
        public IRelayCommand GoSmsCommand { get; }
        public IRelayCommand GoTariffCommand { get; }
        public IRelayCommand GoServicesCommand { get; }
        public IRelayCommand GoActionsCommand { get; }
        public IRelayCommand LogoutCommand { get; }

        public SubscriberShellViewModel(NavigationStore appNav, AuthService auth, string phoneNumber)
        {
            _appNav = appNav;
            _auth = auth;
            _phoneNumber = phoneNumber;

            _dialog = new DialogService();

            var db = new MobileOperatorDbContext();

            _accountService = new SubscriberAccountService(db);
            _balanceService = new SubscriberBalanceService(db);
            _callsService = new SubscriberCallsService(db);
            _callsPdfService = new CallsPdfReportService();
            _smsService = new SubscriberSmsService(db);
            _smsPdfService = new SmsPdfReportService();
            _tariffService = new SubscriberTariffService(db);
            _servicesService = new SubscriberServicesService(db);
            _actionsService = new SubscriberActionsService(db);

            GoDashboardCommand = new RelayCommand(GoDashboard);
            GoBalanceCommand = new RelayCommand(GoBalance);
            GoCallsCommand = new RelayCommand(GoCalls);
            GoSmsCommand = new RelayCommand(GoSms);
            GoTariffCommand = new RelayCommand(GoTariff);
            GoServicesCommand = new RelayCommand(GoServices);
            GoActionsCommand = new RelayCommand(GoActions);

            LogoutCommand = new RelayCommand(() =>
                _appNav.CurrentViewModel = new SubscriberLoginViewModel(_auth, _appNav));

            GoDashboard();
        }

        private void GoDashboard()
        {
            PageTitle = "Личный кабинет";
            PageSubtitle = "Добро пожаловать, абонент SOPlus37!";
            CurrentPageViewModel = new SubscriberDashboardViewModel(_phoneNumber, _accountService);
        }

        private void GoBalance()
        {
            PageTitle = "Баланс";
            PageSubtitle = "Текущий баланс и история пополнений";
            CurrentPageViewModel = new SubscriberBalanceViewModel(_phoneNumber, _balanceService, _dialog);
        }

        private void GoCalls()
        {
            PageTitle = "Звонки";
            PageSubtitle = "Детализация за период";
            CurrentPageViewModel = new SubscriberCallsViewModel(_phoneNumber, _callsService, _callsPdfService);
        }

        private void GoSms()
        {
            PageTitle = "SMS";
            PageSubtitle = "Детализация за период";
            CurrentPageViewModel = new SubscriberSmsViewModel(_phoneNumber, _smsService, _smsPdfService);
        }

        private void GoTariff()
        {
            PageTitle = "Тариф";
            PageSubtitle = "Информация о текущем тарифе и смена тарифа";
            CurrentPageViewModel = new SubscriberTariffsViewModel(_phoneNumber, _tariffService, _dialog);
        }

        private void GoServices()
        {
            PageTitle = "Услуги";
            PageSubtitle = "Подключение и отключение услуг";
            CurrentPageViewModel = new SubscriberServicesViewModel(_phoneNumber, _servicesService, _dialog);
        }

        private void GoActions()
        {
            PageTitle = "Действия";
            PageSubtitle = "Звонки и SMS";
            CurrentPageViewModel = new SubscriberActionsViewModel(_phoneNumber, _actionsService);
        }
    }
}
