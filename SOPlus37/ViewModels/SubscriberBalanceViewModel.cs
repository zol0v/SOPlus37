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
    public class SubscriberBalanceViewModel : ViewModelBase
    {
        private readonly string _phoneNumber;
        private readonly SubscriberBalanceService _balanceService;
        private readonly IDialogService _dialog;

        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        public ObservableCollection<BalanceTopUpItem> History { get; } = new ObservableCollection<BalanceTopUpItem>();

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

        private string _topUpAmountText;
        public string TopUpAmountText
        {
            get => _topUpAmountText;
            set => SetProperty(ref _topUpAmountText, value);
        }

        public IAsyncRelayCommand TopUpCommand { get; }

        public SubscriberBalanceViewModel(string phoneNumber, SubscriberBalanceService balanceService, IDialogService dialog)
        {
            _phoneNumber = phoneNumber;
            _balanceService = balanceService;
            _dialog = dialog;

            TopUpCommand = new AsyncRelayCommand(TopUpAsync);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                Error = null;

                var (balance, history) = await _balanceService.GetBalanceAndHistoryAsync(_phoneNumber);
                Balance = balance;

                History.Clear();
                foreach (var item in history)
                    History.Add(item);
            }
            catch (Exception ex)
            {
                Error = "Не удалось загрузить баланс: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task TopUpAsync()
        {
            try
            {
                Error = null;
                Success = null;

                if (string.IsNullOrWhiteSpace(TopUpAmountText))
                {
                    Error = "Введите сумму пополнения.";
                    return;
                }

                var normalized = TopUpAmountText.Trim().Replace(',', '.');

                if (!decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                {
                    Error = "Некорректная сумма.";
                    return;
                }

                if (amount <= 0)
                {
                    Error = "Сумма должна быть больше 0.";
                    return;
                }

                if (!_dialog.Confirm($"Вы уверены, что хотите пополнить баланс на {amount:N2} ₽ методом ELECTRONIC?",
                                    "Пополнение баланса"))
                    return;

                IsLoading = true;

                await _balanceService.ReplenishAsync(_phoneNumber, amount);

                Success = $"Баланс пополнен на {amount:N2} ₽ методом ELECTRONIC.";
                TopUpAmountText = "";

                await LoadAsync();
            }
            catch (Exception ex)
            {
                Error = "Не удалось пополнить баланс: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
