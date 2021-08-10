using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace viewObjectBuilder
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var result = CommandLineApplication.Execute<Configuration.Configuration>(args);
            if (Configuration.Configuration.Verbose)
            {
                DisplayResults(Configuration.Configuration.ProcessResults);
            }
            return result;
        }

        public static void BuildDirectories(string[] folders)
        {
            var buildPath = new Action<string>(makeDir =>
            {
                if (Directory.Exists(makeDir))
                {
                    Directory.GetFiles(makeDir).ToList().ForEach(File.Delete);
                    Directory.GetDirectories(makeDir).ToList().ForEach(d => Directory.Delete(d, true));
                }
                else
                {
                    Directory.CreateDirectory(makeDir);
                }
            });

            foreach (var folder in folders)
            {
                buildPath(folder);
            }
        }

        public static void DisplayResults(IEnumerable<string> processMessages)
            => DisplayResults(Console.WriteLine, processMessages);

        public static void DisplayResults(Action<string> writer, IEnumerable<string> processMessages)
        {
            var messages = processMessages?.ToArray() ?? new string[] { };
            writer(!messages.Any() ?
                $"Command successful" : $"Results: {string.Join('\r', messages)}");
        }
    }
}
