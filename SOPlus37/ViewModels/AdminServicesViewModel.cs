using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SOPlus37.Infrastructure;
using SOPlus37.Models;
using SOPlus37.Services;

namespace SOPlus37.ViewModels
{
    public class AdminServicesViewModel : ViewModelBase
    {
        private readonly AdminServicesService _service;
        private readonly int _adminId;

        public ObservableCollection<AdminServiceItem> Services { get; } = new ObservableCollection<AdminServiceItem>();

        public ObservableCollection<string> ServiceTypes { get; } = new ObservableCollection<string>
        {
            "INTERNET",
            "OTHER"
        };

        private AdminServiceItem _selected;
        public AdminServiceItem Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
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

        private string _success;
        public string Success
        {
            get => _success;
            set => SetProperty(ref _success, value);
        }

        private string _newName;
        public string NewName
        {
            get => _newName;
            set => SetProperty(ref _newName, value);
        }

        private string _newType = "OTHER";
        public string NewType
        {
            get => _newType;
            set => SetProperty(ref _newType, value);
        }

        private string _newCost = "0";
        public string NewCost
        {
            get => _newCost;
            set => SetProperty(ref _newCost, value);
        }

        private string _newDescription;
        public string NewDescription
        {
            get => _newDescription;
            set => SetProperty(ref _newDescription, value);
        }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand AddCommand { get; }

        public AdminServicesViewModel(AdminServicesService service, int adminId)
        {
            _service = service;
            _adminId = adminId;

            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            AddCommand = new AsyncRelayCommand(AddAsync);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                Error = null;
                Success = null;
                IsLoading = true;

                var items = await _service.GetAllAsync();
                Services.Clear();
                foreach (var s in items) Services.Add(s);

                if (Services.Count > 0 && Selected == null)
                    Selected = Services[0];
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

        private async Task AddAsync()
        {
            try
            {
                Error = null;
                Success = null;

                if (string.IsNullOrWhiteSpace(NewName))
                {
                    Error = "Заполните поля добавления услуги.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(NewType))
                {
                    Error = "Выберите тип услуги.";
                    return;
                }

                if (!TryParseDecimal(NewCost, out var cost))
                {
                    Error = "Стоимость должна быть числом (например 10 или 10,50).";
                    return;
                }

                IsLoading = true;

                await _service.AddAsync(
                    _adminId,
                    NewName,
                    NewType,
                    cost,
                    NewDescription
                );

                Success = $"Услуга \"{NewName}\" добавлена.";

                NewName = "";
                NewType = "OTHER";
                NewCost = "0";
                NewDescription = "";

                await LoadAsync();
            }
            catch (Exception ex)
            {
                Error = "Не удалось добавить услугу: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static bool TryParseDecimal(string text, out decimal value)
        {
            value = 0m;
            if (string.IsNullOrWhiteSpace(text)) return false;

            if (decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out value))
                return true;

            if (decimal.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
                return true;

            return false;
        }
    }
}
