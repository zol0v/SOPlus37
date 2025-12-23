using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SOPlus37.DAL;
using SOPlus37.Models;

namespace SOPlus37.Services
{
    public class AdminActionsService
    {
        private readonly MobileOperatorDbContext _db;

        public AdminActionsService(MobileOperatorDbContext db)
        {
            _db = db;
        }

        public async Task<(int processed, int blockedNow, decimal totalCharged, List<AdminChargeResultItem> rows)> AdvanceTimeAsync()
        {
            var resultRows = new List<AdminChargeResultItem>();

            var activeSubs = await _db.Subscribers
                .Where(s => s.Status == "ACTIVE")
                .ToListAsync();

            if (activeSubs.Count == 0)
                return (0, 0, 0m, resultRows);

            var subIds = activeSubs.Select(s => s.SubscriberID).ToList();
            var tariffIds = activeSubs.Select(s => s.TariffID).Distinct().ToList();

            var tariffs = await _db.Tariffs
                .AsNoTracking()
                .Where(t => tariffIds.Contains(t.TariffID))
                .Select(t => new { t.TariffID, t.SwitchCost })
                .ToListAsync();

            var connections = await _db.SubscriberServices
                .AsNoTracking()
                .Where(x => subIds.Contains(x.SubscriberID))
                .Select(x => new { x.SubscriberID, x.ServiceID })
                .ToListAsync();

            var serviceIds = connections.Select(c => c.ServiceID).Distinct().ToList();

            var services = await _db.Services
                .AsNoTracking()
                .Where(s => serviceIds.Contains(s.ServiceID) && s.IsActive)
                .Select(s => new { s.ServiceID, s.Cost })
                .ToListAsync();

            int blockedNow = 0;
            decimal totalCharged = 0m;

            foreach (var sub in activeSubs)
            {
                decimal tariffFee = tariffs.FirstOrDefault(t => t.TariffID == sub.TariffID)?.SwitchCost ?? 0m;

                var myServiceIds = connections
                    .Where(c => c.SubscriberID == sub.SubscriberID)
                    .Select(c => c.ServiceID)
                    .ToList();

                decimal servicesFee = 0m;
                foreach (var sid in myServiceIds)
                {
                    servicesFee += services.FirstOrDefault(s => s.ServiceID == sid)?.Cost ?? 0m;
                }

                tariffFee = Math.Round(tariffFee, 2, MidpointRounding.AwayFromZero);
                servicesFee = Math.Round(servicesFee, 2, MidpointRounding.AwayFromZero);

                var charge = Math.Round(tariffFee + servicesFee, 2, MidpointRounding.AwayFromZero);

                if (charge > 0)
                {
                    sub.Balance -= charge;
                    totalCharged += charge;
                }

                if (sub.Balance < 0)
                {
                    sub.Status = "BLOCKED";
                    blockedNow++;
                }

                resultRows.Add(new AdminChargeResultItem
                {
                    SubscriberID = sub.SubscriberID,
                    PhoneNumber = sub.PhoneNumber,
                    TariffFee = tariffFee,
                    ServicesFee = servicesFee,
                    BalanceAfter = sub.Balance,
                    StatusAfter = sub.Status
                });
            }

            await _db.SaveChangesAsync();
            return (activeSubs.Count, blockedNow, totalCharged, resultRows);
        }
    }
}
