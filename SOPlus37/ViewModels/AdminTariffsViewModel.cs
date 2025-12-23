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
    public class AdminTariffsViewModel : ViewModelBase
    {
        private readonly AdminTariffsService _service;
        private readonly int _adminId;

        public ObservableCollection<AdminTariffItem> Tariffs { get; } = new ObservableCollection<AdminTariffItem>();

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

        // ===== поля добавления =====
        private string _newName;
        public string NewName
        {
            get => _newName;
            set => SetProperty(ref _newName, value);
        }

        private string _newCityCost = "0";
        public string NewCityCost
        {
            get => _newCityCost;
            set => SetProperty(ref _newCityCost, value);
        }

        private string _newIntercityCost = "0";
        public string NewIntercityCost
        {
            get => _newIntercityCost;
            set => SetProperty(ref _newIntercityCost, value);
        }

        private string _newInternationalCost = "0";
        public string NewInternationalCost
        {
            get => _newInternationalCost;
            set => SetProperty(ref _newInternationalCost, value);
        }

        private string _newSmsCost = "0";
        public string NewSmsCost
        {
            get => _newSmsCost;
            set => SetProperty(ref _newSmsCost, value);
        }

        private string _newSwitchCost = "0";
        public string NewSwitchCost
        {
            get => _newSwitchCost;
            set => SetProperty(ref _newSwitchCost, value);
        }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand AddCommand { get; }

        public AdminTariffsViewModel(AdminTariffsService service, int adminId)
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
                Tariffs.Clear();
                foreach (var t in items)
                    Tariffs.Add(t);
            }
            catch (Exception ex)
            {
                Error = "Не удалось загрузить тарифы: " + ex.Message;
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
                    Error = "Заполните поля добавления тарифа.";
                    return;
                }

                if (!TryParseDecimal(NewCityCost, out var city) ||
                    !TryParseDecimal(NewIntercityCost, out var intercity) ||
                    !TryParseDecimal(NewInternationalCost, out var international) ||
                    !TryParseDecimal(NewSmsCost, out var sms) ||
                    !TryParseDecimal(NewSwitchCost, out var sw))
                {
                    Error = "Проверьте поля стоимости: нужно число (например 10 или 10,50).";
                    return;
                }

                IsLoading = true;

                await _service.AddAsync(_adminId, NewName, city, intercity, international, sms, sw);

                Success = $"Тариф \"{NewName}\" добавлен.";

                NewName = "";
                NewCityCost = "0";
                NewIntercityCost = "0";
                NewInternationalCost = "0";
                NewSmsCost = "0";
                NewSwitchCost = "0";

                await LoadAsync();
            }
            catch (Exception ex)
            {
                Error = "Не удалось добавить тариф: " + ex.Message;
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
