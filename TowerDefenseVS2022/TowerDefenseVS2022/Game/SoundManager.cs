using System;
using System.Media;

namespace TowerDefenseVS2022.Game
{
    public class SoundManager
    {
        public bool Enabled { get; set; } = true;

        private DateTime _lastShot = DateTime.MinValue;
        private DateTime _lastKill = DateTime.MinValue;

        public void PlaceTower() => Play(SystemSounds.Asterisk);
        public void Upgrade() => Play(SystemSounds.Exclamation);
        public void Error() => Play(SystemSounds.Hand);
        public void WaveUp() => Play(SystemSounds.Question);
        public void GameOver() => Play(SystemSounds.Hand);

        public void Shoot()
        {
            if (!Enabled) return;
            if ((DateTime.UtcNow - _lastShot).TotalMilliseconds < 120) return;
            _lastShot = DateTime.UtcNow;
            SystemSounds.Beep.Play();
        }

        public void Kill()
        {
            if (!Enabled) return;
            if ((DateTime.UtcNow - _lastKill).TotalMilliseconds < 150) return;
            _lastKill = DateTime.UtcNow;
            SystemSounds.Asterisk.Play();
        }

        private void Play(SystemSound sound)
        {
            if (!Enabled) return;
            sound.Play();
        }
    }
}
