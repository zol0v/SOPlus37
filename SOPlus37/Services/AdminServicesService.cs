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
    public class AdminServicesService
    {
        private readonly MobileOperatorDbContext _db;

        public AdminServicesService(MobileOperatorDbContext db)
        {
            _db = db;
        }

        public async Task<List<AdminServiceItem>> GetAllAsync()
        {
            var services = await _db.Services
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.ServiceID)
                .ToListAsync();

            return services.Select(s => new AdminServiceItem
            {
                ServiceID = s.ServiceID,
                ServiceName = s.ServiceName,
                ServiceType = s.ServiceType,
                Cost = s.Cost,
                IsRecurring = s.IsRecurring,
                Description = s.Description,
                CreatedByAdminID = s.CreatedByAdminID
            }).ToList();
        }

        public async Task AddAsync(
            int adminId,
            string name,
            string type,
            decimal cost,
            string description)
        {
            if (adminId <= 0)
                throw new InvalidOperationException("Не удалось определить администратора (adminId).");

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Название услуги обязательно.");

            if (string.IsNullOrWhiteSpace(type))
                throw new InvalidOperationException("Тип услуги обязателен.");

            if (cost < 0)
                throw new InvalidOperationException("Стоимость не может быть отрицательной.");

            if (description == null)
            {
                description = "";
            }

            if (description.Length > 500)
                throw new InvalidOperationException("Описание слишком длинное (макс. 500 символов).");

            var service = new Service
            {
                ServiceName = name.Trim(),
                ServiceType = type.Trim().ToUpperInvariant(),
                Cost = Math.Round(cost, 2, MidpointRounding.AwayFromZero),
                IsRecurring = true,
                IsActive = true,
                Description = description.Trim(),
                CreatedByAdminID = adminId
            };

            _db.Services.Add(service);
            await _db.SaveChangesAsync();
        }
    }
}