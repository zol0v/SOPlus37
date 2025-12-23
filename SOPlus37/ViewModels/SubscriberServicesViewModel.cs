using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SOPlus37.Infrastructure;
using SOPlus37.Models;
using SOPlus37.Services;

namespace SOPlus37.ViewModels
{
    public class SubscriberServicesViewModel : ViewModelBase
    {
        private readonly string _phoneNumber;
        private readonly SubscriberServicesService _service;
        private readonly IDialogService _dialog;

        public ObservableCollection<ServiceItem> ConnectedServices { get; } = new ObservableCollection<ServiceItem>();
        public ObservableCollection<ServiceItem> AvailableServices { get; } = new ObservableCollection<ServiceItem>();

        private ServiceItem _selectedConnected;
        public ServiceItem SelectedConnected
        {
            get => _selectedConnected;
            set { SetProperty(ref _selectedConnected, value); SelectedService = value ?? SelectedService; }
        }

        private ServiceItem _selectedAvailable;
        public ServiceItem SelectedAvailable
        {
            get => _selectedAvailable;
            set { SetProperty(ref _selectedAvailable, value); SelectedService = value ?? SelectedService; }
        }

        private ServiceItem _selectedService;
        public ServiceItem SelectedService
        {
            get => _selectedService;
            set => SetProperty(ref _selectedService, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _error;
        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand ConnectCommand { get; }
        public IAsyncRelayCommand DisconnectCommand { get; }

        public SubscriberServicesViewModel(string phoneNumber, SubscriberServicesService service, IDialogService dialog)
        {
            _phoneNumber = phoneNumber;
            _service = service;
            _dialog = dialog;

            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            ConnectCommand = new AsyncRelayCommand(ConnectAsync);
            DisconnectCommand = new AsyncRelayCommand(DisconnectAsync);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                Error = null;
                IsLoading = true;

                var (connected, available) = await _service.GetServicesAsync(_phoneNumber);

                ConnectedServices.Clear();
                foreach (var s in connected) ConnectedServices.Add(s);

                AvailableServices.Clear();
                foreach (var s in available) AvailableServices.Add(s);

                SelectedService = null;
            }
            catch (Exception ex)
            {
                Error = "Не удалось загрузить услуги: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ConnectAsync()
        {
            try
            {
                Error = null;

                if (SelectedAvailable == null)
                {
                    Error = "Выберите услугу для подключения.";
                    return;
                }

                var s = SelectedAvailable;

                if (!_dialog.Confirm($"Вы уверены, что хотите подключить услугу \"{s.ServiceName}\" за {s.Cost:N2} ₽?",
                                    "Подключение услуги"))
                    return;

                IsLoading = true;
                await _service.ConnectAsync(_phoneNumber, s.ServiceID);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Error = "Не удалось подключить услугу: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DisconnectAsync()
        {
            try
            {
                Error = null;

                if (SelectedConnected == null)
                {
                    Error = "Выберите услугу для отключения.";
                    return;
                }

                var s = SelectedConnected;

                if (!_dialog.Confirm($"Вы уверены, что хотите отключить услугу \"{s.ServiceName}\"?",
                                    "Отключение услуги"))
                    return;

                IsLoading = true;
                await _service.DisconnectAsync(_phoneNumber, s.ServiceID);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Error = "Не удалось отключить услугу: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
