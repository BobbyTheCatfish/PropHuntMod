using PropHuntMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PropHuntMod.Tests
{
    internal abstract class BaseTest
    {
        public abstract KeyCode KeyCode { get; }
        public abstract string Name { get; }

        public abstract bool Enabled { get; }
        public abstract bool Execute();
    }

    internal static class TestManager
    {
        static List<BaseTest> tests = new List<BaseTest>();
        public static void GetAllTests()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var testTypes = assembly.GetTypes()
                .Where(t => t.IsClass && t.BaseType == typeof(BaseTest))
                .ToList();

            Log.LogInfo($"Grabbed {testTypes.Count} tests");

            foreach (var testType in testTypes)
            {
                tests.Add(Activator.CreateInstance(testType) as BaseTest);
            }
        }
        public static void Update()
        {
            foreach (var test in tests)
            {
                if (!test.Enabled) return;
                if (Input.GetKeyDown(test.KeyCode))
                {
                    var result = test.Execute();
                    if (!result)
                    {
                        Log.LogError($"Test {test.Name} failed.");
                    }
                }
            }
        }
    }
}
