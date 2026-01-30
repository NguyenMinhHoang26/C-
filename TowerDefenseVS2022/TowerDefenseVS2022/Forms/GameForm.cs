using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TowerDefenseVS2022.Game;

namespace TowerDefenseVS2022.Forms
{
    public class GameForm : Form
    {
        private readonly DoubleBufferedPanel _canvas = new() { Dock = DockStyle.Fill };
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 16 };

        // ===== UI controls =====
        private readonly ComboBox _cbBuild = new()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 190
        };

        private readonly Label _lbMoney = new() { AutoSize = true };
        private readonly Label _lbTime = new() { AutoSize = true };
        private readonly Label _lbBuild = new() { AutoSize = true };
        private readonly Label _lbUnlockTower = new() { AutoSize = true };
        private readonly Label _lbUnlockEnemy = new() { AutoSize = true };
        private readonly Label _lbInfo = new() { AutoSize = true };
        private readonly Label _lbTower = new() { AutoSize = true };

        private readonly ProgressBar _pbWave = new() { Height = 10, Style = ProgressBarStyle.Continuous };
        private readonly ProgressBar _pbLives = new() { Height = 10, Style = ProgressBarStyle.Continuous };

        private readonly Button _btnPause = new() { Text = "Tạm dừng" };
        private readonly Button _btnUpgrade = new() { Text = "Nâng cấp" };
        private readonly Button _btnSound = new() { Text = "Âm: ON" };
        private readonly Button _btnRestart = new() { Text = "Restart" };

        private bool _paused = false;

        // ===== Game =====
        private readonly GameState _s;
        private Tower? _selectedTower;
        private readonly Random _rng = new();
        private readonly SoundManager _snd = new();
        private float _elapsedSeconds = 0f;
        private bool _playedGameOver = false;
        private bool _shownWin = false;

        // ===== Build list cache =====
        private int _buildWaveCached = -1;

        // ===== Fullscreen =====
        private bool _isFullscreen = false;
        private FormBorderStyle _prevBorder;
        private FormWindowState _prevState;
        private Rectangle _prevBounds;

        // ===== Map transform (zoom + offset) =====
        private float _zoom = 1f;
        private bool _autoFit = true;
        private PointF _mapOffset = PointF.Empty;

        private class TowerChoice
        {
            public TowerKind Kind { get; }
            public string Text { get; }
            public TowerChoice(TowerKind k)
            {
                Kind = k;
                var p = TowerPresets.Get(k);
                Text = $"{p.Name} ({p.Cost}$)";
            }
        }

        public GameForm(string username, IEnemyAI ai)
        {
            // DPI-safe + font đẹp
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10F);
            KeyPreview = true;

            Text = $"TD - {username} | {ai.Name}";

            _s = new GameState(Map.CreateDefault(), ai);

            // ===== Layout: left canvas + right panel =====
            var right = BuildRightPanel(username, ai);
            Controls.Add(_canvas);
            Controls.Add(right);

            _canvas.Paint += (_, e) => Draw(e.Graphics);
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.Resize += (_, __) => RecalcTransform();

            KeyDown += GameForm_KeyDown;

            EnsureBuildList(force: true);
            UpdateInfo();

            _timer.Tick += (_, __) =>
            {
                if (!_paused) UpdateGame(0.016f);
                _canvas.Invalidate();
                UpdateInfo();
            };

            Shown += (_, __) =>
            {
                BeginInvoke(new Action(() =>
                {
                    SetFullscreen(true);
                    RecalcTransform();
                }));
            };

            _timer.Start();
        }

        private Panel BuildRightPanel(string username, IEnemyAI ai)
        {
            var right = new Panel
            {
                Dock = DockStyle.Right,
                Width = 400,
                Padding = new Padding(14),
                BackColor = Color.FromArgb(245, 246, 250),
                AutoScroll = true
            };

            void StyleLabelTitle(Label lb)
            {
                lb.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            }

            void StyleSmall(Label lb)
            {
                lb.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            }

            _lbMoney.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _lbMoney.ForeColor = Color.FromArgb(16, 122, 66);

            _lbTime.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            _lbBuild.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            StyleSmall(_lbUnlockTower);
            StyleSmall(_lbUnlockEnemy);

            _pbWave.Maximum = GameState.MaxWaves;
            _pbWave.Value = 1;
            _pbLives.Maximum = 10;
            _pbLives.Value = 10;

            // Button style
            void StyleBtn(Button b)
            {
                b.AutoSize = true;
                b.Padding = new Padding(14, 7, 14, 7);
                b.Margin = new Padding(0, 0, 10, 10);
                b.UseVisualStyleBackColor = true;
            }
            StyleBtn(_btnPause);
            StyleBtn(_btnUpgrade);
            StyleBtn(_btnSound);
            StyleBtn(_btnRestart);

            _btnUpgrade.Enabled = false;

            // Events
            _cbBuild.SelectedIndexChanged += (_, __) => UpdateInfo();

            _btnPause.Click += (_, __) =>
            {
                _paused = !_paused;
                _btnPause.Text = _paused ? "Chạy tiếp" : "Tạm dừng";
            };

            _btnSound.Click += (_, __) =>
            {
                _snd.Enabled = !_snd.Enabled;
                _btnSound.Text = _snd.Enabled ? "Âm: ON" : "Âm: OFF";
            };

            _btnUpgrade.Click += (_, __) => UpgradeSelectedTower();

            _btnRestart.Click += (_, __) =>
            {
                var ns = new GameState(Map.CreateDefault(), _s.AI);
                CopyState(ns);

                _selectedTower = null;
                _paused = false;
                _btnPause.Text = "Tạm dừng";

                _elapsedSeconds = 0f;
                _playedGameOver = false;
                _shownWin = false;

                _buildWaveCached = -1;
                EnsureBuildList(force: true);

                UpdateInfo();
                RecalcTransform();
            };

            // ===== Group: Xây trụ =====
            var gbBuild = new GroupBox
            {
                Text = "Xây trụ",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(12)
            };

            var buildRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2
            };
            buildRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            buildRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var lblBuild = new Label
            {
                Text = "Chọn trụ:",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 6, 10, 0)
            };

            _cbBuild.Margin = new Padding(0, 2, 0, 0);

            buildRow.Controls.Add(lblBuild, 0, 0);
            buildRow.Controls.Add(_cbBuild, 1, 0);

            gbBuild.Controls.Add(_lbBuild);
            gbBuild.Controls.Add(_lbUnlockEnemy);
            gbBuild.Controls.Add(_lbUnlockTower);
            gbBuild.Controls.Add(buildRow);

            // sắp xếp top-down trong group
            SetDockTop(_lbUnlockTower, 6);
            SetDockTop(_lbUnlockEnemy, 2);
            SetDockTop(_lbBuild, 8);
            SetDockTop(buildRow, 6);

            // ===== Group: Trạng thái =====
            var gbStatus = new GroupBox
            {
                Text = "Trạng thái",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(12),
                Margin = new Padding(0, 12, 0, 0)
            };

            var lblWave = new Label { Text = "Tiến độ wave", AutoSize = true };
            var lblLives = new Label { Text = "Máu", AutoSize = true };
            StyleLabelTitle(_lbMoney);

            gbStatus.Controls.Add(_lbTime);
            gbStatus.Controls.Add(_pbLives);
            gbStatus.Controls.Add(lblLives);
            gbStatus.Controls.Add(_pbWave);
            gbStatus.Controls.Add(lblWave);
            gbStatus.Controls.Add(_lbMoney);

            SetDockTop(_lbMoney, 6);
            SetDockTop(lblWave, 10);
            SetDockTop(_pbWave, 4);
            SetDockTop(lblLives, 10);
            SetDockTop(_pbLives, 4);
            SetDockTop(_lbTime, 0);

            // ===== Group: Điều khiển =====
            var gbCtrl = new GroupBox
            {
                Text = "Điều khiển",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(12),
                Margin = new Padding(0, 12, 0, 0)
            };

            var btnGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2
            };
            btnGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            btnGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            _btnPause.Anchor = AnchorStyles.Left;
            _btnUpgrade.Anchor = AnchorStyles.Left;
            _btnSound.Anchor = AnchorStyles.Left;
            _btnRestart.Anchor = AnchorStyles.Left;

            btnGrid.Controls.Add(_btnPause, 0, 0);
            btnGrid.Controls.Add(_btnUpgrade, 1, 0);
            btnGrid.Controls.Add(_btnSound, 0, 1);
            btnGrid.Controls.Add(_btnRestart, 1, 1);

            gbCtrl.Controls.Add(btnGrid);

            // ===== Group: Trụ đang chọn =====
            var gbTower = new GroupBox
            {
                Text = "Trụ đang chọn",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(12),
                Margin = new Padding(0, 12, 0, 0)
            };
            _lbTower.Text = "Trụ: (chưa chọn)";
            gbTower.Controls.Add(_lbTower);

            // wrap label theo width
            right.SizeChanged += (_, __) =>
            {
                int max = Math.Max(200, right.Width - 40);
                _lbBuild.MaximumSize = new Size(max, 0);
                _lbUnlockTower.MaximumSize = new Size(max, 0);
                _lbUnlockEnemy.MaximumSize = new Size(max, 0);
                _lbInfo.MaximumSize = new Size(max, 0);
                _lbTower.MaximumSize = new Size(max, 0);
            };

            // ===== Group: Thông tin + hướng dẫn =====
            var gbHelp = new GroupBox
            {
                Text = "Thông tin",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(12),
                Margin = new Padding(0, 12, 0, 0)
            };

            var help = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Text =
                    "Phím tắt:\n" +
                    "- F11: Fullscreen ON/OFF\n" +
                    "- ESC: Thoát fullscreen\n" +
                    "- + / -: Zoom\n" +
                    "- 0: AutoFit\n\n" +
                    "Hướng dẫn:\n" +
                    "- Wave clear thì sang wave mới\n" +
                    "- Tối đa 15 wave là qua màn\n" +
                    "- Mỗi wave mở 1 loại quái mới\n"
            };

            gbHelp.Controls.Add(_lbInfo);
            gbHelp.Controls.Add(help);

            SetDockTop(help, 10);
            SetDockTop(_lbInfo, 0);

            // Stack all groups
            right.Controls.Add(gbHelp);
            right.Controls.Add(gbTower);
            right.Controls.Add(gbCtrl);
            right.Controls.Add(gbStatus);
            right.Controls.Add(gbBuild);

            return right;
        }

        private static void SetDockTop(Control c, int marginTop)
        {
            c.Dock = DockStyle.Top;
            c.Margin = new Padding(0, marginTop, 0, 0);
        }

        // ===== Keyboard =====
        private void GameForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                SetFullscreen(!_isFullscreen);
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Escape && _isFullscreen)
            {
                SetFullscreen(false);
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add)
            {
                _autoFit = false;
                _zoom = MathF.Min(2.5f, _zoom + 0.1f);
                RecalcTransform();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)
            {
                _autoFit = false;
                _zoom = MathF.Max(0.5f, _zoom - 0.1f);
                RecalcTransform();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0)
            {
                _autoFit = true;
                RecalcTransform();
                e.Handled = true;
            }
        }

        private void SetFullscreen(bool fullscreen)
        {
            if (fullscreen == _isFullscreen) return;

            if (fullscreen)
            {
                _prevBorder = FormBorderStyle;
                _prevState = WindowState;
                _prevBounds = Bounds;

                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                TopMost = true;
                _isFullscreen = true;
            }
            else
            {
                TopMost = false;
                FormBorderStyle = _prevBorder;
                WindowState = _prevState;
                Bounds = _prevBounds;
                _isFullscreen = false;
            }

            RecalcTransform();
        }

        // ===== Map transform =====
        private void RecalcTransform()
        {
            int mapW = _s.Map.Cols * _s.Map.CellSize;
            int mapH = _s.Map.Rows * _s.Map.CellSize;

            if (_autoFit)
            {
                float zx = (_canvas.ClientSize.Width - 40f) / Math.Max(1, mapW);
                float zy = (_canvas.ClientSize.Height - 40f) / Math.Max(1, mapH);
                _zoom = MathF.Max(0.6f, MathF.Min(2.2f, MathF.Min(zx, zy)));
            }

            float scaledW = mapW * _zoom;
            float scaledH = mapH * _zoom;

            float ox = Math.Max(0f, (_canvas.ClientSize.Width - scaledW) / 2f);
            float oy = Math.Max(0f, (_canvas.ClientSize.Height - scaledH) / 2f);
            _mapOffset = new PointF(ox, oy);
        }

        // ===== Build list =====
        private void EnsureBuildList(bool force = false)
        {
            if (!force && _buildWaveCached == _s.Wave) return;
            _buildWaveCached = _s.Wave;

            var unlocked = TowerPresets.UnlockedKinds(_s.Wave)
                .Select(k => new TowerChoice(k))
                .ToList();

            _cbBuild.DataSource = null;
            _cbBuild.DisplayMember = null;

            _cbBuild.DataSource = unlocked;
            _cbBuild.DisplayMember = nameof(TowerChoice.Text);
            if (_cbBuild.Items.Count > 0) _cbBuild.SelectedIndex = 0;

            _lbUnlockTower.Text = TowerPresets.UnlockInfoText(_s.Wave);
            _lbUnlockEnemy.Text = EnemyPresets.UnlockText(_s.Wave);
        }

        private TowerKind CurrentBuildKind()
        {
            if (_cbBuild.SelectedItem is TowerChoice tc) return tc.Kind;
            return TowerKind.Basic;
        }

        // ===== Mouse =====
        private void Canvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (_s.Victory) return;

            // Convert screen -> map space
            float mx = (e.X - _mapOffset.X) / _zoom;
            float my = (e.Y - _mapOffset.Y) / _zoom;
            if (mx < 0 || my < 0) return;

            int gx = (int)(mx / _s.Map.CellSize);
            int gy = (int)(my / _s.Map.CellSize);
            if (!_s.Map.IsInsideGrid(gx, gy)) return;

            var grid = new Point(gx, gy);

            // chọn trụ
            var t = _s.Towers.FirstOrDefault(x => x.Grid == grid);
            if (t != null)
            {
                _selectedTower = t;
                UpdateInfo();
                return;
            }

            // đặt trụ
            var kind = CurrentBuildKind();
            bool ok = _s.TryPlaceTower(grid, kind, out var placed);

            if (ok)
            {
                _snd.PlaceTower();
                _selectedTower = placed;
            }
            else
            {
                _snd.Error();
                var p = TowerPresets.Get(kind);
                if (!_s.IsTowerUnlocked(kind))
                    MessageBox.Show($"Trụ {p.Name} mở khóa từ Wave {p.UnlockWave}.", "Chưa mở khóa",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            UpdateInfo();
        }

        private void UpgradeSelectedTower()
        {
            if (_selectedTower == null) return;

            if (!_selectedTower.CanUpgrade)
            {
                _snd.Error();
                MessageBox.Show("Trụ đã MAX!", "Nâng cấp", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int cost = _selectedTower.UpgradeCost;
            if (_s.Money < cost)
            {
                _snd.Error();
                MessageBox.Show($"Không đủ tiền! Cần {cost}$.", "Nâng cấp",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _s.Money -= cost;
            _selectedTower.Upgrade();
            _snd.Upgrade();
            UpdateInfo();
        }

        // ===== Info =====
        private void UpdateInfo()
        {
            EnsureBuildList();

            var bk = CurrentBuildKind();
            var bp = TowerPresets.Get(bk);

            _lbBuild.Text = $"Trụ đặt: {bp.Name} | Giá: {bp.Cost}$ | (Unlock W{bp.UnlockWave})";
            _lbMoney.Text = $"Tiền: {_s.Money}$";

            var ts = TimeSpan.FromSeconds(_elapsedSeconds);
            _lbTime.Text = $"Time: {ts:mm\\:ss} | Wave: {_s.Wave}/{GameState.MaxWaves} | Zoom: {_zoom:0.0}x";

            _pbWave.Maximum = GameState.MaxWaves;
            _pbWave.Value = Math.Max(1, Math.Min(GameState.MaxWaves, _s.Wave));

            _pbLives.Maximum = 10;
            _pbLives.Value = Math.Max(0, Math.Min(10, _s.Lives));

            _lbInfo.Text =
                $"AI: {_s.AI.Name}\n" +
                $"Wave: {_s.Wave}/{GameState.MaxWaves}\n" +
                $"Lives: {_s.Lives}\n" +
                $"Enemies: {_s.Enemies.Count}\n" +
                $"Towers: {_s.Towers.Count}\n" +
                $"Spawn: {_s.SpawnedThisWave}/{_s.TargetEnemiesThisWave}";

            if (_selectedTower == null)
            {
                _lbTower.Text = "Trụ: (chưa chọn)";
                _btnUpgrade.Enabled = false;
            }
            else
            {
                string up = _selectedTower.CanUpgrade
                    ? $"Cấp {_selectedTower.Level} -> {_selectedTower.Level + 1} | Giá: {_selectedTower.UpgradeCost}$"
                    : $"Cấp {_selectedTower.Level} | MAX";

                _lbTower.Text =
                    $"{_selectedTower.Name}\n" +
                    $"- Cấp: {_selectedTower.Level}\n" +
                    $"- Dame: {_selectedTower.Damage:0.##}\n" +
                    $"- Tầm: {_selectedTower.Range:0.##}\n" +
                    $"- Tốc: {_selectedTower.FireRate:0.##}\n" +
                    (_selectedTower.Kind == TowerKind.Splash ? $"- Nổ: {_selectedTower.SplashRadius:0.##}\n" : "") +
                    $"- {up}";

                _btnUpgrade.Enabled = _selectedTower.CanUpgrade && _s.Money >= _selectedTower.UpgradeCost;
            }
        }

        // ===== Game Loop =====
        private void UpdateGame(float dt)
        {
            if (_s.Victory) return;

            if (_s.Lives <= 0)
            {
                if (!_playedGameOver)
                {
                    _playedGameOver = true;
                    _snd.GameOver();
                }
                return;
            }

            _elapsedSeconds += dt;

            // Spawn
            _s.SpawnTimer -= dt;
            if (_s.SpawnedThisWave < _s.TargetEnemiesThisWave && _s.SpawnTimer <= 0f)
            {
                SpawnEnemyForWave();
                _s.SpawnedThisWave++;
                _s.SpawnTimer = _s.AI.GetSpawnIntervalSeconds(_s);
            }

            // Move enemies
            var path = _s.PathWorld;
            for (int i = _s.Enemies.Count - 1; i >= 0; i--)
            {
                var en = _s.Enemies[i];

                if (en.IsDead)
                {
                    _snd.Kill();
                    _s.Money += en.Reward;
                    _s.Enemies.RemoveAt(i);
                    continue;
                }

                if (en.PathIndex >= path.Count - 1)
                {
                    _s.Lives--;
                    _s.Enemies.RemoveAt(i);
                    continue;
                }

                var target = path[en.PathIndex + 1];
                var dx = target.X - en.Pos.X;
                var dy = target.Y - en.Pos.Y;
                var dist = MathF.Sqrt(dx * dx + dy * dy);

                if (dist < 1f) { en.PathIndex++; continue; }

                var vx = dx / dist;
                var vy = dy / dist;
                en.Pos = new PointF(en.Pos.X + vx * en.Speed * dt, en.Pos.Y + vy * en.Speed * dt);
            }

            // Towers fire
            foreach (var t in _s.Towers)
            {
                t.Cooldown -= dt;
                if (t.Cooldown > 0) continue;

                var tw = _s.GridToWorldCenter(t.Grid);
                var target = _s.Enemies
                    .Select(en => new { en, d = Dist(en.Pos, tw) })
                    .Where(x => x.d <= t.Range)
                    .OrderBy(x => x.d)
                    .FirstOrDefault()?.en;

                if (target != null)
                {
                    var dir = Norm(new PointF(target.Pos.X - tw.X, target.Pos.Y - tw.Y));
                    var vel = new PointF(dir.X * t.BulletSpeed, dir.Y * t.BulletSpeed);

                    _s.Bullets.Add(new Bullet(
                        target.Id, tw, vel,
                        t.Damage, t.BulletRadius,
                        t.SplashRadius, t.SplashFactor
                    ));

                    _snd.Shoot();
                    t.Cooldown = 1f / t.FireRate;
                }
            }

            // Bullets
            for (int i = _s.Bullets.Count - 1; i >= 0; i--)
            {
                var b = _s.Bullets[i];
                b.Pos = new PointF(b.Pos.X + b.Vel.X * dt, b.Pos.Y + b.Vel.Y * dt);

                var en = _s.Enemies.FirstOrDefault(x => x.Id == b.TargetId);
                if (en == null)
                {
                    _s.Bullets.RemoveAt(i);
                    continue;
                }

                if (Dist(b.Pos, en.Pos) <= en.Radius + b.Radius + 2f)
                {
                    if (b.SplashRadius > 0f)
                    {
                        for (int k = 0; k < _s.Enemies.Count; k++)
                        {
                            var e2 = _s.Enemies[k];
                            float d = Dist(b.Pos, e2.Pos);
                            if (d <= b.SplashRadius)
                            {
                                float dmg = (e2.Id == en.Id) ? b.Damage : (b.Damage * b.SplashFactor);
                                e2.HP -= dmg;
                            }
                        }
                    }
                    else
                    {
                        en.HP -= b.Damage;
                    }

                    _s.Bullets.RemoveAt(i);
                    continue;
                }
            }

            // Wave clear -> next wave (max 15)
            bool waveCleared = (_s.SpawnedThisWave >= _s.TargetEnemiesThisWave) && (_s.Enemies.Count == 0);

            if (waveCleared)
            {
                if (_s.Wave >= GameState.MaxWaves)
                {
                    _s.Victory = true;
                    if (!_shownWin)
                    {
                        _shownWin = true;
                        _snd.WaveUp();
                        MessageBox.Show("Chúc mừng! Bạn đã qua màn: 15/15 Wave!", "YOU WIN",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                _s.Wave++;
                _snd.WaveUp();
                _s.StartWave();
                EnsureBuildList(force: true);
            }

            if (_s.Lives <= 0 && !_playedGameOver)
            {
                _playedGameOver = true;
                _snd.GameOver();
            }
        }

        private void SpawnEnemyForWave()
        {
            var start = _s.PathWorld[0];

            var kind = EnemyPresets.PickKindForWave(_s.Wave, _rng);
            var p = EnemyPresets.Get(kind);

            float hp = p.BaseHP * (1f + 0.04f * (_s.Wave - 1));
            float sp = p.BaseSpeed * (1f + 0.01f * (_s.Wave - 1));

            var e = new Enemy(kind, hp, sp, p.Reward, p.Radius, start);
            _s.AI.TweakEnemyForWave(_s, e);
            _s.Enemies.Add(e);
        }

        // ===== Drawing (with zoom transform) =====
        private void Draw(Graphics g)
        {
            g.Clear(Color.White);

            int mapW = _s.Map.Cols * _s.Map.CellSize;
            int mapH = _s.Map.Rows * _s.Map.CellSize;

            // Apply transform
            g.TranslateTransform(_mapOffset.X, _mapOffset.Y);
            g.ScaleTransform(_zoom, _zoom);

            // nền nhẹ
            using (var bg = new SolidBrush(Color.FromArgb(250, 250, 252)))
                g.FillRectangle(bg, 0, 0, mapW, mapH);

            // grid
            using var gridPen = new Pen(Color.Gainsboro);
            for (int x = 0; x <= _s.Map.Cols; x++)
                g.DrawLine(gridPen, x * _s.Map.CellSize, 0, x * _s.Map.CellSize, mapH);
            for (int y = 0; y <= _s.Map.Rows; y++)
                g.DrawLine(gridPen, 0, y * _s.Map.CellSize, mapW, y * _s.Map.CellSize);

            // path
            foreach (var pc in _s.Map.PathCells)
            {
                var r = new Rectangle(
                    pc.X * _s.Map.CellSize,
                    pc.Y * _s.Map.CellSize,
                    _s.Map.CellSize, _s.Map.CellSize);

                using var br = new SolidBrush(Color.FromArgb(235, 235, 235));
                g.FillRectangle(br, r);
            }

            // towers
            foreach (var t in _s.Towers)
            {
                var r = new Rectangle(
                    t.Grid.X * _s.Map.CellSize + 8,
                    t.Grid.Y * _s.Map.CellSize + 8,
                    _s.Map.CellSize - 16,
                    _s.Map.CellSize - 16);

                Brush br = t.Kind switch
                {
                    TowerKind.Basic => Brushes.DarkSlateBlue,
                    TowerKind.Sniper => Brushes.DarkGreen,
                    TowerKind.Rapid => Brushes.MediumPurple,
                    TowerKind.Splash => Brushes.DarkOrange,
                    _ => Brushes.DarkSlateBlue
                };

                g.FillEllipse(br, r);

                if (_selectedTower == t)
                {
                    using var pen = new Pen(Color.Gold, 3);
                    g.DrawEllipse(pen, r);
                }

                using var f = new Font("Segoe UI", 8, FontStyle.Bold);
                var s = t.Level.ToString();
                var sz = g.MeasureString(s, f);
                g.DrawString(s, f, Brushes.White,
                    r.X + (r.Width - sz.Width) / 2,
                    r.Y + (r.Height - sz.Height) / 2);
            }

            // enemies
            foreach (var en in _s.Enemies)
            {
                var preset = EnemyPresets.Get(en.Kind);
                using var enemyBrush = new SolidBrush(preset.Color);

                var r = new RectangleF(
                    en.Pos.X - en.Radius,
                    en.Pos.Y - en.Radius,
                    en.Radius * 2, en.Radius * 2);

                g.FillEllipse(enemyBrush, r);

                // HP bar
                float hpPct = MathF.Max(0, en.HP / en.MaxHP);
                var bg = new RectangleF(en.Pos.X - 14, en.Pos.Y - en.Radius - 10, 28, 5);
                var fg = new RectangleF(bg.X, bg.Y, bg.Width * hpPct, bg.Height);
                g.FillRectangle(Brushes.LightGray, bg);
                g.FillRectangle(Brushes.DarkGreen, fg);
            }

            // bullets
            foreach (var b in _s.Bullets)
            {
                var r = new RectangleF(
                    b.Pos.X - b.Radius,
                    b.Pos.Y - b.Radius,
                    b.Radius * 2, b.Radius * 2);

                g.FillEllipse(Brushes.Black, r);
            }

            // overlay win/lose
            if (_s.Victory)
            {
                using var font = new Font("Segoe UI", 28, FontStyle.Bold);
                var s = "YOU WIN (15/15)";
                var size = g.MeasureString(s, font);
                g.DrawString(s, font, Brushes.Black,
                    (mapW - size.Width) / 2,
                    (mapH - size.Height) / 2);
            }
            else if (_s.Lives <= 0)
            {
                using var font = new Font("Segoe UI", 28, FontStyle.Bold);
                var s = "GAME OVER";
                var size = g.MeasureString(s, font);
                g.DrawString(s, font, Brushes.Black,
                    (mapW - size.Width) / 2,
                    (mapH - size.Height) / 2);
            }
        }

        private void CopyState(GameState ns)
        {
            _s.Enemies = ns.Enemies;
            _s.Towers = ns.Towers;
            _s.Bullets = ns.Bullets;
            _s.Money = ns.Money;
            _s.Lives = ns.Lives;
            _s.Wave = ns.Wave;
            _s.SpawnTimer = ns.SpawnTimer;
            _s.SpawnedThisWave = ns.SpawnedThisWave;
            _s.TargetEnemiesThisWave = ns.TargetEnemiesThisWave;
            _s.Victory = ns.Victory;
        }

        private static float Dist(PointF a, PointF b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        private static PointF Norm(PointF v)
        {
            float d = MathF.Sqrt(v.X * v.X + v.Y * v.Y);
            if (d < 1e-5f) return new PointF(1, 0);
            return new PointF(v.X / d, v.Y / d);
        }
    }
}
