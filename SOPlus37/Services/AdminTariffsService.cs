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
    public class AdminTariffsService
    {
        private readonly MobileOperatorDbContext _db;

        public AdminTariffsService(MobileOperatorDbContext db)
        {
            _db = db;
        }

        public async Task<List<AdminTariffItem>> GetAllAsync()
        {
            var tariffs = await _db.Tariffs
                .AsNoTracking()
                .OrderBy(t => t.TariffID)
                .ToListAsync();

            return tariffs.Select(t => new AdminTariffItem
            {
                TariffID = t.TariffID,
                TariffName = t.TariffName,
                CityCallCost = t.CityCallCost,
                IntercityCallCost = t.IntercityCallCost,
                InternationalCallCost = t.InternationalCallCost,
                SmsCost = t.SmsCost,
                SwitchCost = t.SwitchCost,
                IsActive = t.IsActive,
                CreatedByAdminID = t.CreatedByAdminID
            }).ToList();
        }

        public async Task AddAsync(int adminId,
            string name,
            decimal cityCost,
            decimal intercityCost,
            decimal internationalCost,
            decimal smsCost,
            decimal switchCost)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Название тарифа обязательно.");

            if (adminId <= 0)
                throw new InvalidOperationException("Не удалось определить администратора (adminId).");

            if (cityCost < 0 || intercityCost < 0 || internationalCost < 0 || smsCost < 0 || switchCost < 0)
                throw new InvalidOperationException("Стоимость не может быть отрицательной.");

            var tariff = new Tariff
            {
                TariffName = name.Trim(),
                CityCallCost = cityCost,
                IntercityCallCost = intercityCost,
                InternationalCallCost = internationalCost,
                SmsCost = smsCost,
                SwitchCost = switchCost,
                IsActive =  true,
                CreatedByAdminID = adminId
            };

            _db.Tariffs.Add(tariff);
            await _db.SaveChangesAsync();
        }
    }
}
