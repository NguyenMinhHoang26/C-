namespace TowerDefenseVS2022.Auth
{
    public class User
    {
        public string Username { get; set; } = "";
        public string SaltB64 { get; set; } = "";
        public string HashB64 { get; set; } = "";
        public int Iterations { get; set; } = 100_000;
    }
}
