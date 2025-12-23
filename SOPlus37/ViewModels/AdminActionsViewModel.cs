using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SOPlus37.Infrastructure;
using SOPlus37.Models;
using SOPlus37.Services;

namespace SOPlus37.ViewModels
{
    public class AdminActionsViewModel : ViewModelBase
    {
        private readonly AdminActionsService _service;

        public ObservableCollection<AdminChargeResultItem> Results { get; } = new ObservableCollection<AdminChargeResultItem>();

        private int _processed;
        public int Processed
        {
            get => _processed;
            set => SetProperty(ref _processed, value);
        }

        private int _blockedNow;
        public int BlockedNow
        {
            get => _blockedNow;
            set => SetProperty(ref _blockedNow, value);
        }

        private decimal _totalCharged;
        public decimal TotalCharged
        {
            get => _totalCharged;
            set => SetProperty(ref _totalCharged, value);
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

        public IAsyncRelayCommand AdvanceTimeCommand { get; }

        public AdminActionsViewModel(AdminActionsService service)
        {
            _service = service;
            AdvanceTimeCommand = new AsyncRelayCommand(AdvanceTimeAsync);
        }

        private async Task AdvanceTimeAsync()
        {
            try
            {
                Error = null;
                Success = null;
                IsLoading = true;

                Results.Clear();

                var (processed, blockedNow, totalCharged, rows) = await _service.AdvanceTimeAsync();

                Processed = processed;
                BlockedNow = blockedNow;
                TotalCharged = totalCharged;

                foreach (var r in rows.OrderBy(x => x.StatusAfter).ThenBy(x => x.PhoneNumber))
                    Results.Add(r);

                Success = $"Готово. Обработано ACTIVE: {processed}. Заблокировано: {blockedNow}. Списано: {totalCharged:N2} ₽.";
            }
            catch (Exception ex)
            {
                Error = "Не удалось промотать время: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
