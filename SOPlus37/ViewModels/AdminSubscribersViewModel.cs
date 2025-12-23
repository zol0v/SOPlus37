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
    public class AdminSubscribersViewModel : ViewModelBase
    {
        private readonly AdminSubscribersService _service;
        private readonly AdminSubscribersPdfReportService _pdfService;
        private readonly string _adminFullName;

        public ObservableCollection<AdminSubscriberItem> Subscribers { get; } = new ObservableCollection<AdminSubscriberItem>();

        public ObservableCollection<FilterOption> StatusFilters { get; } = new ObservableCollection<FilterOption>();
        public ObservableCollection<FilterOption> ClientTypeFilters { get; } = new ObservableCollection<FilterOption>();

        private FilterOption _selectedStatusFilter;
        public FilterOption SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set => SetProperty(ref _selectedStatusFilter, value);
        }

        private FilterOption _selectedClientTypeFilter;
        public FilterOption SelectedClientTypeFilter
        {
            get => _selectedClientTypeFilter;
            set => SetProperty(ref _selectedClientTypeFilter, value);
        }

        private int _totalSubscribers;
        public int TotalSubscribers { get => _totalSubscribers; set => SetProperty(ref _totalSubscribers, value); }

        private int _individualsCount;
        public int IndividualsCount { get => _individualsCount; set => SetProperty(ref _individualsCount, value); }

        private int _legalEntitiesCount;
        public int LegalEntitiesCount { get => _legalEntitiesCount; set => SetProperty(ref _legalEntitiesCount, value); }

        private int _activeCount;
        public int ActiveCount { get => _activeCount; set => SetProperty(ref _activeCount, value); }

        private int _blockedCount;
        public int BlockedCount { get => _blockedCount; set => SetProperty(ref _blockedCount, value); }

        private int _totalCalls;
        public int TotalCalls { get => _totalCalls; set => SetProperty(ref _totalCalls, value); }

        private int _totalSms;
        public int TotalSms { get => _totalSms; set => SetProperty(ref _totalSms, value); }

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        private string _error;
        public string Error { get => _error; set => SetProperty(ref _error, value); }

        private string _appliedStatusText = "Все";
        private string _appliedClientTypeText = "Все";

        public IAsyncRelayCommand ShowCommand { get; }
        public IRelayCommand GeneratePdfReportCommand { get; }

        public AdminSubscribersViewModel(
            AdminSubscribersService service,
            AdminSubscribersPdfReportService pdfService,
            string adminFullName)
        {
            _service = service;
            _pdfService = pdfService;
            _adminFullName = adminFullName;

            StatusFilters.Add(new FilterOption { Value = "ALL", Display = "Все" });
            StatusFilters.Add(new FilterOption { Value = "ACTIVE", Display = "ACTIVE" });
            StatusFilters.Add(new FilterOption { Value = "BLOCKED", Display = "BLOCKED" });

            ClientTypeFilters.Add(new FilterOption { Value = "ALL", Display = "Все" });
            ClientTypeFilters.Add(new FilterOption { Value = "IND", Display = "Физ. лица" });
            ClientTypeFilters.Add(new FilterOption { Value = "LEGAL", Display = "Юр. лица" });

            SelectedStatusFilter = StatusFilters[0];
            SelectedClientTypeFilter = ClientTypeFilters[0];

            ShowCommand = new AsyncRelayCommand(LoadAsync);
            GeneratePdfReportCommand = new RelayCommand(GeneratePdfReport);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                Error = null;
                IsLoading = true;

                var status = SelectedStatusFilter?.Value ?? "ALL";
                var type = SelectedClientTypeFilter?.Value ?? "ALL";

                var (stats, items) = await _service.GetSubscribersAsync(status, type);

                TotalSubscribers = stats.TotalSubscribers;
                IndividualsCount = stats.IndividualsCount;
                LegalEntitiesCount = stats.LegalEntitiesCount;
                ActiveCount = stats.ActiveCount;
                BlockedCount = stats.BlockedCount;
                TotalCalls = stats.TotalCalls;
                TotalSms = stats.TotalSms;

                _appliedStatusText = SelectedStatusFilter?.Display ?? "Все";
                _appliedClientTypeText = SelectedClientTypeFilter?.Display ?? "Все";

                Subscribers.Clear();
                foreach (var it in items)
                    Subscribers.Add(it);
            }
            catch (Exception ex)
            {
                Error = "Не удалось загрузить абонентов: " + ex.Message;
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

                if (Subscribers.Count == 0)
                {
                    Error = "Нет абонентов по выбранным фильтрам — отчет не сформирован.";
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "PDF (*.pdf)|*.pdf",
                    FileName = $"Subscribers_{_appliedStatusText}_{_appliedClientTypeText}_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                };

                if (dialog.ShowDialog() != true)
                    return;

                _pdfService.GenerateSubscribersReport(
                    _appliedStatusText,
                    _appliedClientTypeText,
                    _adminFullName,
                    new System.Collections.Generic.List<AdminSubscriberItem>(Subscribers),
                    dialog.FileName);
            }
            catch (Exception ex)
            {
                Error = "Не удалось сформировать PDF: " + ex.Message;
            }
        }
    }
}
