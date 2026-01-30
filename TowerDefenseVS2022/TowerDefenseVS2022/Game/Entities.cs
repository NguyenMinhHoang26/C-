using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TowerDefenseVS2022.Game
{
    public enum TowerKind
    {
        Basic,
        Sniper,
        Rapid,
        Splash
    }

    // Tower
    public record TowerPreset(
        string Name,
        int Cost,
        int UnlockWave,
        float Range,
        float FireRate,
        float Damage,
        float BulletSpeed,
        float BulletRadius,
        float SplashRadius,
        float SplashFactor
    );

    public static class TowerPresets
    {
        private static readonly Dictionary<TowerKind, TowerPreset> _presets = new()
        {
            [TowerKind.Basic] = new("cơ bản", 80, 1, 120f, 2.5f, 18f, 320f, 4f, 0f, 0f),
            [TowerKind.Rapid] = new("nhanh", 95, 2, 110f, 5.5f, 9f, 340f, 3f, 0f, 0f),
            [TowerKind.Sniper] = new("bắn xa", 110, 4, 190f, 1.2f, 40f, 520f, 3f, 0f, 0f),
            [TowerKind.Splash] = new("cận chiến", 120, 6, 130f, 1.8f, 22f, 300f, 4f, 55f, 0.55f),
        };

        public static TowerPreset Get(TowerKind kind) => _presets[kind];
        public static bool IsUnlocked(TowerKind kind, int wave) => wave >= Get(kind).UnlockWave;

        public static List<TowerKind> UnlockedKinds(int wave) =>
            Enum.GetValues(typeof(TowerKind)).Cast<TowerKind>()
                .Where(k => IsUnlocked(k, wave))
                .ToList();

        public static string UnlockInfoText(int wave)
        {
            var all = Enum.GetValues(typeof(TowerKind)).Cast<TowerKind>()
                .Select(k => Get(k))
                .OrderBy(p => p.UnlockWave)
                .ToList();
            return "Mở khóa trụ: " + string.Join(" | ", all.Select(p => $"{p.Name}(W{p.UnlockWave})"));
        }
    }

    // ===== Enemy types (mỗi wave mở 1 loại) =====
    public enum EnemyKind
    {
        Lv1 = 1, Lv2, Lv3, Lv4, Lv5, Lv6, Lv7, Lv8, Lv9, Lv10, Lv11, Lv12, Lv13, Lv14, Lv15
    }

    public record EnemyPreset(
        string Name,
        float BaseHP,
        float BaseSpeed,
        int Reward,
        float Radius,
        Color Color
    );

    public static class EnemyPresets
    {
        public const int MaxEnemyKinds = 15;

        private static readonly Dictionary<EnemyKind, EnemyPreset> _p = new()
        {
            [EnemyKind.Lv1] = new("Quái Lv1", 55, 55, 10, 12, Color.IndianRed),
            [EnemyKind.Lv2] = new("Quái Lv2", 65, 56, 11, 12, Color.Salmon),
            [EnemyKind.Lv3] = new("Quái Lv3", 75, 58, 12, 12, Color.OrangeRed),
            [EnemyKind.Lv4] = new("Quái Lv4", 90, 60, 13, 12, Color.Orange),
            [EnemyKind.Lv5] = new("Quái Lv5", 105, 60, 14, 12, Color.Goldenrod),
            [EnemyKind.Lv6] = new("Quái Lv6", 120, 61, 15, 12, Color.OliveDrab),
            [EnemyKind.Lv7] = new("Quái Lv7", 140, 62, 16, 12, Color.SeaGreen),
            [EnemyKind.Lv8] = new("Quái Lv8", 160, 63, 17, 12, Color.Teal),
            [EnemyKind.Lv9] = new("Quái Lv9", 180, 64, 18, 12, Color.SteelBlue),
            [EnemyKind.Lv10] = new("Quái Lv10", 200, 65, 19, 13, Color.RoyalBlue),
            [EnemyKind.Lv11] = new("Quái Lv11", 220, 66, 20, 13, Color.SlateBlue),
            [EnemyKind.Lv12] = new("Quái Lv12", 235, 67, 21, 13, Color.MediumPurple),
            [EnemyKind.Lv13] = new("Quái Lv13", 250, 68, 22, 13, Color.DarkViolet),
            [EnemyKind.Lv14] = new("Quái Lv14", 270, 69, 24, 14, Color.DarkMagenta),
            [EnemyKind.Lv15] = new("Quái Lv15 (Boss)", 310, 70, 28, 15, Color.Black),
        };

        public static EnemyPreset Get(EnemyKind k) => _p[k];

        public static EnemyKind NewKindAtWave(int wave)
        {
            int w = Math.Clamp(wave, 1, MaxEnemyKinds);
            return (EnemyKind)w;
        }

        public static List<EnemyKind> UnlockedKinds(int wave)
        {
            int w = Math.Clamp(wave, 1, MaxEnemyKinds);
            var list = new List<EnemyKind>();
            for (int i = 1; i <= w; i++) list.Add((EnemyKind)i);
            return list;
        }

        // Cho chắc chắn “wave nào cũng thấy quái mới”
        public static EnemyKind PickKindForWave(int wave, Random rng)
        {
            int w = Math.Clamp(wave, 1, MaxEnemyKinds);
            var newest = (EnemyKind)w;

            // 35% ra quái mới của wave hiện tại
            if (rng.NextDouble() < 0.35) return newest;

            // còn lại random trong các quái đã mở
            int pick = rng.Next(1, w + 1);
            return (EnemyKind)pick;
        }

        public static string UnlockText(int wave)
        {
            var newest = Get(NewKindAtWave(wave)).Name;
            return $"Quái mới mở (Wave {wave}): {newest}";
        }
    }

    // ===== Entities =====
    public class Enemy
    {
        public Guid Id { get; } = Guid.NewGuid();

        public EnemyKind Kind { get; }
        public int Reward { get; }

        public float HP;
        public float MaxHP;
        public float Speed;
        public float Radius;

        public int PathIndex = 0;
        public PointF Pos;

        public bool IsDead => HP <= 0;

        public Enemy(EnemyKind kind, float hp, float speed, int reward, float radius, PointF start)
        {
            Kind = kind;
            HP = hp;
            MaxHP = hp;
            Speed = speed;
            Reward = reward;
            Radius = radius;
            Pos = start;
        }
    }

    public class Tower
    {
        public Point Grid;

        public TowerKind Kind { get; }
        public string Name => TowerPresets.Get(Kind).Name;

        public int Level { get; private set; } = 1;
        public const int MaxLevel = 5;

        public float Range;
        public float FireRate;
        public float Damage;
        public float BulletSpeed;
        public float BulletRadius;

        // Splash-only
        public float SplashRadius;
        public float SplashFactor;

        public float Cooldown = 0f;

        public int BaseCost => TowerPresets.Get(Kind).Cost;
        public int UpgradeCost => (int)MathF.Round(BaseCost * 0.75f * Level);
        public bool CanUpgrade => Level < MaxLevel;

        public Tower(Point grid, TowerKind kind)
        {
            Grid = grid;
            Kind = kind;

            var p = TowerPresets.Get(kind);
            Range = p.Range;
            FireRate = p.FireRate;
            Damage = p.Damage;
            BulletSpeed = p.BulletSpeed;
            BulletRadius = p.BulletRadius;
            SplashRadius = p.SplashRadius;
            SplashFactor = p.SplashFactor;
        }

        public void Upgrade()
        {
            if (!CanUpgrade) return;

            Level++;
            var p = TowerPresets.Get(Kind);

            Damage += p.Damage * 0.35f;
            Range += p.Range * 0.10f;
            FireRate += p.FireRate * 0.08f;

            if (Kind == TowerKind.Splash)
            {
                SplashRadius += 6f;
                SplashFactor = MathF.Min(0.75f, SplashFactor + 0.03f);
            }
            if (Kind == TowerKind.Sniper) Range += 8f;
            if (Kind == TowerKind.Rapid) FireRate += 0.25f;
        }
    }

    public class Bullet
    {
        public Guid TargetId;
        public PointF Pos;
        public PointF Vel;
        public float Damage;
        public float Radius;
        public float SplashRadius;
        public float SplashFactor;

        public Bullet(Guid targetId, PointF pos, PointF vel, float dmg, float radius, float splashRadius, float splashFactor)
        {
            TargetId = targetId;
            Pos = pos;
            Vel = vel;
            Damage = dmg;
            Radius = radius;
            SplashRadius = splashRadius;
            SplashFactor = splashFactor;
        }
    }
}
