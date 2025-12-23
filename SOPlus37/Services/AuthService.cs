using Microsoft.EntityFrameworkCore;
using SOPlus37.DAL;
using SOPlus37.DAL.Entities;
using System.Threading.Tasks;

namespace SOPlus37.Services
{
    public class AuthResult
    {
        public bool Success { get; }
        public string Role { get; }
        public Administrator Admin { get; }
        public Subscriber Subscriber { get; }
        public string Error { get; }

        private AuthResult(bool success, string role, Administrator admin, Subscriber subscriber, string error)
        {
            Success = success;
            Role = role;
            Admin = admin;
            Subscriber = subscriber;
            Error = error;
        }

        public static AuthResult Fail(string error) => new AuthResult(false, null, null, null, error);
        public static AuthResult OkAdmin(Administrator admin) => new AuthResult(true, "Admin", admin, null, null);
        public static AuthResult OkSubscriber(Subscriber sub) => new AuthResult(true, "Subscriber", null, sub, null);
    }

    public class AuthService
    {
        public async Task<AuthResult> LoginAsync(string loginOrPhone, string password, bool loginAsAdmin)
        {
            if (string.IsNullOrWhiteSpace(loginOrPhone) || string.IsNullOrWhiteSpace(password))
                return AuthResult.Fail("Все поля обязательны к заполнению.");

            try
            {
                using (var db = new MobileOperatorDbContext())
                {
                    var canConnect = await db.Database.CanConnectAsync();
                    if (!canConnect)
                        return AuthResult.Fail("Нет подключения к базе данных.");

                    if (loginAsAdmin)
                    {
                        var admin = await db.Administrators.FirstOrDefaultAsync(a =>
                            a.Username == loginOrPhone && a.Password == password && a.IsActive);

                        return admin == null
                            ? AuthResult.Fail("Неверный логин или пароль администратора.")
                            : AuthResult.OkAdmin(admin);
                    }

                    else
                    {
                        var sub = await db.Subscribers
                            .FirstOrDefaultAsync(s => s.PhoneNumber == loginOrPhone && s.Password == password);

                        return sub == null
                            ? AuthResult.Fail("Неверный телефон или пароль абонента.")
                            : AuthResult.OkSubscriber(sub);
                    }
                }
            }
            catch (System.Exception ex)
            {
                return AuthResult.Fail("Ошибка подключения/запроса к БД: " + ex.Message);
            }
        }

    }
}
