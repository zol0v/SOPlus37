using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SOPlus37.DAL;
using SOPlus37.Models;

namespace SOPlus37.Services
{
    public class SubscriberSmsService
    {
        private readonly MobileOperatorDbContext _db;

        public SubscriberSmsService(MobileOperatorDbContext db)
        {
            _db = db;
        }

        public async Task<List<SmsItem>> GetSmsAsync(string phoneNumber, DateTime from, DateTime to)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("phoneNumber empty", nameof(phoneNumber));
            if (to < from)
                throw new ArgumentException("to < from");

            var fromDate = from.Date;
            var toExclusive = to.Date.AddDays(1);

            var subscriber = await _db.Subscribers.AsNoTracking()
                .FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber);

            if (subscriber == null)
                throw new InvalidOperationException($"Абонент с номером {phoneNumber} не найден.");

            var list = await _db.Smses.AsNoTracking()
                .Where(s => s.SubscriberID == subscriber.SubscriberID
                            && s.SmsDateTime >= fromDate
                            && s.SmsDateTime < toExclusive)
                .OrderByDescending(s => s.SmsDateTime)
                .Select(s => new SmsItem
                {
                    SmsedNumber = s.SmsedNumber,
                    SmsDateTime = s.SmsDateTime,
                    Cost = s.Cost,
                    IsPaid = s.IsPaid
                })
                .ToListAsync();

            return list;
        }
    }
}
