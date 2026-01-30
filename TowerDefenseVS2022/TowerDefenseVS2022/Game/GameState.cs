using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TowerDefenseVS2022.Game
{
    public class GameState
    {
        public const int MaxWaves = 15;

        public Map Map { get; }
        public List<PointF> PathWorld { get; }

        public List<Enemy> Enemies = new();
        public List<Tower> Towers = new();
        public List<Bullet> Bullets = new();

        public int Money = 200;
        public int Lives = 10;
        public int Wave = 1;

        public bool Victory = false;

        public IEnemyAI AI { get; }

        public float SpawnTimer = 0f;
        public int SpawnedThisWave = 0;
        public int TargetEnemiesThisWave = 0;

        public GameState(Map map, IEnemyAI ai)
        {
            Map = map;
            PathWorld = map.BuildPathWorld();
            AI = ai;

            StartWave(); // wave 1
        }

        public void StartWave()
        {
            SpawnedThisWave = 0;
            TargetEnemiesThisWave = AI.EnemiesThisWave(this);
            SpawnTimer = 0f;
        }

        public PointF GridToWorldCenter(Point grid) =>
            new PointF(grid.X * Map.CellSize + Map.CellSize / 2f, grid.Y * Map.CellSize + Map.CellSize / 2f);

        public bool CanPlaceTower(Point grid)
        {
            if (!Map.IsInsideGrid(grid.X, grid.Y)) return false;
            if (Map.IsOnPath(grid.X, grid.Y)) return false;
            if (Towers.Any(t => t.Grid == grid)) return false;
            return true;
        }

        public bool IsTowerUnlocked(TowerKind kind) => TowerPresets.IsUnlocked(kind, Wave);

        public bool TryPlaceTower(Point grid, TowerKind kind, out Tower? placed)
        {
            placed = null;

            if (!IsTowerUnlocked(kind)) return false;

            int cost = TowerPresets.Get(kind).Cost;
            if (Money < cost) return false;
            if (!CanPlaceTower(grid)) return false;

            Money -= cost;
            placed = new Tower(grid, kind);
            Towers.Add(placed);
            return true;
        }
    }
}
