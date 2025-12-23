using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SOPlus37.DAL;
using SOPlus37.Models;

namespace SOPlus37.Services
{
    public class SubscriberTariffService
    {
        private readonly MobileOperatorDbContext _db;

        public SubscriberTariffService(MobileOperatorDbContext db)
        {
            _db = db;
        }

        private static bool IsCorporateByName(string name) =>
            !string.IsNullOrWhiteSpace(name) &&
            name.StartsWith("Корпоратив", StringComparison.OrdinalIgnoreCase);

        public async Task<(TariffItem current, List<TariffItem> available, bool isLegal)> GetTariffsAsync(string phoneNumber)
        {
            var s = await _db.Subscribers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);

            if (s == null) throw new InvalidOperationException("Абонент не найден.");

            bool isLegal = s.LegalEntityID.HasValue;
            bool mustBeCorporate = isLegal;

            var currentTariff = await _db.Tariffs.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TariffID == s.TariffID);

            if (currentTariff == null) throw new InvalidOperationException("Текущий тариф не найден.");

            var current = Map(currentTariff);

            var tariffs = await _db.Tariffs.AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.TariffName)
                .ToListAsync();

            var available = tariffs
                .Where(t => IsCorporateByName(t.TariffName) == mustBeCorporate)
                .Select(Map)
                .ToList();

            return (current, available, isLegal);
        }

        public async Task ChangeTariffAsync(string phoneNumber, int newTariffId)
        {
            var s = await _db.Subscribers.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
            if (s == null) throw new InvalidOperationException("Абонент не найден.");

            bool isLegal = s.LegalEntityID.HasValue;
            bool mustBeCorporate = isLegal;

            var newTariff = await _db.Tariffs.FirstOrDefaultAsync(t => t.TariffID == newTariffId);
            if (newTariff == null) throw new InvalidOperationException("Выбранный тариф не найден.");

            if (!newTariff.IsActive)
                throw new InvalidOperationException("Тариф не активен.");

            if (IsCorporateByName(newTariff.TariffName) != mustBeCorporate)
                throw new InvalidOperationException(isLegal
                    ? "Юр.лицам доступны только корпоративные тарифы."
                    : "Физ.лицам доступны только некорпоративные тарифы.");

            if (s.TariffID == newTariffId)
                throw new InvalidOperationException("Этот тариф уже подключен.");

            if (s.Balance < newTariff.SwitchCost)
                throw new InvalidOperationException("Недостаточно средств для смены тарифа.");

            s.Balance -= newTariff.SwitchCost;
            s.TariffID = newTariffId;

            await _db.SaveChangesAsync();
        }

        private static TariffItem Map(DAL.Entities.Tariff t) => new TariffItem
        {
            TariffID = t.TariffID,
            TariffName = t.TariffName,
            SwitchCost = t.SwitchCost,
            CityCallCost = t.CityCallCost,
            IntercityCallCost = t.IntercityCallCost,
            InternationalCallCost = t.InternationalCallCost,
            SmsCost = t.SmsCost,
            IsCorporate = IsCorporateByName(t.TariffName)
        };
    }
}
