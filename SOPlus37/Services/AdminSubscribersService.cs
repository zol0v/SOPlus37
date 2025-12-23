using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SOPlus37.DAL;
using SOPlus37.Models;

namespace SOPlus37.Services
{
    public class AdminSubscribersService
    {
        private readonly MobileOperatorDbContext _db;

        public AdminSubscribersService(MobileOperatorDbContext db)
        {
            _db = db;
        }

        public async Task<(AdminSubscribersStats stats, List<AdminSubscriberItem> items)> GetSubscribersAsync(
            string statusFilter = "ALL",
            string clientTypeFilter = "ALL")
        {
            var subscribers = await _db.Subscribers
                .AsNoTracking()
                .Include(s => s.Individual)
                .Include(s => s.LegalEntity)
                .ToListAsync();

            var tariffsDict = await _db.Tariffs
                .AsNoTracking()
                .ToDictionaryAsync(t => t.TariffID, t => t.TariffName);

            var callsCounts = await _db.Calls
                .AsNoTracking()
                .GroupBy(c => c.SubscriberID)
                .Select(g => new { SubscriberID = g.Key, Count = g.Count() })
                .ToListAsync();

            var callsDict = callsCounts.ToDictionary(x => x.SubscriberID, x => x.Count);

            var smsCounts = await _db.Smses
                .AsNoTracking()
                .GroupBy(s => s.SubscriberID)
                .Select(g => new { SubscriberID = g.Key, Count = g.Count() })
                .ToListAsync();

            var smsDict = smsCounts.ToDictionary(x => x.SubscriberID, x => x.Count);

            var servicesCounts = await _db.SubscriberServices
                .AsNoTracking()
                .Where(ss => ss.IsActive)
                .GroupBy(ss => ss.SubscriberID)
                .Select(g => new { SubscriberID = g.Key, Count = g.Count() })
                .ToListAsync();

            var servicesDict = servicesCounts.ToDictionary(x => x.SubscriberID, x => x.Count);

            var stats = new AdminSubscribersStats
            {
                TotalSubscribers = subscribers.Count,
                IndividualsCount = subscribers.Count(s => s.IndividualID.HasValue && !s.LegalEntityID.HasValue),
                LegalEntitiesCount = subscribers.Count(s => s.LegalEntityID.HasValue && !s.IndividualID.HasValue),
                ActiveCount = subscribers.Count(s => (s.Status ?? "").ToUpper() == "ACTIVE"),
                BlockedCount = subscribers.Count(s => (s.Status ?? "").ToUpper() == "BLOCKED"),
                TotalCalls = callsCounts.Sum(x => x.Count),
                TotalSms = smsCounts.Sum(x => x.Count),
                TotalBalance = subscribers.Sum(s => s.Balance)
            };

            var filtered = subscribers.AsEnumerable();

            var sf = (statusFilter ?? "ALL").Trim().ToUpper();
            if (sf == "ACTIVE" || sf == "BLOCKED")
                filtered = filtered.Where(s => (s.Status ?? "").Trim().ToUpper() == sf);

            var cf = (clientTypeFilter ?? "ALL").Trim().ToUpper();
            if (cf == "IND")
                filtered = filtered.Where(s => s.IndividualID.HasValue && !s.LegalEntityID.HasValue);
            else if (cf == "LEGAL")
                filtered = filtered.Where(s => s.LegalEntityID.HasValue && !s.IndividualID.HasValue);

            var items = filtered
                .OrderBy(s => s.PhoneNumber)
                .Select(s =>
                {
                    var isLegal = s.LegalEntityID.HasValue && !s.IndividualID.HasValue;

                    string name;
                    string doc;

                    if (isLegal)
                    {
                        name = s.LegalEntity?.OrganizationName ?? "";
                        doc = s.LegalEntity?.OGRN ?? "";
                    }
                    else
                    {
                        name = s.Individual?.FullName ?? "";
                        doc = s.Individual?.PassportData ?? "";
                    }

                    var tariffName = tariffsDict.TryGetValue(s.TariffID, out var tname)
                        ? tname
                        : $"TariffID={s.TariffID}";

                    return new AdminSubscriberItem
                    {
                        SubscriberID = s.SubscriberID,
                        PhoneNumber = s.PhoneNumber,
                        Status = s.Status,
                        Balance = s.Balance,

                        ClientType = isLegal ? "Юр. лицо" : "Физ. лицо",
                        NameOrOrg = name,
                        Doc = doc,

                        TariffName = tariffName,

                        CallsCount = callsDict.TryGetValue(s.SubscriberID, out var cc) ? cc : 0,
                        SmsCount = smsDict.TryGetValue(s.SubscriberID, out var sc) ? sc : 0,

                        ServicesCount = servicesDict.TryGetValue(s.SubscriberID, out var svc) ? svc : 0
                    };
                })
                .ToList();

            return (stats, items);
        }
    }
}
