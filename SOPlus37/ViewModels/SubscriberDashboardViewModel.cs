using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SOPlus37.Models;
using SOPlus37.Services;
using SOPlus37.Infrastructure;

namespace SOPlus37.ViewModels
{
    public class SubscriberDashboardViewModel : ViewModelBase
    {
        private readonly SubscriberAccountService _accountService;
        private readonly string _phoneNumber;

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

        private SubscriberAccountInfo _info;
        public SubscriberAccountInfo Info
        {
            get => _info;
            set => SetProperty(ref _info, value);
        }

        public SubscriberDashboardViewModel(string phoneNumber, SubscriberAccountService accountService)
        {
            _phoneNumber = phoneNumber;
            _accountService = accountService;

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                Error = null;

                Info = await _accountService.GetAccountInfoByPhoneAsync(_phoneNumber);
            }
            catch (Exception ex)
            {
                Error = "Не удалось загрузить данные абонента: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
