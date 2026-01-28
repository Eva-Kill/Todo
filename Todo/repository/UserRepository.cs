using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.Repository
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime RegistrationDate { get; set; }

        public UserModel()
        {
            RegistrationDate = DateTime.Now;
        }
    }
    public class UserRepository
    {
        private static List<UserModel> _users = new List<UserModel>();
        private static int _nextId = 1;

        public bool RegisterUser(string username, string password, string email, string fullName = "")
        {
            // проверка уникальности логина
            if (_users.Any(u => u.Login.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Пользователь с таким логином уже существует");
            }

            // проверка уникальности email
            if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Пользователь с таким email уже существует");
            }

            // создание нового пользователя
            var newUser = new UserModel
            {
                Id = _nextId++,
                Login = username.Trim(),
                Password = password,
                Email = email.Trim(),
                FullName = fullName?.Trim() ?? "",
                RegistrationDate = DateTime.Now
            };

            _users.Add(newUser);
            return true;
        }

        public UserModel Authenticate(string login, string password)
        {
            return _users.FirstOrDefault(u =>
                u.Login.Equals(login, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);
        }

        public static List<UserModel> GetAllUsers()
        {
            return _users;
        }

        // метод для проверки существования пользователя
        public bool UserExists(string username)
        {
            return _users.Any(u => u.Login.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        // метод для проверки существования email
        public bool EmailExists(string email)
        {
            return _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}
