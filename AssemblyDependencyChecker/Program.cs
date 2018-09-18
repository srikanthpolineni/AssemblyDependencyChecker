using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDependencyChecker
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Command usage. AssemblyDependencyChecker.exe \"C:\\Program Files(x86)\\MTXEPS\\WebEPS\" ");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }
            var path = args[0];

            if (!Directory.Exists(path))
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(path + " doesn't exist!");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            Dictionary<string, AssemblyEntiry> uniqueAssemblies = new Dictionary<string, AssemblyEntiry>();
            foreach (var file in Directory.GetFiles(path).Where(f => f.EndsWith(".dll") || f.EndsWith(".exe")))
            {
                var assembly = Assembly.ReflectionOnlyLoadFrom(file);
                AddDllCollection(uniqueAssemblies, assembly.GetName());

                foreach (var refAssembly in assembly.GetReferencedAssemblies())
                {
                    AddDllCollection(uniqueAssemblies, refAssembly, assembly);
                }
            }

            foreach (var assembly in uniqueAssemblies.Values.ToList().OrderBy(ass => ass.FullName))
            {
                bool isDllMissing = false;

                Console.BackgroundColor = ConsoleColor.Black;
                try
                {
                    Assembly.ReflectionOnlyLoad(assembly.FullName);
                }
                catch (FileNotFoundException ex)
                {
                    isDllMissing = true;
                }
                if (isDllMissing)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("Missing>>>> {0} <<<<Missing", assembly.FullName));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(assembly.FullName);
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Referencing By:");
                foreach (var item in assembly.Parents)
                {
                    Console.WriteLine("\t\t " + item.FullName);
                }
                Console.WriteLine("");
                Console.WriteLine("");
                Console.ResetColor();
            }

            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
        }


        private static void AddDllCollection(Dictionary<string, AssemblyEntiry> uniqueDlls, AssemblyName assemblyName, Assembly parentAssemly = null)
        {
            AssemblyEntiry entiry;
            if (!uniqueDlls.TryGetValue(assemblyName.FullName, out entiry))
            {
                entiry = new AssemblyEntiry(assemblyName.FullName);
                uniqueDlls[entiry.FullName] = entiry;
            }
            if (parentAssemly != null)
            {
                var parentEntiry = new AssemblyEntiry(parentAssemly.GetName().FullName);
                if (entiry.Parents.Count == 0 || entiry.Parents.Where(p => p.FullName == parentEntiry.FullName).Count() == 0)
                    entiry.Parents.Add(parentEntiry);
            }
        }
    }

    class AssemblyEntiry
    {
        public AssemblyEntiry(string fullName)
        {
            this.FullName = fullName;
            this.Parents = new List<AssemblyEntiry>();
        }
        public string FullName { get; set; }

        public List<AssemblyEntiry> Parents { get; set; }

        public override string ToString()
        {
            return FullName;
        }

    }
}
