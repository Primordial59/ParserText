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
                // var str = b.Title + "\n\t" + string.Join("\n\t", b.Body);
                // var str = b.Title + "\n\t" + string.Join("\n\t", b.PhoneNumber);
                // var str =b.PhoneNumber;
                var str = string.Join("\n", b.Body);
                Console.WriteLine(str);

                //String Separator = ";"; //разделитель данных в формате CSV
                //string[] splitResult = Regex.Split(string.Join("!", b.Body), Separator);
                //foreach (string str in splitResult)
                //{
                //    System.Diagnostics.Trace.WriteLine(str);
                //    Console.WriteLine(str);
                //}

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
            String CurrentPhone =""; //здесь храним текущий обрабатываемый номер, сначала его еще нет
            int GlobalCounter = 0; // счетчиксобытий во всей загрузке
            int Counter = 0; // счетчик событий по текущему номеру 
            Block ret = null;


                foreach (var line in File.ReadLines(path,Encoding.GetEncoding("windows-1251")).Select(l => l.Trim()))
                {
                  //  if (line.Length == 0 && ret != null)// если подготвленные данные уже есть и достигнута  пустая строка
                     if (ret != null && GlobalCounter>=50) 
                    {
                        yield return ret;
                        ret = null;
                        continue;
                    }

                //   if (line.EndsWith(":"))
                // определим новый номер или продолжаем рабоать со старым
                if (line.StartsWith("Детализация"))
                {
                    if (CurrentPhone=="")
                    {
                        ret = new Block { Title = line.TrimEnd(':'), Body = new List<string>(), PhoneNumber = CurrentPhone };
                    }
                    string Phone = line.Substring(53, 11); // выделяем из строки номер
                    if (CurrentPhone != Phone)
                    {
                        // здесь можно инициализирвоать новый объект для очередного номера телефона
                        CurrentPhone = Phone;
                        continue;
                    }

                } // здесь была строка с номером
                else // для других строк ловим данные
                {
                    if (line.StartsWith("01.05"))
                    {
                        if (ret != null)
                        ret.Body.Add(line+CurrentPhone);
                        GlobalCounter++;

                    }

                }


                   
             

                 }
        }
        }


    }

