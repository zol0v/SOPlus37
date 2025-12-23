using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SOPlus37.DAL;
using SOPlus37.DAL.Entities;
using SOPlus37.Models;

namespace SOPlus37.Services
{
    public class SubscriberActionsService
    {
        private readonly MobileOperatorDbContext _db;

        public SubscriberActionsService(MobileOperatorDbContext db)
        {
            _db = db;
        }

        public async Task<List<CallTypeOption>> GetCallTypesAsync()
        {
            return await _db.CallTypes
                .AsNoTracking()
                .OrderBy(ct => ct.CallTypeID)
                .Select(ct => new CallTypeOption
                {
                    CallTypeID = ct.CallTypeID,
                    TypeName = ct.TypeName,
                    DisplayText = string.IsNullOrWhiteSpace(ct.Description) ? ct.TypeName : ct.Description
                })
                .ToListAsync();
        }

        public async Task<decimal> MakeCallAsync(string fromPhoneNumber, string toPhoneNumber, int durationMinutes, int callTypeId)
        {
            if (string.IsNullOrWhiteSpace(fromPhoneNumber))
                throw new ArgumentException("fromPhoneNumber empty", nameof(fromPhoneNumber));
            if (string.IsNullOrWhiteSpace(toPhoneNumber))
                throw new ArgumentException("Укажите номер, кому звонить.");
            if (durationMinutes <= 0)
                throw new ArgumentException("Длительность должна быть больше 0 минут.");

            if (NormalizePhone(fromPhoneNumber) == NormalizePhone(toPhoneNumber))
                throw new InvalidOperationException("Нельзя звонить самому себе.");

            var subscriber = await _db.Subscribers
                .FirstOrDefaultAsync(s => s.PhoneNumber == fromPhoneNumber);

            if (subscriber == null)
                throw new InvalidOperationException($"Абонент с номером {fromPhoneNumber} не найден.");

            await EnsureStatusConsistentAsync(subscriber);
            if (subscriber.Balance < 0 || string.Equals(subscriber.Status, "BLOCKED", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Вы заблокированы, т.к. Ваш баланс отрицательный. Пополните счет, чтобы совершать звонки.");

            var tariff = await _db.Tariffs
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TariffID == subscriber.TariffID && t.IsActive);

            if (tariff == null)
                throw new InvalidOperationException("Тариф абонента не найден.");

            var callType = await _db.CallTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(ct => ct.CallTypeID == callTypeId);

            if (callType == null)
                throw new InvalidOperationException("Тип звонка не найден.");

            var typeName = (callType.TypeName ?? string.Empty).Trim().ToUpperInvariant();
            decimal perMinute;

            switch (typeName)
            {
                case "ПО_ГОРОДУ":
                    perMinute = tariff.CityCallCost;
                    break;
                case "МЕЖГОРОД":
                    perMinute = tariff.IntercityCallCost;
                    break;
                case "МЕЖДУНАРОДНЫЙ":
                    perMinute = tariff.InternationalCallCost;
                    break;
                default:
                    if (callTypeId == 1) perMinute = tariff.CityCallCost;
                    else if (callTypeId == 2) perMinute = tariff.IntercityCallCost;
                    else if (callTypeId == 3) perMinute = tariff.InternationalCallCost;
                    else perMinute = tariff.CityCallCost;
                    break;
            }

            var cost = Math.Round(perMinute * durationMinutes, 2, MidpointRounding.AwayFromZero);

            if (subscriber.Balance < cost)
                throw new InvalidOperationException($"Недостаточно средств. Нужно {cost:N2} ₽, на балансе {subscriber.Balance:N2} ₽.");

            subscriber.Balance -= cost;
            subscriber.Status = subscriber.Balance < 0 ? "BLOCKED" : "ACTIVE";

            var call = new Call
            {
                SubscriberID = subscriber.SubscriberID,
                CalledNumber = toPhoneNumber.Trim(),
                CallDateTime = DateTime.Now,
                Duration = durationMinutes,
                CallTypeID = callTypeId,
                Cost = cost,
                IsPaid = true
            };

            _db.Calls.Add(call);
            await _db.SaveChangesAsync();

            return cost;
        }

        public async Task<decimal> SendSmsAsync(string fromPhoneNumber, string toPhoneNumber)
        {
            if (string.IsNullOrWhiteSpace(fromPhoneNumber))
                throw new ArgumentException("fromPhoneNumber empty", nameof(fromPhoneNumber));
            if (string.IsNullOrWhiteSpace(toPhoneNumber))
                throw new ArgumentException("Укажите номер, кому отправить SMS.");

            if (NormalizePhone(fromPhoneNumber) == NormalizePhone(toPhoneNumber))
                throw new InvalidOperationException("Нельзя отправить SMS самому себе.");

            var subscriber = await _db.Subscribers
                .FirstOrDefaultAsync(s => s.PhoneNumber == fromPhoneNumber);

            if (subscriber == null)
                throw new InvalidOperationException($"Абонент с номером {fromPhoneNumber} не найден.");

            await EnsureStatusConsistentAsync(subscriber);
            if (subscriber.Balance < 0 || string.Equals(subscriber.Status, "BLOCKED", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Вы заблокированы, т.к. Ваш баланс отрицательный. Пополните счет, чтобы отправлять SMS.");

            var tariff = await _db.Tariffs
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TariffID == subscriber.TariffID && t.IsActive);

            if (tariff == null)
                throw new InvalidOperationException("Тариф абонента не найден.");

            var cost = Math.Round(tariff.SmsCost, 2, MidpointRounding.AwayFromZero);

            if (subscriber.Balance < cost)
                throw new InvalidOperationException($"Недостаточно средств. Нужно {cost:N2} ₽, на балансе {subscriber.Balance:N2} ₽.");

            subscriber.Balance -= cost;
            subscriber.Status = subscriber.Balance < 0 ? "BLOCKED" : "ACTIVE";

            var sms = new Sms
            {
                SubscriberID = subscriber.SubscriberID,
                SmsedNumber = toPhoneNumber.Trim(),
                SmsDateTime = DateTime.Now,
                Cost = cost,
                IsPaid = true
            };

            _db.Smses.Add(sms);
            await _db.SaveChangesAsync();

            return cost;
        }

        private async Task EnsureStatusConsistentAsync(Subscriber subscriber)
        {
            var correct = subscriber.Balance < 0 ? "BLOCKED" : "ACTIVE";

            if (!string.Equals(subscriber.Status, correct, StringComparison.OrdinalIgnoreCase))
            {
                subscriber.Status = correct;
                await _db.SaveChangesAsync();
            }
        }

        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.Length == 11 && digits.StartsWith("8"))
                digits = "7" + digits.Substring(1);
            if (digits.Length == 10)
                digits = "7" + digits;

            return digits;
        }
    }
}
