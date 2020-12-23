using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day22
{
    public static class Program
    {
        private static void Main()
        {
            //parse stuff
            var cardsP1 = new Queue<int>();
            var cardsP2 = new Queue<int>();
            var player2 = false;
            foreach (var line in File.ReadLines("input.txt"))
            {
                if (string.IsNullOrWhiteSpace(line))
                    player2 = !player2;
                if(int.TryParse(line, out var cardNum))
                    (player2 ? cardsP2 : cardsP1).Enqueue(cardNum);
            }
            
            //play game
            while (cardsP1.TryDequeue(out var p1Card) && cardsP2.TryDequeue(out var p2Card))
            {
                var roundWinner = p1Card > p2Card ? cardsP1 : cardsP2;
                roundWinner.Enqueue(Math.Max(p1Card, p2Card));
                roundWinner.Enqueue(Math.Min(p1Card, p2Card));
            }

            var winningDeck = cardsP1.Any() ? cardsP1 : cardsP2;
            var score = winningDeck.Reverse().Select((card, index) => card * (index + 1)).Sum();
            Console.WriteLine($"{(cardsP1.Any()?"player":"crab")} wins the first round with a score of {score}");
            
            //parse cards again
            cardsP1.Clear();
            cardsP2.Clear();
            player2 = false;
            foreach (var line in File.ReadLines("input.txt"))
            {
                if (string.IsNullOrWhiteSpace(line))
                    player2 = !player2;
                if(int.TryParse(line, out var cardNum))
                    (player2 ? cardsP2 : cardsP1).Enqueue(cardNum);
            }

            bool playRecursive(Queue<int> deck1, Queue<int> deck2, ref int game)
            {
                game++;
                //Console.WriteLine($"Started game {game}");
                var previousStates = new HashSet<(int[] deck1, int[] deck2)>(new DeckEqualityComparer());
                while (deck1.Any() && deck2.Any())
                {
                    //add returns false if state already exists
                    if (!previousStates.Add((deck1.ToArray(), deck2.ToArray())))
                        return true;
                    var p1Card = deck1.Dequeue();
                    var p2Card = deck2.Dequeue();
                    bool roundWinner;
                    if (deck1.Count >= p1Card && deck2.Count >= p2Card)
                    {
                        //play subround with cloned decks
                        roundWinner = playRecursive(
                            new Queue<int>(deck1.Take(p1Card)), 
                            new Queue<int>(deck2.Take(p2Card)), ref game);
                    } else  {
                        roundWinner = p1Card > p2Card;
                    }
                    var roundWinnerDeck = roundWinner ? deck1 : deck2;
                    roundWinnerDeck.Enqueue(roundWinner?p1Card:p2Card);
                    roundWinnerDeck.Enqueue(roundWinner?p2Card:p1Card);
                }
                return deck1.Any();
            }

            int gameNum = 0;
            bool winner = playRecursive(cardsP1, cardsP2, ref gameNum);
            winningDeck = winner ? cardsP1 : cardsP2;
            score = winningDeck.Reverse().Select((card, index) => card * (index + 1)).Sum();
            Console.WriteLine($"{(winner?"player":"crab")} wins the second round with a score of {score}");
        }
        class DeckEqualityComparer : IEqualityComparer<(int[] deck1, int[] deck2)>
        {
            public bool Equals((int[] deck1, int[] deck2) x, (int[] deck1, int[] deck2) y)
            {
                return x.deck1.SequenceEqual(y.deck1) && x.deck2.SequenceEqual(y.deck2);
            }

            public int GetHashCode((int[] deck1, int[] deck2) obj)
            {
                return HashCode.Combine(GetHashCode(obj.deck1), GetHashCode(obj.deck2));
            }
            
            static int GetHashCode(int[] values)
            {
                int result = 0;
                int shift = 0;
                foreach (var val in values)
                {
                    shift = (shift + 11) % 21;
                    result ^= (val+1024) << shift;
                }
                return result;
            }
        }
    }
}