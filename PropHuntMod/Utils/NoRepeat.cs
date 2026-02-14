using System;
using System.Collections.Generic;
using System.Linq;
using PropHuntMod.Utils;

namespace NoRepeat
{
    public class NoRepeat<T>
    {
        public List<T> used { get; private set; }
        public List<T> inputValues { get; private set; }
        private readonly Random random = new Random();
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
                Log.LogInfo("Resetting NoRepeat");
            }
            used.Add(element);

            return element;
        }

        public T GetSpecific(Func<T, bool> func)
        {
            T found = inputValues.FirstOrDefault(func);
            if (found != null) return found;

            found = used.FirstOrDefault(func);
            return found;
        }
    }
}
