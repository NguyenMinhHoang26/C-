using System;
using System.Collections.Generic;
using System.Drawing;

namespace TowerDefenseVS2022.Game
{
    public class Map
    {
        public int Cols { get; }
        public int Rows { get; }
        public int CellSize { get; }
        public List<Point> PathCells { get; }

        public Map(int cols, int rows, int cellSize, List<Point> pathCells)
        {
            Cols = cols;
            Rows = rows;
            CellSize = cellSize;
            PathCells = pathCells;
        }

        // Các điểm world ở tâm từng ô path (dùng để enemy chạy)
        public List<PointF> BuildPathWorld()
        {
            var pts = new List<PointF>(PathCells.Count);
            foreach (var c in PathCells)
                pts.Add(new PointF(c.X * CellSize + CellSize / 2f, c.Y * CellSize + CellSize / 2f));
            return pts;
        }

        // Polyline world (tối ưu để VẼ đường đi mượt hơn – không bắt buộc)
        public List<PointF> BuildPathPolylineWorld()
        {
            var centers = BuildPathWorld();
            if (centers.Count <= 2) return centers;

            // Rút gọn các điểm thẳng hàng (giữ lại điểm góc) => vẽ đẹp hơn
            var poly = new List<PointF>();
            poly.Add(centers[0]);

            for (int i = 1; i < centers.Count - 1; i++)
            {
                var a = centers[i - 1];
                var b = centers[i];
                var c = centers[i + 1];

                var ab = new PointF(b.X - a.X, b.Y - a.Y);
                var bc = new PointF(c.X - b.X, c.Y - b.Y);

                bool straight =
                    (Math.Sign(ab.X) == Math.Sign(bc.X) && Math.Sign(ab.Y) == Math.Sign(bc.Y)) &&
                    (Math.Abs(ab.X) < 0.001f || Math.Abs(ab.Y) < 0.001f) &&
                    (Math.Abs(bc.X) < 0.001f || Math.Abs(bc.Y) < 0.001f);

                if (!straight) poly.Add(b); // giữ góc
            }

            poly.Add(centers[^1]);
            return poly;
        }

        public bool IsInsideGrid(int gx, int gy) => gx >= 0 && gx < Cols && gy >= 0 && gy < Rows;

        public bool IsOnPath(int gx, int gy)
        {
            for (int i = 0; i < PathCells.Count; i++)
            {
                var p = PathCells[i];
                if (p.X == gx && p.Y == gy) return true;
            }
            return false;
        }

        // ===============================
        // MAP FACTORY
        // ===============================

        public enum MapStyle
        {
            Random,
            SmoothS,
            SmoothU,
            SmoothZ
        }

        public static Map CreateDefault()
        {
            // Bạn đổi style ở đây:
            // - Random: mỗi lần chạy chọn 1 trong 3 map
            // - SmoothS / SmoothU / SmoothZ: cố định 1 kiểu
            return Create(MapStyle.Random);
        }

        public static Map Create(MapStyle style, int? seed = null)
        {
            int cols = 22, rows = 14, cell = 40;

            var rng = seed.HasValue ? new Random(seed.Value) : new Random();

            if (style == MapStyle.Random)
            {
                int pick = rng.Next(0, 3);
                style = pick switch
                {
                    0 => MapStyle.SmoothS,
                    1 => MapStyle.SmoothU,
                    _ => MapStyle.SmoothZ
                };
            }

            List<Point> path = style switch
            {
                MapStyle.SmoothS => BuildSmoothS(cols, rows),
                MapStyle.SmoothU => BuildSmoothU(cols, rows),
                MapStyle.SmoothZ => BuildSmoothZ(cols, rows),
                _ => BuildSmoothS(cols, rows)
            };

            return new Map(cols, rows, cell, path);
        }

        // ===============================
        // MAP PATTERNS (đẹp, cân đối)
        // ===============================

        // Kiểu S: vào trái -> lên -> qua phải -> xuống -> qua phải
        private static List<Point> BuildSmoothS(int cols, int rows)
        {
            // đảm bảo dư chỗ đặt trụ
            int y1 = rows / 2;       // đường giữa
            int yTop = 3;
            int yBot = rows - 4;

            int xLeft = 0;
            int xMid1 = cols / 3;
            int xMid2 = (cols * 2) / 3;
            int xRight = cols - 1;

            var path = new List<Point>();

            AddLine(path, xLeft, y1, xMid1, y1);          // ngang giữa
            AddLine(path, xMid1, y1, xMid1, yTop);        // lên
            AddLine(path, xMid1, yTop, xMid2, yTop);      // ngang trên
            AddLine(path, xMid2, yTop, xMid2, yBot);      // xuống
            AddLine(path, xMid2, yBot, xRight, yBot);     // ngang dưới ra phải

            return path;
        }

        // Kiểu U: vào trái -> lên -> qua phải -> xuống -> qua trái -> ra phải
        private static List<Point> BuildSmoothU(int cols, int rows)
        {
            int yMid = rows / 2;
            int yTop = 2;
            int yBot = rows - 3;

            int xLeft = 0;
            int xA = cols / 4;
            int xB = cols - cols / 4;
            int xRight = cols - 1;

            var path = new List<Point>();

            AddLine(path, xLeft, yMid, xA, yMid);     // vào
            AddLine(path, xA, yMid, xA, yTop);        // lên
            AddLine(path, xA, yTop, xB, yTop);        // ngang trên
            AddLine(path, xB, yTop, xB, yBot);        // xuống
            AddLine(path, xB, yBot, xA, yBot);        // ngang dưới (quay đầu)
            AddLine(path, xA, yBot, xA, yMid + 1);    // lên nhẹ
            AddLine(path, xA, yMid + 1, xRight, yMid + 1); // ra phải

            return path;
        }

        // Kiểu Z: vào trái -> qua phải -> xuống -> qua trái -> xuống -> qua phải
        private static List<Point> BuildSmoothZ(int cols, int rows)
        {
            int yTop = 3;
            int yMid = rows / 2;
            int yBot = rows - 4;

            int xLeft = 0;
            int xRight = cols - 1;
            int xA = cols - 4;
            int xB = 3;

            var path = new List<Point>();

            AddLine(path, xLeft, yTop, xA, yTop);     // ngang trên
            AddLine(path, xA, yTop, xA, yMid);        // xuống
            AddLine(path, xA, yMid, xB, yMid);        // ngang giữa (về trái)
            AddLine(path, xB, yMid, xB, yBot);        // xuống
            AddLine(path, xB, yBot, xRight, yBot);    // ngang dưới ra phải

            return path;
        }

        // ===============================
        // HELPERS
        // ===============================

        private static void AddLine(List<Point> path, int x1, int y1, int x2, int y2)
        {
            int dx = Math.Sign(x2 - x1);
            int dy = Math.Sign(y2 - y1);

            int x = x1, y = y1;

            AddUnique(path, new Point(x, y));

            while (x != x2 || y != y2)
            {
                if (x != x2) x += dx;
                else if (y != y2) y += dy;

                AddUnique(path, new Point(x, y));
            }
        }

        private static void AddUnique(List<Point> path, Point p)
        {
            // tránh trùng
            for (int i = 0; i < path.Count; i++)
                if (path[i].X == p.X && path[i].Y == p.Y) return;

            path.Add(p);
        }
    }
}
