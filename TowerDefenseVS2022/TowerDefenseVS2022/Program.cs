using System;
using System.Windows.Forms;
using TowerDefenseVS2022.Forms;

namespace TowerDefenseVS2022
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Bỏ đăng nhập -> vào thẳng menu chọn AI
            Application.Run(new MainMenuForm("Player"));
        }
    }
}
