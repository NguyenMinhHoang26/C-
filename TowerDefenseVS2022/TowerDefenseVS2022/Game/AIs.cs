using System;
using System.Collections.Generic;

namespace TowerDefenseVS2022.Game
{
    public interface IEnemyAI
    {
        string Name { get; }
        float GetSpawnIntervalSeconds(GameState s);
        void TweakEnemyForWave(GameState s, Enemy e);
        int EnemiesThisWave(GameState s);
    }

    public static class AIRegistry
    {
        public static List<IEnemyAI> All() => new()
        {
            new AIEasy(),
            new AINormal(),
            new AIHard(),
            new AIRush(),
            new AITank(),
            new AISwarm(),
            new AISniper(),
            new AIAdaptive(),
            new AIRandom(),
            new AIBoss()
        };
    }

    public class AIEasy : IEnemyAI
    {
        public string Name => "Ải 01 - Easy";
        public float GetSpawnIntervalSeconds(GameState s) => 1.2f;
        public void TweakEnemyForWave(GameState s, Enemy e) { e.HP += s.Wave * 2; e.Speed += s.Wave * 0.2f; }
        public int EnemiesThisWave(GameState s) => 8 + s.Wave * 2;
    }

    public class AINormal : IEnemyAI
    {
        public string Name => "Ải 02 - Normal";
        public float GetSpawnIntervalSeconds(GameState s) => 1.0f;
        public void TweakEnemyForWave(GameState s, Enemy e) { e.HP += s.Wave * 4; e.Speed += s.Wave * 0.3f; }
        public int EnemiesThisWave(GameState s) => 10 + s.Wave * 3;
    }

    public class AIHard : IEnemyAI
    {
        public string Name => "Ải 03 - Hard";
        public float GetSpawnIntervalSeconds(GameState s) => 0.85f;
        public void TweakEnemyForWave(GameState s, Enemy e) { e.HP += s.Wave * 6; e.Speed += s.Wave * 0.4f; }
        public int EnemiesThisWave(GameState s) => 12 + s.Wave * 4;
    }

    public class AIRush : IEnemyAI
    {
        public string Name => "AI 04 - Rush";
        public float GetSpawnIntervalSeconds(GameState s) => MathF.Max(0.45f, 0.9f - s.Wave * 0.03f);
        public void TweakEnemyForWave(GameState s, Enemy e) { e.HP += s.Wave * 4; e.Speed += 10f + s.Wave * 0.6f; }
        public int EnemiesThisWave(GameState s) => 14 + s.Wave * 5;
    }

    public class AITank : IEnemyAI
    {
        public string Name => "AI 05 - Tank";
        public float GetSpawnIntervalSeconds(GameState s) => 1.1f;
        public void TweakEnemyForWave(GameState s, Enemy e) { e.HP += 25f + s.Wave * 10; e.Speed += s.Wave * 0.25f; }
        public int EnemiesThisWave(GameState s) => 9 + s.Wave * 2;
    }

    public class AISwarm : IEnemyAI
    {
        public string Name => "AI 06 - Swarm";
        public float GetSpawnIntervalSeconds(GameState s) => 0.6f;
        public void TweakEnemyForWave(GameState s, Enemy e) { e.HP += s.Wave * 2; e.Speed += 8f + s.Wave * 0.5f; }
        public int EnemiesThisWave(GameState s) => 18 + s.Wave * 6;
    }

    public class AISniper : IEnemyAI
    {
        public string Name => "AI 07 - Sniper";
        public float GetSpawnIntervalSeconds(GameState s) => 1.4f;
        public void TweakEnemyForWave(GameState s, Enemy e) { e.HP += s.Wave * 5; e.Speed += 18f + s.Wave * 0.7f; }
        public int EnemiesThisWave(GameState s) => 7 + s.Wave * 2;
    }

    public class AIAdaptive : IEnemyAI
    {
        public string Name => "AI 08 - Adaptive";
        public float GetSpawnIntervalSeconds(GameState s)
        {
            if (s.Money > 220) return 0.75f;
            if (s.Lives <= 5) return 1.25f;
            return 0.95f;
        }
        public void TweakEnemyForWave(GameState s, Enemy e)
        {
            float factor = 1f + (s.Money / 500f);
            e.HP += s.Wave * 5 * factor;
            e.Speed += (s.Wave * 0.35f) + (s.Money > 250 ? 6 : 0);
        }
        public int EnemiesThisWave(GameState s) => 11 + s.Wave * 3;
    }

    public class AIRandom : IEnemyAI
    {
        public string Name => "AI 09 - Random";
        private readonly Random _r = new();
        public float GetSpawnIntervalSeconds(GameState s) => 0.7f + (float)_r.NextDouble() * 0.8f;
        public void TweakEnemyForWave(GameState s, Enemy e)
        {
            e.HP += s.Wave * (2 + _r.Next(0, 8));
            e.Speed += _r.Next(0, 20) + s.Wave * 0.25f;
        }
        public int EnemiesThisWave(GameState s) => 8 + s.Wave * 4;
    }

    public class AIBoss : IEnemyAI
    {
        public string Name => "Ải 10 - Boss";
        public float GetSpawnIntervalSeconds(GameState s) => 1.0f;
        public void TweakEnemyForWave(GameState s, Enemy e)
        {
            bool boss = (s.SpawnedThisWave == s.TargetEnemiesThisWave - 1);
            if (boss)
            {
                e.HP += 120 + s.Wave * 35;
                e.Speed += 6 + s.Wave * 0.2f;
                e.Radius = 16f;
            }
            else
            {
                e.HP += s.Wave * 4;
                e.Speed += s.Wave * 0.3f;
            }
        }
        public int EnemiesThisWave(GameState s) => 10 + s.Wave * 2;
    }
}
