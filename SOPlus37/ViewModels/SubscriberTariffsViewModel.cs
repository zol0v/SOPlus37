using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SOPlus37.Infrastructure;
using SOPlus37.Models;
using SOPlus37.Services;

namespace SOPlus37.ViewModels
{
    public class SubscriberTariffsViewModel : ViewModelBase
    {
        private readonly string _phoneNumber;
        private readonly SubscriberTariffService _service;
        private readonly IDialogService _dialog;

        public ObservableCollection<TariffItem> AvailableTariffs { get; } = new ObservableCollection<TariffItem>();

        private TariffItem _currentTariff;
        public TariffItem CurrentTariff
        {
            get => _currentTariff;
            set => SetProperty(ref _currentTariff, value);
        }

        private TariffItem _selectedTariff;
        public TariffItem SelectedTariff
        {
            get => _selectedTariff;
            set => SetProperty(ref _selectedTariff, value);
        }

        private string _clientTypeText;
        public string ClientTypeText
        {
            get => _clientTypeText;
            set => SetProperty(ref _clientTypeText, value);
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
        public IAsyncRelayCommand ChangeTariffCommand { get; }

        public SubscriberTariffsViewModel(string phoneNumber, SubscriberTariffService service, IDialogService dialog)
        {
            _phoneNumber = phoneNumber;
            _service = service;
            _dialog = dialog;

            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            ChangeTariffCommand = new AsyncRelayCommand(ChangeTariffAsync);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                Error = null;
                IsLoading = true;

                var (current, available, isLegal) = await _service.GetTariffsAsync(_phoneNumber);

                CurrentTariff = current;
                ClientTypeText = isLegal ? "Юридическое лицо (корпоративные тарифы)" : "Физическое лицо (некорпоративные тарифы)";

                AvailableTariffs.Clear();
                foreach (var t in available)
                    AvailableTariffs.Add(t);

                SelectedTariff = null;
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

        private async Task ChangeTariffAsync()
        {
            try
            {
                Error = null;

                if (SelectedTariff == null)
                {
                    Error = "Выберите тариф из списка.";
                    return;
                }

                var t = SelectedTariff;
                var msg =
                    $"Вы уверены, что хотите перейти на тариф \"{t.TariffName}\"?\n\n" +
                    $"Стоимость перехода и месячная плата: {t.SwitchCost:N2} ₽\n" +
                    $"Стоимость по городу: {t.CityCallCost:N2} ₽/мин\n" +
                    $"Стоимость межгород: {t.IntercityCallCost:N2} ₽/мин\n" +
                    $"Стоимость международн.: {t.InternationalCallCost:N2} ₽/мин\n" +
                    $"SMS: {t.SmsCost:N2} ₽/шт";

                if (!_dialog.Confirm(msg, "Смена тарифа"))
                    return;

                IsLoading = true;
                await _service.ChangeTariffAsync(_phoneNumber, t.TariffID);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Error = "Не удалось сменить тариф: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
