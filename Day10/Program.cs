using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day10
{
    public static class Program
    {
        static void Main(string[] args) {
            List<int> adapters = new []{0}.Union(File.ReadAllLines("input.txt").Select(int.Parse)
                .OrderBy(adapter => adapter)).ToList();
            adapters.Add(adapters.Last()+3);

            int diff3 = 0;
            int diff1 = 0;
            for (int i = 1; i < adapters.Count; i++) {
                int diff = adapters[i] - adapters[i-1];
                switch (diff) {
                    case 1:
                        diff1++;
                        break;
                    case 3:
                        diff3++;
                        break;
                }
            }

            diff3++; // because the last to the phone is always +3
            
            Console.WriteLine($"{diff1} 1-jolt differences times {diff3} 3-jolt differences result in {diff1 * diff3}");

            //this one is cheated because a friend helped me ✨
            //instantiate array of zeroes
            var paths = new long[adapters.Last() + 1];
            //start with 1 path
            paths[^1] = 1;
            //go from last to first (skip last because we know theres no variation there)
            foreach (int jolt in adapters.SkipLast(1).Reverse()) {
                //each adapter has as many combinations as the next 3 combined
                paths[jolt] = paths[jolt + 1] + paths[jolt + 2] + paths[jolt + 3];
            }
            Console.WriteLine($"There are {paths.First()} ways to combine the adapters");
        }
    }
}
