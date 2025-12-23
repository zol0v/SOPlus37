using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SOPlus37.DAL;
using SOPlus37.Models;

namespace SOPlus37.Services
{
    public class SubscriberCallsService
    {
        private readonly MobileOperatorDbContext _db;

        public SubscriberCallsService(MobileOperatorDbContext db)
        {
            _db = db;
        }

        public async Task<List<CallItem>> GetCallsAsync(string phoneNumber, DateTime from, DateTime to)
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

            var raw = await (
                from c in _db.Calls.AsNoTracking()
                join ct in _db.CallTypes.AsNoTracking() on c.CallTypeID equals ct.CallTypeID
                where c.SubscriberID == subscriber.SubscriberID
                      && c.CallDateTime >= fromDate
                      && c.CallDateTime < toExclusive
                orderby c.CallDateTime descending
                select new
                {
                    c.CalledNumber,
                    c.CallDateTime,
                    c.Duration,
                    c.Cost,
                    CallTypeText = ct.Description
                }
            ).ToListAsync();

            var result = raw.Select(x => new CallItem
            {
                CalledNumber = x.CalledNumber,
                CallDateTime = x.CallDateTime,
                DurationMinutes = x.Duration,
                CallTypeText = string.IsNullOrWhiteSpace(x.CallTypeText) ? "Неизвестно" : x.CallTypeText,
                Cost = x.Cost
            }).ToList();

            return result;
        }
    }
}
