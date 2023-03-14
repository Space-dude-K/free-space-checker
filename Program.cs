using System;
using System.Collections.Generic;
using System.IO;

namespace FreeSpaceChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            Checker checker = new Checker();
            //checker.PerfomCheck();

            /*
            TotalSpaceChecker tsc = new TotalSpaceChecker();

            var ips = GetIpsFromFile(@"C:\Users\mgl15.GKMOGILEV\Desktop\Проекты\Soft GU\FreeSpaceChecker\FreeSpaceChecker\Settings\TotalSpaceIp.txt");

            foreach (var ip in ips)
            {
                tsc.CheckTotalSpace1(ip.Item1, ip.Item2);
            }

            Console.WriteLine("Done.");

            Console.ReadLine();
            */
        }
        private static List<Tuple<string, string>> GetIpsFromFile(string path)
        {
            List<Tuple<string, string>> lines = new List<Tuple<string, string>>();

            using (StreamReader sr = new StreamReader(path))
            {
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();

                    if (!string.IsNullOrEmpty(line))
                    {
                        var sLine = line.Split(',');

                        lines.Add(Tuple.Create(sLine[0], sLine[1]));
                    }
                }
            }

            return lines;
        }
    }
}
