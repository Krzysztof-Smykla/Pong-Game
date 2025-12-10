using Microsoft.Data.Sqlite;
using System;
using System.Globalization;
namespace Project
{
    public class DatabaseManager
    {
        private static string _dbPath;

        public static void InitializeDB(string dbPath)
        {
                _dbPath = dbPath;

                using var conn = new SqliteConnection($"Data Source={_dbPath}");
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Scoreboard (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                GameDate TIMESTAMP NOT NULL,
                PlayerName TEXT NOT NULL,
                Score INTEGER NOT NULL
            );
            ";
            cmd.ExecuteNonQuery();

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

        }

        public static void LoadCsvIntoDatabase(string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"CSV file not found: {csvPath}");
                return;
            }

            var lines = File.ReadAllLines(csvPath);
            if (lines.Length == 0)
            {
                Console.WriteLine("CSV file is empty.");
                return;
            }

            int startIndex = lines[0].StartsWith("DateTime") ? 1 : 0;

            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            using var tran = conn.BeginTransaction();

            foreach (string line in lines[startIndex..])
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 3)
                    continue;

                // Parse date with multiple supported formats
                string[] formats =
                {
                "yyyy-MM-dd h:mm:ss tt",
                "yyyy-MM-dd hh:mm:ss tt",
                "yyyy-MM-dd H:mm:ss",
                "yyyy-MM-dd HH:mm:ss"
            };

                if (!DateTime.TryParseExact(parts[0].Trim(), formats,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
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

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                INSERT INTO Scoreboard (GameDate, PlayerName, Score)
                VALUES ($date, $player, $score);
            ";

                cmd.Parameters.AddWithValue("$date", dt.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("$player", player);
                cmd.Parameters.AddWithValue("$score", score);

                cmd.ExecuteNonQuery();
            }

            tran.Commit();
            Console.WriteLine("CSV data successfully imported.");
        }
        public static void InsertScore(string playerName, int score)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            INSERT INTO Scoreboard (GameDate, PlayerName, Score)
            VALUES ($date, $player, $score);
            ";

            cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("$player", playerName);
            cmd.Parameters.AddWithValue("$score", score);

            cmd.ExecuteNonQuery();
        }

        public static void ClearDB(string _dbPath)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                DELETE FROM Scoreboard;
                DELETE FROM sqlite_sequence WHERE name = 'Scoreboard';
            ";
            cmd.ExecuteNonQuery();   // remember to execute the command on the database

            Console.WriteLine("Scoreboard table truncated.");
        }
        public static void PrintTopScores(int limit)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT GameDate, PlayerName, Score
            FROM Scoreboard
            ORDER BY Score DESC, GameDate ASC
            LIMIT $limit;
            ";
            cmd.Parameters.AddWithValue("$limit", limit);

            using var reader = cmd.ExecuteReader();

            Console.WriteLine("\n=== Top Scores ===");
            int rank = 1;
            while (reader.Read())
            {
                string date = reader.GetString(0);
                string player = reader.GetString(1);
                int score = reader.GetInt32(2);

                Console.WriteLine($"{rank}. {player} – {score} ({date})");
                rank++;
            }
        }

    }
}


