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
                //                var str = b.Title + "\n\t" + string.Join("\n\t", b.Body);
                //               var str = b.Title + "\n\t" + string.Join("\n\t", b.PhoneNumber);
                var str =b.PhoneNumber;
                System.Diagnostics.Trace.WriteLine(str);
                Console.WriteLine(str);
               

            }

        }
    }

class Block
        {
            public string Title;
            public IList<string> Body;
            public string PhoneNumber;



        public static IEnumerable<Block> Load(string path)
            {
            String CurrentPhone = "";

                Block ret = null;
                foreach (var line in File.ReadLines(path,Encoding.GetEncoding("windows-1251")).Select(l => l.Trim()))
                {
                //    if (line.Length == 0 && ret != null)
                if (ret != null)
                {
                    yield return ret;
                        ret = null;
                        continue;
                    }

                 //   if (line.EndsWith(":"))
                    if (line.StartsWith("Детализация"))
                    {
                    string Phone=line.Substring(53,11);
                    if (Phone != CurrentPhone)
                        {
                        ret = new Block { Title = line.TrimEnd(':'), Body = new List<string>(), PhoneNumber = Phone };
                        CurrentPhone = Phone;
                        continue;
                        }
                    }

                    if (ret != null)
                        ret.Body.Add(line);
                    

            }
        }
        }


    }

