using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Day23
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            string input = "327465189";
            var currentCup = new Item() {Value = int.Parse(input[0].ToString())};
            var parsePointer = currentCup;
            foreach (var num in input.Skip(1).Select(letter => int.Parse(letter.ToString()))) {
                var newItem = new Item {Value = num, /*Prev = parsePointer*/};
                parsePointer.Next = newItem;
                parsePointer = newItem;
            }
            parsePointer.Next = currentCup;
            //currentCup.Prev = parsePointer;
            
            CrabGame(currentCup, 100);

            while (currentCup.Value != 1)
                currentCup = currentCup.Next;
            currentCup = currentCup.Next;
            
            string res = "";
            while (currentCup.Value != 1)
            {
                res += currentCup.Value;
                currentCup = currentCup.Next;
            }
            Console.WriteLine(res);

            currentCup = new Item() {Value = int.Parse(input[0].ToString())};
            parsePointer = currentCup;
            foreach (var num in input.Skip(1).Select(letter => int.Parse(letter.ToString()))) {
                var newItem = new Item {Value = num, /*Prev = parsePointer*/};
                parsePointer.Next = newItem;
                parsePointer = newItem;
            }

            foreach (var num in Enumerable.Range(1, 1_000_000).Skip(input.Length)) {
                var newItem = new Item {Value = num, /*Prev = parsePointer*/};
                parsePointer.Next = newItem;
                parsePointer = newItem;
            }
            parsePointer.Next = currentCup;
            //currentCup.Prev = parsePointer;
            var sw = Stopwatch.StartNew();
            CrabGame(currentCup, 10_000_000);
            Console.WriteLine($"Big shuffle took {sw.ElapsedMilliseconds}ms");
            while (currentCup.Value != 1)
                currentCup = currentCup.Next;
            Console.WriteLine($"The 2 cups clockwise of `1` are {currentCup.Next.Value} and {currentCup.Next.Next.Value}," +
                              $"their product is {(long)currentCup.Next.Value * (long)currentCup.Next.Next.Value}");
        }

        private static void CrabGame(Item current, int moves)
        {
            var maxCup = current.Max();
            var lookUp = new Item[maxCup+1];
            foreach (var i in current as IEnumerable<Item>) {
                lookUp[i.Value] = i;
            }
            for (int round = 0; round < moves; round++)
            {
                //Console.WriteLine(string.Join(", ", current as IEnumerable<Item>));
                //take cups
                var heldCups = current.RemoveNext(3);

                int destinationNumber = current.Value;
                Item destination;
                do {
                    destinationNumber = (destinationNumber - 2 + maxCup) % maxCup + 1;
                    destination = heldCups.Any(held => held.Value == destinationNumber) ? null : lookUp[destinationNumber];
                } while (destination == null);
                
                destination.Add(heldCups);
                current = current.Next;
            }
        }

        class Item : IEnumerable<int>, IEnumerable<Item>
        {
            public int Value;
            public Item Next;
            //public Item Prev;

            public override string ToString()
            {
                return $"{{{Value}}}";
            }

            public Item[] RemoveNext(int amount)
            {
                var removed = new Item[amount];
                for (int i = 0; i < amount; i++)
                {
                    removed[i] = Next;
                    Next = Next.Next;
                    //Next.Prev = this;
                }
                return removed;
            }

            public void Add(params Item[] range)
            {
                foreach (var item in range.Reverse())
                {
                    //item.Prev = this;
                    item.Next = Next;
                    
                    //Next.Prev = item;
                    Next = item;
                }
            }

            IEnumerator<Item> IEnumerable<Item>.GetEnumerator()
            {
                var start = this;
                var reference = this;
                do
                {
                    yield return reference;
                    reference = reference.Next;
                } while (reference != start);
            }

            public IEnumerator<int> GetEnumerator()
            {
                var start = this;
                var reference = this;
                do
                {
                    yield return reference.Value;
                    reference = reference.Next;
                } while (reference != start);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private static IEnumerator<int> GetEnumerator(this Range range)
        {
            if (range.Start.IsFromEnd || range.End.IsFromEnd)
                throw new Exception();
            for (int i = range.Start.Value; i < range.End.Value; i++)
            {
                yield return i;
            }
        }
    }
}