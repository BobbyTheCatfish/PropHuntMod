using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            int i = random.Next(inputValues.Count);
            Console.WriteLine(i);
            Console.WriteLine(inputValues.Count);
            var element = inputValues[i];
            inputValues.RemoveAt(i);
            if (inputValues.Count == 0)
            {
                inputValues = inputValues.Concat(used).ToList();
                used = new List<T>();
                Console.WriteLine("Resetting NoRepeat");
            }
            used.Add(element);

            return element;
        }
    }
}
