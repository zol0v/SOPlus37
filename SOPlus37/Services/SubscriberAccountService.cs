using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SOPlus37.DAL;
using SOPlus37.Models;

namespace SOPlus37.Services
{
    public class SubscriberAccountService
    {
        private readonly MobileOperatorDbContext _db;

        public SubscriberAccountService(MobileOperatorDbContext db)
        {
            _db = db;
        }

        public async Task<SubscriberAccountInfo> GetAccountInfoByPhoneAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("PhoneNumber is empty.", nameof(phoneNumber));

            var subscriber = await _db.Subscribers
                .AsNoTracking()
                .Include(s => s.Individual)
                .Include(s => s.LegalEntity)
                .FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber);

            if (subscriber == null)
                throw new InvalidOperationException($"Абонент с номером {phoneNumber} не найден.");

            var isLegal = subscriber.LegalEntityID.HasValue;

            return new SubscriberAccountInfo
            {
                PhoneNumber = subscriber.PhoneNumber,
                Status = subscriber.Status,
                ClientType = isLegal ? "Юридическое лицо" : "Физическое лицо",
                DisplayName = isLegal
                    ? subscriber.LegalEntity?.OrganizationName
                    : subscriber.Individual?.FullName,
                IdentityDocument = isLegal
                    ? subscriber.LegalEntity?.OGRN
                    : subscriber.Individual?.PassportData
            };
        }
    }
}
