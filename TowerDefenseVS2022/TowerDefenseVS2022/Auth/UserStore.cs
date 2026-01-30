using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TowerDefenseVS2022.Auth
{
    public class UserStore
    {
        private readonly string _path;

        public UserStore()
        {
            _path = Path.Combine(AppContext.BaseDirectory, "users.json");
        }

        public List<User> Load()
        {
            if (!File.Exists(_path)) return new List<User>();
            string json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public void Save(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }

        public bool Register(string username, string password, out string message)
        {
            username = (username ?? "").Trim();
            if (username.Length < 3) { message = "Username tối thiểu 3 ký tự."; return false; }
            if (password.Length < 4) { message = "Password tối thiểu 4 ký tự."; return false; }

            var users = Load();
            if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                message = "Username đã tồn tại.";
                return false;
            }

            var (saltB64, hashB64, iters) = PasswordHasher.HashPassword(password);
            users.Add(new User { Username = username, SaltB64 = saltB64, HashB64 = hashB64, Iterations = iters });
            Save(users);

            message = "Đăng ký thành công!";
            return true;
        }

        public bool Login(string username, string password, out string message)
        {
            username = (username ?? "").Trim();
            var users = Load();
            var u = users.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (u == null) { message = "Sai username hoặc password."; return false; }

            bool ok = PasswordHasher.Verify(password, u.SaltB64, u.HashB64, u.Iterations);
            message = ok ? "OK" : "Sai username hoặc password.";
            return ok;
        }
    }
}
