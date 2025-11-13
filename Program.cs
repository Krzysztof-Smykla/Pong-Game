using System;

class Program
{
    class PongGame
    {
        #region Fields
        private int width = 40;
        private int height = 20;
        private int paddleY;
        private int prevPaddleY;
        private int paddleHeight = 3;
        private int ballX, ballY;
        private int prevBallX, prevBallY;
        private int ballDirX = 1, ballDirY = 1;  
        private int score = 0;
        private int speed = 50; // ms delay
        private bool isRunning = true; // flag variable
        private readonly string folder;
        private readonly string filePath;
        private string playerName = "Unknown";
        #endregion

        // Constructor method
        public PongGame()
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folder = Path.Combine(documents, "Studia\\Informatyka Społeczna AGH\\Drugi Stopień\\Semestr 1\\Programowanie w C#\\Projekt\\Pong Game Console");
            filePath = Path.Combine(folder, "score.txt");

            Directory.CreateDirectory(folder); // Ensure it exists
        }

        #region Initialization
        private void AskForPlayerName()
        {
            Console.Write("Enter your player name: ");
            playerName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(playerName))
                playerName = "Anonymous"; // fallback
        }

        private void ChooseDifficulty()
        {
            Console.Clear();
            Console.WriteLine("Choose difficulty:");
            Console.WriteLine("1. Easy");
            Console.WriteLine("2. Medium");
            Console.WriteLine("3. Hard");

            // Adjust paddle size and speed to difficulty level
            ConsoleKey key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.D1) { speed = 150; paddleHeight = 5; }
            else if (key == ConsoleKey.D2) { speed = 100; paddleHeight = 3; }
            else if (key == ConsoleKey.D3) { speed = 60; paddleHeight = 2; }
            else speed = 100;
        }

        private void Initialize()
        {
            Console.Clear();
            Console.CursorVisible = false;
            // find the central position
            paddleY = height / 2;
            ballX = width / 2;
            ballY = height / 2;

            // Draw walls once
            for (int y = 0; y < height; y++)
            {
                Console.SetCursorPosition(0, y);
                Console.Write('|');
                Console.SetCursorPosition(width - 1, y);
                Console.Write('|');
            }
            // set random start direction
            Random rand = new Random();
            ballDirX = rand.Next(0, 2) == 0 ? 1 : -1;
            ballDirY = rand.Next(0, 2) == 0 ? 1 : -1;

        }
        #endregion

        #region Game Logic
        private void Input()
        {
            while (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;

                if ((key == ConsoleKey.W || key == ConsoleKey.UpArrow) && paddleY > 1)
                    paddleY--;
                else if ((key == ConsoleKey.S || key == ConsoleKey.DownArrow) && paddleY < height - paddleHeight - 1)
                    paddleY++;
                else if (key == ConsoleKey.Escape)
                    isRunning = false; // Gracefully stop the game
            }
        }

        private void Logic()
        {
            // Move ball
            ballX += ballDirX;
            ballY += ballDirY;

            // Bounce off top/bottom walls
            if (ballY <= 0 || ballY >= height - 1)
                ballDirY *= -1;


            // Paddle collision
            if (ballX == 3 && ballY >= paddleY && ballY < paddleY + paddleHeight)
            {
                Console.Beep();
                ballDirX *= -1;
                score++;

                int paddleCenter = paddleY + paddleHeight / 2;

                if (ballY < paddleCenter)
                    ballDirY = -1;  // bounce up
                else if (ballY > paddleCenter)
                    ballDirY = 1;  // bounce down
                else
                    ballDirY = (new Random().Next(0, 2) == 0) ? -1 : 1; // Randomize slightly
            }

            if (ballX <= 0)
            {
                isRunning = false;
            }

            // Right wall bounce
            if (ballX >= width - 2)
                ballDirX *= -1;
        }

        private void Draw()
        {
            // ── Erase previous ball ─
            Console.SetCursorPosition(prevBallX, prevBallY);
            Console.Write(' ');
            
            // ── Erase previous paddle ─
            for (int i = 0; i < paddleHeight; i++)
            {
                Console.SetCursorPosition(2, prevPaddleY + i);
                Console.Write(' ');
            }

            // ── Draw ball ─
            Console.SetCursorPosition(ballX, ballY);
            Console.Write('O');

            if (prevPaddleY != paddleY)
            {
                for (int i = 0; i < paddleHeight; i++)
                {
                    Console.SetCursorPosition(2, prevPaddleY + i);
                    Console.Write(" ");
                }
            }

            // ── Draw paddle ─
            for (int i = 0; i < paddleHeight; i++)
            {
                Console.SetCursorPosition(2, paddleY + i);
                Console.Write("█");
            }

            // ── Draw score ─
            Console.SetCursorPosition(0, height + 1);
            Console.Write($"Score: {score}   ");
        }
        #endregion

        #region Main Game Loop
        private void Run()
        {
            Initialize();

            while (isRunning)
            {
                // 1️ Store old positions before updating
                prevBallX = ballX;
                prevBallY = ballY;
                prevPaddleY = paddleY;

                // 2️ Input handled continuously
                Input();


                // 3️ Movement and logic 
                Logic();
                Draw();

                // 4️ Control speed
                Thread.Sleep(speed);
            }

            Console.Clear();
            Console.WriteLine($"Game Over! Final score: {score}");

            SaveScore(score);
            FormatScoreboard();
        }

        public void Start()
        {
            AskForPlayerName();
            ChooseDifficulty();
            Run();
        }
        #endregion

        #region Scoreboard
        public void SaveScore(int score)
        {
            string entry = $"{DateTime.Now:G} | Player: {playerName} | Score: {score}";

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, entry);
            }
            else
            {
                File.AppendAllText(filePath, Environment.NewLine + entry);
            }

            Console.WriteLine($"SCOREBOARD:\n{File.ReadAllText(filePath)}");
            Console.WriteLine($"Score saved to file at:\n{filePath}");
        }
        public void FormatScoreboard()
        {
            string filePath2 = Path.Combine(folder, "score_csv.txt");

            if (!File.Exists(filePath))
            {
                Console.WriteLine("No score file found to format.");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            List<string> csvLines = new List<string>();

            // Add a CSV header row
            csvLines.Add("DateTime,Player,Score");

            foreach (string line in lines)
            {
                // Example line: "10/22/2025 21:05:43 | Player: Alex | Score: 7"
                string[] parts = line.Split('|', StringSplitOptions.TrimEntries);

                if (parts.Length == 3)
                {
                    string date = parts[0];
                    string player = parts[1].Replace("Player:", "").Trim();
                    string score = parts[2].Replace("Score:", "").Trim();

                    csvLines.Add($"{date},{player},{score}");
                }
            }

            File.WriteAllLines(filePath2, csvLines);
            Console.WriteLine($"CSV scoreboard saved at:\n{filePath2}");
        }
        #endregion
    }

    static void Main()
    {
        Console.CursorVisible = false;
        PongGame game = new PongGame();
        game.Start();
    }
}