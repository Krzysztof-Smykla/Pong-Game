using System;
using System.Runtime.InteropServices;
using System.Threading;

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
        #endregion

        #region Initialization
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
            paddleY = height/2; 
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
                    Environment.Exit(0);
            }
            /*
            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                if ((key == ConsoleKey.W || key == ConsoleKey.UpArrow) && paddleY > 1)
                    paddleY--;
                else if ((key == ConsoleKey.S || key == ConsoleKey.DownArrow) && paddleY < height - paddleHeight - 1)
                    paddleY++;
                else if (key == ConsoleKey.Escape)
                    Environment.Exit(0);
            }
            */
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
                ballDirX *= -1;
                score++;
            }
            /*
            // Bounce off paddle
            if (ballX == 3 && ballY == paddleY)
            {
                ballDirX *= -1;
                score++;
            }
            */
            // Missed ball (Game Over)

            if (ballX <= 0)
            {
                Console.Clear();
                Console.WriteLine($"Game Over! Final score: {score}");
                Environment.Exit(0);
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
            for (int i  = 0; i < paddleHeight; i++)
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

            while (true)
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

                /*
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    break;
                */
            }

            Console.Clear();
            Console.WriteLine($"Game Over! Final score: {score}");
        }

        public void Start()
        {
            ChooseDifficulty();
            Run();
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