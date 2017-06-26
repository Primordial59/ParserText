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
                //var str = string.Join("\n", b.Body);
                //Console.WriteLine(str);

               // String Separator = ";"; //разделитель данных в формате CSV
               // string[] splitResult = Regex.Split(string.Join("",b.Body), Separator);
               // foreach (string str in splitResult)
               // {
               //     System.Diagnostics.Trace.WriteLine(str);
               //     Console.WriteLine(str);
               // }

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
           // int Counter = 0; // счетчик событий по текущему номеру 
            Block ret = null;
            String Separator = ";"; //разделитель данных в формате CSV
            int Year_event = 2017;
            int Month_event = 5;
            String Day_event = "";
            String Account_Number = "73711191";



            foreach (var line in File.ReadLines(path,Encoding.GetEncoding("windows-1251")).Select(l => l.Trim()))
                {
                //  if (line.Length == 0 && ret != null)// если подготвленные данные уже есть и достигнута  пустая строка
                //     if (ret != null && line.StartsWith("Всего по всем абонентам")) 
                if (ret != null && GlobalCounter>=30)
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
                    string str_d= line.Substring(0, 6)+"20"+ line.Substring(6, 2);
                    //  string str_d = "01.05.2017";

                    DateTime? d;
                    try
                    {
                        d = DateTime.ParseExact(str_d, "d", null);
                        if (ret != null)
                        {
                           // ret.Body.Add(line + CurrentPhone);
                            GlobalCounter++;
                            Day_event = line.Substring(0, 2);
                            String line2 = line + CurrentPhone + ";" + Year_event.ToString() + ";" + Month_event.ToString() + ";" + Day_event + ";"+Account_Number;
                            string[] splitResult = Regex.Split(line2, Separator);

                            Console.WriteLine("Дата: "+splitResult[0]);
                            Console.WriteLine("Время: "+splitResult[3]);
                            Console.WriteLine("Вид услуги: " + splitResult[5]);
                            Console.WriteLine("Направление вызова: " + splitResult[6]);
                            Console.WriteLine("Номер оппонента: " + splitResult[9]);
                            Console.WriteLine("Место вызова: " + splitResult[10]);
                            Console.WriteLine("Прод/Объем: " + splitResult[14]);
                            Console.WriteLine("Единица: " + splitResult[15]);
                            Console.WriteLine("Стоимость: " + splitResult[18]);
                            Console.WriteLine("Номер: " + splitResult[25]);
                            Console.WriteLine("Год: " + splitResult[26]);
                            Console.WriteLine("Месяц: " + splitResult[27]);
                            Console.WriteLine("Дата: " + splitResult[28]);
                            Console.WriteLine("Лицевой счет: " + splitResult[29]);

                            

                            // foreach (string str in splitResult)
                            // {
                            //     System.Diagnostics.Trace.WriteLine(str);
                            //     Console.WriteLine(str);
                            // }
                        }
                    }
                    catch (SystemException)
                    {
                        d = null;
                                    
                   
                    }

                }


                   
             

                 }
        }
        }


    }

