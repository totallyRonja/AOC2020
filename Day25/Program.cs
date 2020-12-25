using System;
using System.IO;

namespace Day25
{
    static class Program
    {
        static void Main() {
            var lines = File.ReadAllLines("input.txt");
            var doorPub = int.Parse(lines[0]);
            var cardPub = int.Parse(lines[1]);
            var doorLoop = FindLoopSize(7, doorPub);//optional
            var cardLoop = FindLoopSize(7, cardPub);
            var encryptCard = Transform(doorPub, cardLoop);
            var encryptDoor = Transform(cardPub, doorLoop); //optional (equal encryptCard)
            Console.WriteLine($"The door loops {doorLoop} times, the card loops {cardLoop} times and the encryption key" +
                              $"is {encryptCard}");
        }

        static long Transform(int subject, int loop) {
            long key = 1;
            for (int i = 0; i < loop; i++) {
                key *= subject;
                key %= 20201227;
            }
            return key;
        }

        static int FindLoopSize(int subject, int targetKey) {
            int loopSize = 0;
            long key = 1;
            while (key != targetKey) {
                loopSize++;
                key *= subject;
                key %= 20201227;
            }
            return loopSize;
        }
    }
}