using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using static System.Formats.Asn1.AsnWriter;
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
            //TODO: How to create absolute paths in C#
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
            LoadDatabase("score_csv.txt");
        }

        public void Start()
        {
            AskForPlayerName();
            ChooseDifficulty();
            Run();
        }
        #endregion

        #region Scoreboard
        // TODO: Reformat SaveScore method to save the socoreboard directly into csv format,
        // removing redundant FormatScoreboard method."
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
            Console.WriteLine($"\nCSV scoreboard saved at:\n{filePath2}");
        }
        #endregion

    }
    public static void LoadDatabase(string filePath2)
    {
        // 1. Validate CSV file
        if (!File.Exists(filePath2))
        {
            Console.WriteLine($"CSV file not found: {filePath2}");
            return;
        }

        // 2. Read all lines (skip header if present)
        var lines = File.ReadAllLines(filePath2);
        if (lines.Length == 0)
        {
            Console.WriteLine("CSV file is empty.");
            return;
        }

        int startIndex = lines[0].StartsWith("DateTime") ? 1 : 0;

        // 3. Open SQLite connection
        using var conn = new SqliteConnection("Data Source=PongGameDB.db");
        conn.Open();
        using var tran = conn.BeginTransaction();

        foreach (var line in lines[startIndex..])
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length < 3) continue;

            // Parse DateTime properly
            if (!DateTime.TryParseExact(
                    parts[0].Trim(),
                    "yyyy-MM-dd h:mm:ss tt",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime gameDateTime))
            {
                Console.WriteLine($"Skipping invalid date: {parts[0]}");
                continue;
            }

            string player = parts[1].Trim();
            if (!int.TryParse(parts[2].Trim(), out int score))
            {
                Console.WriteLine($"Skipping invalid score: {parts[2]}");
                continue;
            }

            // Insert into database
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            INSERT INTO Scoreboard (GameDate, PlayerName, Score)
            VALUES ($GameDate, $PlayerName, $Score);";

            // Store GameDate in ISO 8601 format
            cmd.Parameters.AddWithValue("$GameDate", gameDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("$PlayerName", player);
            cmd.Parameters.AddWithValue("$Score", score);

            cmd.ExecuteNonQuery();
        }

        tran.Commit();
        conn.Close();

        Console.WriteLine("CSV data successfully loaded into the database.");
    }
    public static void ReadDatabase(SqliteConnection c, int topScores)
    {
        //TODO: Read data from tables in the PongGameDB.db
        using var command = c.CreateCommand();
        command.CommandText = @"
            SELECT GameDate, PlayerName, Score
            FROM scoreboard
            ORDER BY Score DESC
            LIMIT $top;
            ";
        command.Parameters.AddWithValue("$top", topScores);
        using var reader = command.ExecuteReader();


        Console.WriteLine($"Top Scores: ");
        int rank = 1;
        while (reader.Read())
        {
            string gameDate = reader.GetString(0); // GameDate stored as string
            string playerName = reader.GetString(1);
            int score = reader.GetInt32(2);

            Console.WriteLine($"{rank}. {playerName} - score {score} ({gameDate})");
            rank++;
        }
    }

    static void Main()
    {
        #region Database connection
        string dbPath = Path.Combine("PongGameDB.db");
        string sqlFile = Path.Combine("scoreboard.sql");

        using var conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();

        if (!File.Exists(sqlFile))
        {
            Console.WriteLine($"SQL file not found: {sqlFile}");
            return;
        }

        string sqlScript = File.ReadAllText(sqlFile);
        
        // 3. Execute SQL script
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = sqlScript;
            cmd.ExecuteNonQuery();
        }

        // 4. Verify table creation
        using (var verifyCmd = conn.CreateCommand())
        {
            verifyCmd.CommandText =
                "SELECT name FROM sqlite_master WHERE type='table';";

            using var reader = verifyCmd.ExecuteReader();
            Console.WriteLine("Tables in database:");

            while (reader.Read())
            {
                Console.WriteLine("- " + reader.GetString(0));
            }
        }

        conn.Close(); 
        Console.WriteLine("Connection Verified.");

        #endregion

        #region GameStart
        // Start the game
        Console.CursorVisible = false;
        PongGame game = new PongGame();
        game.Start();


        // Loading score data into database and reading top 10 scorers 
        using var conn2 = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();

        // Load CSV data
        LoadDatabase("score_csv.txt");

        // Read top 10 scores
        ReadDatabase(conn, 10);

        conn.Close();
        #endregion

    }
}