using System;
using System.Collections.Generic;

namespace PropHuntMod
{
    public class NoRepeat<T>
    {
        private List<T> used;
        private List<T> inputValues;
        private Random random = new Random();
        public NoRepeat(List<T> values)
        {
            inputValues = values;
            used = new List<T>();
        }

        public T GetRandom()
        {
            if (used.Count == 0 && inputValues.Count <= 1)
            {
                if (inputValues.Count == 0) return default;
                return inputValues[0];
            }
            int i = random.Next(inputValues.Count);
            var element = inputValues[i];
            inputValues.RemoveAt(i);
            if (inputValues.Count == 0)
            {
                inputValues.AddRange(used);
                used = new List<T>();
                Console.WriteLine("Resetting NoRepeat");
            }
            used.Add(element);

            return element;
        }
    }
}
