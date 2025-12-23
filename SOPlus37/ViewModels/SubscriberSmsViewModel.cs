using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SOPlus37.Infrastructure;
using SOPlus37.Models;
using SOPlus37.Services;

namespace SOPlus37.ViewModels
{
    public class SubscriberSmsViewModel : ViewModelBase
    {
        private readonly string _phoneNumber;
        private readonly SubscriberSmsService _smsService;
        private readonly SmsPdfReportService _pdfService;

        public ObservableCollection<SmsItem> Sms { get; } = new ObservableCollection<SmsItem>();

        private DateTime _dateFrom;
        public DateTime DateFrom
        {
            get => _dateFrom;
            set => SetProperty(ref _dateFrom, value);
        }

        private DateTime _dateTo;
        public DateTime DateTo
        {
            get => _dateTo;
            set => SetProperty(ref _dateTo, value);
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

        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand GeneratePdfReportCommand { get; }

        public SubscriberSmsViewModel(string phoneNumber, SubscriberSmsService smsService, SmsPdfReportService pdfService)
        {
            _phoneNumber = phoneNumber;
            _smsService = smsService;
            _pdfService = pdfService;

            DateTo = DateTime.Now;
            DateFrom = DateTo.AddDays(-30);

            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            GeneratePdfReportCommand = new RelayCommand(GeneratePdfReport);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                Error = null;

                if (DateTo < DateFrom)
                {
                    Error = "Дата 'по' не может быть раньше даты 'с'.";
                    return;
                }

                IsLoading = true;

                var list = await _smsService.GetSmsAsync(_phoneNumber, DateFrom, DateTo);

                Sms.Clear();
                foreach (var item in list)
                    Sms.Add(item);
            }
            catch (Exception ex)
            {
                Error = "Не удалось загрузить SMS: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void GeneratePdfReport()
        {
            try
            {
                Error = null;

                if (DateTo < DateFrom)
                {
                    Error = "Дата 'по' не может быть раньше даты 'с'.";
                    return;
                }

                if (Sms.Count == 0)
                {
                    Error = "Нет SMS за выбранный период.";
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "PDF (*.pdf)|*.pdf",
                    FileName = $"SMS_{_phoneNumber}_{DateFrom:yyyyMMdd}_{DateTo:yyyyMMdd}.pdf"
                };

                if (dialog.ShowDialog() != true)
                    return;

                _pdfService.GenerateSmsReport(
                    _phoneNumber,
                    DateFrom,
                    DateTo,
                    new System.Collections.Generic.List<SmsItem>(Sms),
                    dialog.FileName);
            }
            catch (Exception ex)
            {
                Error = "Не удалось сформировать PDF: " + ex.Message;
            }
        }
    }
}
