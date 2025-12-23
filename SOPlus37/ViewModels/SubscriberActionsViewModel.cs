using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SOPlus37.Infrastructure;
using SOPlus37.Models;
using SOPlus37.Services;

namespace SOPlus37.ViewModels
{
    public class SubscriberActionsViewModel : ViewModelBase
    {
        private readonly string _phoneNumber;
        private readonly SubscriberActionsService _actionsService;

        public ObservableCollection<CallTypeOption> CallTypes { get; } = new ObservableCollection<CallTypeOption>();

        private CallTypeOption _selectedCallType;
        public CallTypeOption SelectedCallType
        {
            get => _selectedCallType;
            set => SetProperty(ref _selectedCallType, value);
        }

        private string _callToNumber;
        public string CallToNumber
        {
            get => _callToNumber;
            set => SetProperty(ref _callToNumber, value);
        }

        private string _callMinutesText;
        public string CallMinutesText
        {
            get => _callMinutesText;
            set => SetProperty(ref _callMinutesText, value);
        }

        private string _smsToNumber;
        public string SmsToNumber
        {
            get => _smsToNumber;
            set => SetProperty(ref _smsToNumber, value);
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

        public IAsyncRelayCommand MakeCallCommand { get; }
        public IAsyncRelayCommand SendSmsCommand { get; }
        public IAsyncRelayCommand RefreshCallTypesCommand { get; }

        public SubscriberActionsViewModel(string phoneNumber, SubscriberActionsService actionsService)
        {
            _phoneNumber = phoneNumber;
            _actionsService = actionsService;

            MakeCallCommand = new AsyncRelayCommand(MakeCallAsync);
            SendSmsCommand = new AsyncRelayCommand(SendSmsAsync);
            RefreshCallTypesCommand = new AsyncRelayCommand(LoadCallTypesAsync);

            _ = LoadCallTypesAsync();
        }

        private async Task LoadCallTypesAsync()
        {
            try
            {
                Error = null;
                Success = null;
                IsLoading = true;

                var types = await _actionsService.GetCallTypesAsync();
                CallTypes.Clear();
                foreach (var t in types)
                    CallTypes.Add(t);

                if (SelectedCallType == null && CallTypes.Count > 0)
                    SelectedCallType = CallTypes[0];
            }
            catch (Exception ex)
            {
                Error = "Не удалось загрузить типы звонков: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task MakeCallAsync()
        {
            try
            {
                Error = null;
                Success = null;

                if (string.IsNullOrWhiteSpace(CallToNumber))
                {
                    Error = "Введите номер, вызываемого абонента.";
                    return;
                }

                CallToNumber = PhoneNumberValidator.Normalize(CallToNumber);

                if (!PhoneNumberValidator.IsValid(CallToNumber))
                {
                    Error = "Номер телефона должен быть в формате +79161234567 (знак + и минимум 11 цифр).";
                    return;
                }

                if (SelectedCallType == null)
                {
                    Error = "Выберите тип звонка.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(CallMinutesText))
                {
                    Error = "Введите длительность звонка в минутах.";
                    return;
                }

                if (!int.TryParse(CallMinutesText.Trim(), out var minutes) || minutes <= 0)
                {
                    Error = "Длительность должна быть целым числом больше 0.";
                    return;
                }

                IsLoading = true;

                var cost = await _actionsService.MakeCallAsync(_phoneNumber, CallToNumber, minutes, SelectedCallType.CallTypeID);
                Success = $"Звонок выполнен. Списано {cost:N2} ₽.";

                CallToNumber = string.Empty;
                CallMinutesText = string.Empty;
            }
            catch (InvalidOperationException ex)
            {
                Error = ex.Message;
            }
            catch (ArgumentException ex)
            {
                Error = ex.Message;
            }
            catch (Exception ex)
            {
                Error = "Не удалось выполнить звонок: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SendSmsAsync()
        {
            try
            {
                Error = null;
                Success = null;

                if (string.IsNullOrWhiteSpace(SmsToNumber))
                {
                    Error = "Введите номер абонента, которому хотите отправить SMS.";
                    return;
                }

                SmsToNumber = PhoneNumberValidator.Normalize(SmsToNumber);

                if (!PhoneNumberValidator.IsValid(SmsToNumber))
                {
                    Error = "Номер телефона должен быть в формате +79161234567 (знак + и минимум 11 цифр).";
                    return;
                }

                IsLoading = true;

                var cost = await _actionsService.SendSmsAsync(_phoneNumber, SmsToNumber);
                Success = $"SMS отправлено. Списано {cost:N2} ₽.";

                SmsToNumber = string.Empty;
            }
            catch (InvalidOperationException ex)
            {
                Error = ex.Message;
            }
            catch (ArgumentException ex)
            {
                Error = ex.Message;
            }
            catch (Exception ex)
            {
                Error = "Не удалось отправить SMS: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
