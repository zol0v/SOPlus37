using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SOPlus37.DAL;
using SOPlus37.Models;

namespace SOPlus37.Services
{
    public class SubscriberServicesService
    {
        private readonly MobileOperatorDbContext _db;

        public SubscriberServicesService(MobileOperatorDbContext db)
        {
            _db = db;
        }

        public async Task<(List<ServiceItem> connected, List<ServiceItem> available)> GetServicesAsync(string phoneNumber)
        {
            var sub = await _db.Subscribers.AsNoTracking()
                .FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber);

            if (sub == null) throw new InvalidOperationException("Абонент не найден.");

            var all = await _db.Services.AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.ServiceName)
                .ToListAsync();

            var links = await _db.SubscriberServices.AsNoTracking()
                .Where(x => x.SubscriberID == sub.SubscriberID)
                .ToListAsync();

            var activeByServiceId = links
                .Where(x => x.IsActive)
                .GroupBy(x => x.ServiceID)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.ActivatedDate).FirstOrDefault());

            var items = all.Select(s =>
            {
                var isConnected = activeByServiceId.TryGetValue(s.ServiceID, out var link);
                return new ServiceItem
                {
                    ServiceID = s.ServiceID,
                    ServiceName = s.ServiceName,
                    ServiceType = s.ServiceType,
                    Description = s.Description,
                    Cost = s.Cost,
                    IsRecurring = s.IsRecurring,
                    IsConnected = isConnected,
                    ActivatedDate = link?.ActivatedDate
                };
            }).ToList();

            var connected = items.Where(i => i.IsConnected).ToList();
            var available = items.Where(i => !i.IsConnected).ToList();

            return (connected, available);
        }

        public async Task ConnectAsync(string phoneNumber, int serviceId)
        {
            var sub = await _db.Subscribers.FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber);
            if (sub == null) throw new InvalidOperationException("Абонент не найден.");

            var service = await _db.Services.AsNoTracking()
                .FirstOrDefaultAsync(s => s.ServiceID == serviceId && s.IsActive);

            if (service == null) throw new InvalidOperationException("Услуга не найдена или не активна.");

            var activeSame = await _db.SubscriberServices
                .FirstOrDefaultAsync(x => x.SubscriberID == sub.SubscriberID && x.ServiceID == serviceId && x.IsActive);

            if (activeSame != null)
                throw new InvalidOperationException("Услуга уже подключена.");

            // SO5: SMS-услуг больше нет (SMS теперь часть тарифа),
            // оставляем ограничение только для INTERNET: максимум 1 интернет-услуга одновременно
            var type = (service.ServiceType ?? "").Trim().ToUpperInvariant();
            if (type == "INTERNET")
            {
                var hasInternet = await (
                    from ss in _db.SubscriberServices
                    join s in _db.Services on ss.ServiceID equals s.ServiceID
                    where ss.SubscriberID == sub.SubscriberID
                          && ss.IsActive
                          && s.IsActive
                          && (s.ServiceType ?? "").Trim().ToUpper() == "INTERNET"
                    select ss.SubscriberServiceID
                ).AnyAsync();

                if (hasInternet)
                    throw new InvalidOperationException("У вас уже подключена интернет-услуга. Сначала отключите её.");
            }

            if (sub.Balance < service.Cost)
                throw new InvalidOperationException("Недостаточно средств на балансе для подключения услуги.");

            sub.Balance -= service.Cost;

            var existing = await _db.SubscriberServices
                .OrderByDescending(x => x.ActivatedDate)
                .FirstOrDefaultAsync(x => x.SubscriberID == sub.SubscriberID && x.ServiceID == serviceId);

            if (existing != null)
            {
                existing.IsActive = true;
                existing.ActivatedDate = DateTime.Now;
            }
            else
            {
                _db.SubscriberServices.Add(new DAL.Entities.SubscriberService
                {
                    SubscriberID = sub.SubscriberID,
                    ServiceID = serviceId,
                    IsActive = true,
                    ActivatedDate = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task DisconnectAsync(string phoneNumber, int serviceId)
        {
            var sub = await _db.Subscribers.FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber);
            if (sub == null) throw new InvalidOperationException("Абонент не найден.");

            var active = await _db.SubscriberServices
                .OrderByDescending(x => x.ActivatedDate)
                .FirstOrDefaultAsync(x => x.SubscriberID == sub.SubscriberID && x.ServiceID == serviceId && x.IsActive);

            if (active == null)
                throw new InvalidOperationException("Услуга не подключена.");

            active.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}
