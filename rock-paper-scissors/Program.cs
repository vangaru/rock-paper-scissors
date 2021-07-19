using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace rock_paper_scissors
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var moves = args[1..].ToList();

            if (moves.Count < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(args),
                    $"Number of strings must be more or equal to 3 but got {moves.Count}." +
                    "For example: dotnet run Program.cs A B C");
            }

            if (moves.Count % 2 == 0)
            {
                throw new ArgumentException($"Number of arguments must be odd but got {moves.Count}",
                    nameof(args));
            }

            if (!ArgsContainsOnlyUniqueItems(moves))
            {
                throw new ArgumentException("Args can contain only unique parameters",
                    nameof(args));
            }
            
            var secretKey = GenerateSecretKey(16);

            var computerMove = GetComputerMove(moves);

            var computerMoveHash = GetMessageHash(computerMove, secretKey);
            Console.WriteLine($"HMAC: {computerMoveHash}");

            var userInput = GetUserInput(moves);

            if (userInput <= 0) return;
            var userMove = GetUserMove(userInput, moves);
            Raffle(userMove, computerMove, moves);
            
            Console.WriteLine($"Your move: {userMove}");
            Console.WriteLine($"Computer move: {computerMove}");
            Console.WriteLine($"Key: {BitConverter.ToString(secretKey)}");
        }

        private static bool ArgsContainsOnlyUniqueItems(IReadOnlyCollection<string> moves)
        {
            var uniqueItems = moves.Distinct();
            return uniqueItems.Count() == moves.Count;
        }

        private static byte[] GenerateSecretKey(int bytes)
        {
            var secreteKey = new byte[bytes];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(secreteKey);
            return secreteKey;
        }

        private static string GetComputerMove(IReadOnlyList<string> moves)
        {
            var computerMoveIndex = GenerateRangedRandomNumber(0, moves.Count);
            var computerMove = moves[computerMoveIndex];
            return computerMove;
        }

        private static int GenerateRangedRandomNumber(int start, int stop)
        {
            var random = new Random();
            var randomNumber = random.Next(start, stop);
            return randomNumber;
        }

        private static string GetMessageHash(string message, byte[] secretKey)
        {
            var encoding = new ASCIIEncoding();
            var messageBytes = encoding.GetBytes(message);

            using var hmacSha256 = new HMACSHA256(secretKey);
            var messageHash = hmacSha256.ComputeHash(messageBytes);
            return BitConverter.ToString(messageHash);
        }

        private static int GetUserInput(IReadOnlyList<string> moves)
        {
            int chosenNumber;
            while (true)
            {
                PrintUserMenu(moves);
                Console.Write("Your Move: ");
                var userInput = Console.ReadLine();
                var success = int.TryParse(userInput, out chosenNumber);
                if (!success)
                {
                    Console.WriteLine($"Input number must be integer");
                    continue;
                }
                if (chosenNumber < 0 || chosenNumber > moves.Count)
                {
                    Console.WriteLine($"Chosen number cannot be less than 0 or more than {moves.Count}");
                }
                else break;
            }
            return chosenNumber;
        }

        private static void PrintUserMenu(IReadOnlyList<string> moves)
        {
            Console.WriteLine("Available moves");
            for (var i = 0; i < moves.Count; i++)
            {
                Console.WriteLine($"{i + 1} {moves[i]}");
            }
            Console.WriteLine("0 exit");
        }
        
        private static string GetUserMove(int chosenNumber, IReadOnlyList<string> moves)
        {
            if (chosenNumber < 1 || chosenNumber > moves.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(chosenNumber),
                    $"Chosen number cannot be less than 1 or bigger than {moves.Count} but got {chosenNumber}");
            }

            // If chosenNumber is 0 - exit the program
            return moves[chosenNumber - 1];
        }
        
        private static void Raffle(string userMove, string computerMove, IList<string> moves)
        {
            var mid = moves.Count / 2;
            var indexOfUserMove = moves.IndexOf(userMove);
            var indexOfComputerMove = moves.IndexOf(computerMove);
            if (userMove == computerMove)
            {
                Console.WriteLine("Draw");
            }
            else if (indexOfUserMove <= mid)
            {
                if (indexOfComputerMove > indexOfUserMove &&
                    indexOfComputerMove <= indexOfUserMove + mid)
                {
                    Console.WriteLine("You win");
                }
                else
                {
                    Console.WriteLine("You lost");
                }
            }
            else
            {
                if (indexOfComputerMove > indexOfUserMove || 
                    indexOfComputerMove < indexOfUserMove - mid)
                {
                    Console.WriteLine("You win");
                }
                else
                {
                    Console.WriteLine("You lost");
                }
            }
        }
    }
}