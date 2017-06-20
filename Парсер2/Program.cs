using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;



namespace Парсер2
{
    class Program
    {
        static void Main(string[] args)
        {

            foreach (var b in Block.Load("test.txt"))
            {
                var str = b.Title + "\n\t" + string.Join("\n\t", b.Body);
                System.Diagnostics.Trace.WriteLine(str);
                Console.WriteLine(str);
                Console.WriteLine("тест");

            }

        }
    }

class Block
        {
            public string Title;
            public IList<string> Body;

            public static IEnumerable<Block> Load(string path)
            {
                Block ret = null;
                foreach (var line in File.ReadLines(path).Select(l => l.Trim()))
                {
                    if (line.Length == 0 && ret != null)
                    {
                        yield return ret;
                        ret = null;
                        continue;
                    }

                    if (line.EndsWith(":"))
                    {
                        ret = new Block { Title = line.TrimEnd(':'), Body = new List<string>() };
                        continue;
                    }

                    if (ret != null)
                        ret.Body.Add(line);
                }
            }
        }


    }

