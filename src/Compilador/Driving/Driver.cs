using Compilador.Common;
using Compilador.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Compilador.Driving
{
    public static class Driver
    {
        public static void Run(string[] args)
        {
            try
            {
                var driver_args = GetArgs(args);
                var source = File.ReadAllText(driver_args.Input);
                var grammar = Parser.Parse(source);
                grammar.WriteToFile(driver_args.Output);
                Console.WriteLine("Compilação finalizada.");
            }
            catch (GrammarException e)
            {
                DisplayError(e.Message);
            }
            catch
            {
                DisplayError("erro desconhecido");
            }
#if DEBUG
            Console.ReadKey(true);
#endif
        }

        private static void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static DriverArguments GetArgs(string[] args)
        {
            return new DriverArguments()
            {
                Input = args[0],
                Output = args.Length >= 2 ? args[1] : "BNFOutput"
            };
        }

        private class DriverArguments
        {
            public string Input { get; set; }
            public string Output { get; set; }
        }
    }
}
