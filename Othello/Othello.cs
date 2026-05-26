using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace Othello
{
    public partial class Form1 : Form
    {
        private SoundPlayer welcomeSound;
        private SoundPlayer playSound;
        private SoundPlayer winner_bSound;
        private SoundPlayer winner_wSound;
        private SoundPlayer ruleSound;

        private const int BoardSize = 8;
        private int cellSize;

        // 0 = 空格，1 = 黑棋，2 = 白棋
        private int[,] board = new int[BoardSize, BoardSize];

        // 黑棋先手
        private int currentPlayer = 1;

        private bool gameOver = false;

        private readonly int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
        private readonly int[] dc = { -1, 0, 1, -1, 1, -1, 0, 1 };

        private readonly Random random = new Random();

        private void LoadSounds()
        {
            welcomeSound = CreateSoundPlayer("welcome.wav");
            playSound = CreateSoundPlayer("play.wav");
            winner_bSound = CreateSoundPlayer("black_win.wav");
            winner_wSound = CreateSoundPlayer("white_win.wav");
            ruleSound = CreateSoundPlayer("rule.wav");
        }

        private SoundPlayer CreateSoundPlayer(string fileName)
        {
            string path = Path.Combine(Application.StartupPath, "Sounds", fileName);

            if (File.Exists(path))
            {
                SoundPlayer player = new SoundPlayer(path);
                player.LoadAsync();
                return player;
            }

            return null;
        }

        private void PlaySound(SoundPlayer player)
        {
            try
            {
                if (player != null)
                {
                    player.Stop();
                    player.Play();
                }
            }
            catch
            {
                // 音效播放失敗時不讓遊戲當掉
            }
        }
        private class GameState
        {
            public int[,] BoardSnap { get; set; }
            public int Player { get; set; }
        }

        private Stack<GameState> history = new Stack<GameState>();

        public Form1()
        {
            InitializeComponent();

            LoadSounds();

            pnlBoard.BackColor = Color.FromArgb(34, 139, 70);

            mode.DropDownStyle = ComboBoxStyle.DropDownList;
            if (mode.Items.Count > 0)
            {
                mode.SelectedIndex = 0;
            }

            timerAI.Tick += timerAI_Tick;
            mode.SelectedIndexChanged += mode_SelectedIndexChanged;

            InitGame();
        }

        private void InitGame()
        {
            timerAI.Stop();

            board = new int[BoardSize, BoardSize];
            currentPlayer = 1;
            gameOver = false;
            history.Clear();

            int mid = BoardSize / 2;

            board[mid - 1, mid - 1] = 2;
            board[mid - 1, mid] = 1;
            board[mid, mid - 1] = 1;
            board[mid, mid] = 2;

            UpdateUI();
            pnlBoard.Invalidate();

            PlaySound(welcomeSound);
        }

        private int GetCellSize()
        {
            int size = Math.Min(pnlBoard.Width, pnlBoard.Height) / BoardSize;
            if (size <= 0)
            {
                size = 1;
            }

            return size;
        }

        private void pnlBoard_Paint(object sender, PaintEventArgs e)
        {
            cellSize = GetCellSize();

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int boardPixelSize = BoardSize * cellSize;

            using (SolidBrush boardBrush = new SolidBrush(Color.FromArgb(34, 139, 70)))
            {
                g.FillRectangle(boardBrush, 0, 0, boardPixelSize, boardPixelSize);
            }

            using (Pen gridPen = new Pen(Color.FromArgb(0, 80, 40), 1))
            {
                for (int i = 0; i <= BoardSize; i++)
                {
                    g.DrawLine(gridPen, i * cellSize, 0, i * cellSize, boardPixelSize);
                    g.DrawLine(gridPen, 0, i * cellSize, boardPixelSize, i * cellSize);
                }
            }

            DrawStarPoints(g);
            DrawPiecesAndHints(g);
        }

        private void DrawStarPoints(Graphics g)
        {
            int[] starCells = { 2, 5 };

            foreach (int r in starCells)
            {
                foreach (int c in starCells)
                {
                    int x = c * cellSize + cellSize / 2 - 4;
                    int y = r * cellSize + cellSize / 2 - 4;

                    g.FillEllipse(Brushes.DarkGreen, x, y, 8, 8);
                }
            }
        }

        private void DrawPiecesAndHints(Graphics g)
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    int x = col * cellSize + 4;
                    int y = row * cellSize + 4;
                    int size = cellSize - 8;

                    if (size <= 0)
                    {
                        continue;
                    }

                    if (board[row, col] == 1)
                    {
                        DrawBlackPiece(g, x, y, size);
                    }
                    else if (board[row, col] == 2)
                    {
                        DrawWhitePiece(g, x, y, size);
                    }
                    else if (!gameOver && !IsAITurn() && IsValidMove(row, col, currentPlayer))
                    {
                        DrawHint(g, x, y, size);
                    }
                }
            }
        }

        private void DrawBlackPiece(Graphics g, int x, int y, int size)
        {
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
            {
                g.FillEllipse(shadow, x + 2, y + 2, size, size);
            }

            g.FillEllipse(Brushes.Black, x, y, size, size);

            using (SolidBrush highlight = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
            {
                g.FillEllipse(highlight, x + size / 5, y + size / 8, size / 3, size / 4);
            }
        }

        private void DrawWhitePiece(Graphics g, int x, int y, int size)
        {
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(100, 100, 100)))
            {
                g.FillEllipse(shadow, x + 2, y + 2, size, size);
            }

            g.FillEllipse(Brushes.White, x, y, size, size);

            using (Pen outline = new Pen(Color.LightGray, 1))
            {
                g.DrawEllipse(outline, x, y, size, size);
            }

            using (SolidBrush highlight = new SolidBrush(Color.FromArgb(70, 255, 255, 255)))
            {
                g.FillEllipse(highlight, x + size / 5, y + size / 8, size / 3, size / 4);
            }
        }

        private void DrawHint(Graphics g, int x, int y, int size)
        {
            Color hintColor;

            if (currentPlayer == 1)
            {
                hintColor = Color.FromArgb(90, 0, 0, 0);
            }
            else
            {
                hintColor = Color.FromArgb(100, 230, 230, 230);
            }

            using (SolidBrush hintBrush = new SolidBrush(hintColor))
            {
                g.FillEllipse(
                    hintBrush,
                    x + size / 4,
                    y + size / 4,
                    size / 2,
                    size / 2
                );
            }
        }

        private void pnlBoard_MouseClick(object sender, MouseEventArgs e)
        {
            if (gameOver)
            {
                return;
            }

            if (IsAITurn())
            {
                return;
            }

            cellSize = GetCellSize();

            int col = e.X / cellSize;
            int row = e.Y / cellSize;

            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
            {
                return;
            }

            if (e.X >= BoardSize * cellSize || e.Y >= BoardSize * cellSize)
            {
                return;
            }

            if (!IsValidMove(row, col, currentPlayer))
            {
                return;
            }

            PushHistory();
            PlacePiece(row, col, currentPlayer);
            PlaySound(playSound);
            NextTurn();
        }

        private bool IsValidMove(int row, int col, int player)
        {
            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
            {
                return false;
            }

            if (board[row, col] != 0)
            {
                return false;
            }

            int opponent = 3 - player;

            for (int direction = 0; direction < 8; direction++)
            {
                int r = row + dr[direction];
                int c = col + dc[direction];

                bool hasOpponentBetween = false;

                while (IsInsideBoard(r, c) && board[r, c] == opponent)
                {
                    hasOpponentBetween = true;
                    r += dr[direction];
                    c += dc[direction];
                }

                if (hasOpponentBetween && IsInsideBoard(r, c) && board[r, c] == player)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsInsideBoard(int row, int col)
        {
            return row >= 0 && row < BoardSize && col >= 0 && col < BoardSize;
        }

        private void PlacePiece(int row, int col, int player)
        {
            board[row, col] = player;

            int opponent = 3 - player;

            for (int direction = 0; direction < 8; direction++)
            {
                int r = row + dr[direction];
                int c = col + dc[direction];

                List<Point> piecesToFlip = new List<Point>();

                while (IsInsideBoard(r, c) && board[r, c] == opponent)
                {
                    piecesToFlip.Add(new Point(c, r));
                    r += dr[direction];
                    c += dc[direction];
                }

                if (piecesToFlip.Count > 0 && IsInsideBoard(r, c) && board[r, c] == player)
                {
                    foreach (Point p in piecesToFlip)
                    {
                        board[p.Y, p.X] = player;
                    }
                }
            }
        }

        private bool HasValidMove(int player)
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (IsValidMove(row, col, player))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private List<Point> GetValidMoves(int player)
        {
            List<Point> moves = new List<Point>();

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (IsValidMove(row, col, player))
                    {
                        moves.Add(new Point(col, row));
                    }
                }
            }

            return moves;
        }

        private void NextTurn()
        {
            int nextPlayer = 3 - currentPlayer;

            if (HasValidMove(nextPlayer))
            {
                currentPlayer = nextPlayer;
            }
            else if (HasValidMove(currentPlayer))
            {
                string skippedPlayer = nextPlayer == 1 ? "黑棋" : "白棋";

                MessageBox.Show(
                    skippedPlayer + "沒有合法落點，跳過回合！",
                    "跳過回合",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                gameOver = true;
                UpdateUI();
                pnlBoard.Invalidate();
                ShowResult();
                return;
            }

            UpdateUI();
            pnlBoard.Invalidate();
            StartAIIfNeeded();
        }

        private void CountPieces(out int black, out int white)
        {
            black = 0;
            white = 0;

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (board[row, col] == 1)
                    {
                        black++;
                    }
                    else if (board[row, col] == 2)
                    {
                        white++;
                    }
                }
            }
        }

        private void UpdateUI()
        {
            int black;
            int white;
            CountPieces(out black, out white);

            lblBlackScore.Text = "黑棋: " + black;
            lblWhiteScore.Text = "白棋: " + white;

            string playerName = currentPlayer == 1 ? "黑棋" : "白棋";

            if (gameOver)
            {
                lblStatus.Text = "遊戲結束";
            }
            else
            {
                lblStatus.Text = "輪到: " + playerName;
            }
        }

        private void StopSound(SoundPlayer player)
        {
            try
            {
                if (player != null)
                {
                    player.Stop();
                }
            }
            catch
            {
                // 停止音效失敗時，不讓遊戲當掉
            }
        }

        private void ShowResult()
        {
            int black;
            int white;
            CountPieces(out black, out white);

            string result;
            SoundPlayer resultSound;

            if (black > white)
            {
                result = "黑棋獲勝！";
                resultSound = winner_bSound;
            }
            else
            {
                result = "白棋獲勝！";
                resultSound = winner_wSound;
            }

            PlaySound(resultSound);

            MessageBox.Show(
                result + "\n\n黑棋: " + black + "\n白棋: " + white,
                "遊戲結束",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            StopSound(resultSound);
        }

        private void PushHistory()
        {
            int[,] snap = new int[BoardSize, BoardSize];
            Array.Copy(board, snap, board.Length);

            history.Push(new GameState
            {
                BoardSnap = snap,
                Player = currentPlayer
            });
        }

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "確定要開始新遊戲嗎？",
                "新遊戲",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                InitGame();
            }
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            timerAI.Stop();

            if (history.Count == 0)
            {
                MessageBox.Show(
                    "目前沒有可以悔棋的紀錄！",
                    "悔棋",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            GameState lastState = history.Pop();

            board = lastState.BoardSnap;
            currentPlayer = lastState.Player;
            gameOver = false;

            UpdateUI();
            pnlBoard.Invalidate();
            StartAIIfNeeded();
        }

        private bool IsAIMode()
        {
            return mode.SelectedIndex == 1 || mode.SelectedIndex == 2;
        }

        private bool IsAITurn()
        {
            return IsAIMode() && currentPlayer == 2;
        }

        private void StartAIIfNeeded()
        {
            timerAI.Stop();

            if (!gameOver && IsAITurn())
            {
                timerAI.Start();
            }
        }

        private void timerAI_Tick(object sender, EventArgs e)
        {
            timerAI.Stop();

            if (gameOver)
            {
                return;
            }

            if (!IsAITurn())
            {
                return;
            }

            List<Point> moves = GetValidMoves(currentPlayer);

            if (moves.Count == 0)
            {
                NextTurn();
                return;
            }

            Point aiMove = ChooseAIMove(moves);

            PushHistory();
            PlacePiece(aiMove.Y, aiMove.X, currentPlayer);
            PlaySound(playSound);
            NextTurn();
        }

        private Point ChooseAIMove(List<Point> moves)
        {
            // 人 vs AI（簡易）：隨機落子
            if (mode.SelectedIndex == 1)
            {
                return moves[random.Next(moves.Count)];
            }

            // 人 vs AI（稍難）：優先選擇翻最多棋，角落加分
            Point bestMove = moves[0];
            int bestScore = int.MinValue;

            foreach (Point move in moves)
            {
                int row = move.Y;
                int col = move.X;

                int score = CountFlips(row, col, currentPlayer) * 10;

                if (IsCorner(row, col))
                {
                    score += 1000;
                }
                else if (IsEdge(row, col))
                {
                    score += 30;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
                else if (score == bestScore && random.Next(2) == 0)
                {
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private int CountFlips(int row, int col, int player)
        {
            if (!IsValidMove(row, col, player))
            {
                return 0;
            }

            int total = 0;
            int opponent = 3 - player;

            for (int direction = 0; direction < 8; direction++)
            {
                int r = row + dr[direction];
                int c = col + dc[direction];

                int count = 0;

                while (IsInsideBoard(r, c) && board[r, c] == opponent)
                {
                    count++;
                    r += dr[direction];
                    c += dc[direction];
                }

                if (count > 0 && IsInsideBoard(r, c) && board[r, c] == player)
                {
                    total += count;
                }
            }

            return total;
        }

        private bool IsCorner(int row, int col)
        {
            return
                (row == 0 && col == 0) ||
                (row == 0 && col == BoardSize - 1) ||
                (row == BoardSize - 1 && col == 0) ||
                (row == BoardSize - 1 && col == BoardSize - 1);
        }

        private bool IsEdge(int row, int col)
        {
            return row == 0 || row == BoardSize - 1 || col == 0 || col == BoardSize - 1;
        }

        private void mode_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartAIIfNeeded();
        }

        private void 遊戲說明ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaySound(ruleSound);

            MessageBox.Show(
                "【黑白棋 Othello 規則】\n\n" +
                "1. 棋盤為 8×8，黑棋先手。\n\n" +
                "2. 落子時，必須在橫、直、斜任一方向夾住對手棋子。\n\n" +
                "3. 被夾住的對手棋子會全部翻成自己的顏色。\n\n" +
                "4. 如果輪到某方時沒有合法落點，該方會跳過回合。\n\n" +
                "5. 雙方都無法落子，或棋盤已滿時，遊戲結束。\n\n" +
                "6. 棋子數較多者獲勝；數量相同則平手。\n\n" +
                "7. 盤面上的半透明小圓點代表目前可以落子的位置。\n\n" +
                "8. 若當回合沒有合法的落棋位置，會跳過該回合。",
                "遊戲規則說明",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            if (ruleSound != null)
            {
                ruleSound.Stop();
            }
        }

        private void 關於ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "黑白棋 Othello v1.0\n製作者：元智大學資工系三年級 Mandy Hsu",
                "關於",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}