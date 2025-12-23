using Microsoft.EntityFrameworkCore;
using SOPlus37.DAL;
using SOPlus37.DAL.Entities;
using SOPlus37.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOPlus37.Services
{
    public class SubscriberBalanceService
    {
        private readonly MobileOperatorDbContext _db;

        public SubscriberBalanceService(MobileOperatorDbContext db)
        {
            _db = db;
        }

        public async Task<(decimal balance, List<BalanceTopUpItem> history)> GetBalanceAndHistoryAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("PhoneNumber is empty.", nameof(phoneNumber));

            var subscriber = await _db.Subscribers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber);

            if (subscriber == null)
                throw new InvalidOperationException($"Абонент с номером {phoneNumber} не найден.");

            var history = await _db.BalanceReplenishments
                .AsNoTracking()
                .Where(r => r.SubscriberID == subscriber.SubscriberID)
                .OrderByDescending(r => r.ReplenishmentDate)
                .Select(r => new BalanceTopUpItem
                {
                    Date = r.ReplenishmentDate,
                    Amount = r.Amount,
                    Method = r.PaymentMethod
                })
                .ToListAsync();

            return (subscriber.Balance, history);
        }

        public async Task ReplenishAsync(string phoneNumber, decimal amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Сумма пополнения должна быть больше 0.");

            var sub = await _db.Subscribers.FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber);
            if (sub == null)
                throw new InvalidOperationException("Абонент не найден.");

            sub.Balance += amount;

            sub.Status = sub.Balance < 0 ? "BLOCKED" : "ACTIVE";

            _db.BalanceReplenishments.Add(new BalanceReplenishment
            {
                SubscriberID = sub.SubscriberID,
                Amount = amount,
                PaymentMethod = "ELECTRONIC",
                ReplenishmentDate = DateTime.Now
            });

            await _db.SaveChangesAsync();
        }
    }
}
