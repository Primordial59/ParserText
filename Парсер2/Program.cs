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
            int GlobalCounter = 0; // счетчик событий во всей загрузке
           // int Counter = 0; // счетчик событий по текущему номеру 
            Block ret = null;
            String Separator = ";"; //разделитель данных в формате CSV
            bool ItIsRouming = false;
            String Place = "";
            String Mess = "";
            String Cost ="";
            int input_call = 0;
            String Day_event = "";

            // !!!! Поля ниже следует корректировать перед каждой загрузкой!!!!
            int Year_event = 2017;
            int Month_event = 5;
            String Account_Number = "73711191";
            // !!!! Поля выше следует корректировать перед каждой загрузкой!!!!

            System.Data.SqlClient.SqlConnection sqlConnection1 =
                     
            new System.Data.SqlClient.SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MobileBase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            //  new System.Data.SqlClient.SqlConnection("Data Source=b-sql-test;Initial Catalog=MobileBase;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            


            foreach (var line in File.ReadLines(path,Encoding.GetEncoding("windows-1251")).Select(l => l.Trim()))
                {
                //  if (line.Length == 0 && ret != null)// если подготвленные данные уже есть и достигнута  пустая строка
                if (ret != null && line.StartsWith("Всего по всем абонентам")) 
                //if (ret != null && GlobalCounter>=20)
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
                    if (line.Contains("за пределами домашней"))
                    {
                        ItIsRouming = true;
                    }
                    else
                    {
                        ItIsRouming = false;
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
                            int Time_min = 0;
                            String Final_Time = "";

                            String line2 = line + CurrentPhone + ";" + Year_event.ToString() + ";" + Month_event.ToString() + ";" + Day_event + ";"+Account_Number;
                            string[] splitResult = Regex.Split(line2, Separator);

                            //   Console.WriteLine("Дата: "+splitResult[0]); //совпадает для роуминга и домашней сети
                            //   Console.WriteLine("Время: "+splitResult[3]); //совпадает для роуминга и домашней сети
                            //   Console.WriteLine("Вид услуги: " + splitResult[5]); //совпадает для роуминга и домашней сети
                            //   Console.WriteLine("Направление вызова: " + splitResult[6]); // !!!!здесь "Дата-время отображения на балансе"  для роуминга
                            //   Console.WriteLine("Номер оппонента: " + splitResult[9]); //совпадает для роуминга и домашней сети
                            //   Console.WriteLine("Место вызова: " + splitResult[10]); !!!!здесь "Пусто"  для роуминга
                            if (ItIsRouming)
                            {
                                Place = splitResult[19];
                            }

                            else
                            {
                                Place = splitResult[10];
                            }
                           
                            // Далее минуты перведем из формата ЧЧ:ММ в целое число
                            if (ItIsRouming)
                            {
                                if (splitResult[11].Length == 5)
                                {
                                    if (splitResult[11].Substring(2, 1) == ":")
                                    {
                                        Time_min = Convert.ToInt32(splitResult[11].Substring(0, 2)) * 60 + Convert.ToInt32(splitResult[11].Substring(3, 2));
                                    }
                                }

                                if (Time_min != 0)
                                {
                                    Final_Time = Time_min.ToString();
                                }
                                else
                                {
                                    Final_Time = splitResult[11];
                                }
                            }
                            else
                            {
                                if (splitResult[14].Length == 5)
                                {
                                    if (splitResult[14].Substring(2, 1) == ":")
                                    {
                                        Time_min = Convert.ToInt32(splitResult[14].Substring(0, 2)) * 60 + Convert.ToInt32(splitResult[14].Substring(3, 2));
                                    }
                                }

                                if (Time_min != 0)
                                {
                                    Final_Time = Time_min.ToString();
                                }
                                else
                                {
                                    Final_Time = splitResult[14];
                                }
                            }
                            // Console.WriteLine("Прод/Объем: " + Final_Time);

                            // Console.WriteLine("Единица: " + splitResult[15]);
                            if (ItIsRouming)
                            {
                                Mess = splitResult[13];
                            }
                            else
                            {
                                Mess = splitResult[15];
                            }

                            if (ItIsRouming)
                            {
                                Cost= splitResult[16];
                            }
                            else
                            {
                                Cost = splitResult[18];
                            }
                            // Console.WriteLine("Стоимость: " + splitResult[18]);


                            // Console.WriteLine("Номер: " + splitResult[25]);
                            // Console.WriteLine("Год: " + splitResult[26]);
                            // Console.WriteLine("Месяц: " + splitResult[27]);
                            // Console.WriteLine("Дата: " + splitResult[28]);
                            // Console.WriteLine("Лицевой счет: " + splitResult[29]);
                            if(splitResult[5].Substring(0,4)=="Вход")
                            {
                                input_call = 1;
                            }
                            else
                            {
                                input_call = 0;
                            }





                            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
                            cmd.CommandType = System.Data.CommandType.Text;
                            //      cmd.CommandText = "INSERT MobileTable (phone_number) VALUES ('79026459606')";
                            cmd.CommandText = "INSERT MobileTable (phone_number,date_event,time_event,service,target_area,callnumber,call_area,year_event,month_event,day_event,clientaccount,duration,cost,mess,input_call)"+
                                "VALUES (@phone_number,@date_event,@time_event,@service,@target_area,@callnumber,@call_area,@year_event,@month_event,@day_event,@clientaccount,@duration,@cost,@mess,@input_call)";
                            {
                                // Добавить параметры
                                cmd.Parameters.AddWithValue("@phone_number", splitResult[25].Trim());
                                cmd.Parameters.AddWithValue("@date_event", d);
                                cmd.Parameters.AddWithValue("@time_event", splitResult[3].Trim());
                                cmd.Parameters.AddWithValue("@service", splitResult[5].Trim());
                                cmd.Parameters.AddWithValue("@target_area", splitResult[6].Trim());
                                cmd.Parameters.AddWithValue("@callnumber", splitResult[9].Trim());
                                cmd.Parameters.AddWithValue("@call_area", Place.Trim());
                                cmd.Parameters.AddWithValue("@year_event", Convert.ToDecimal(splitResult[26].Trim()));
                                cmd.Parameters.AddWithValue("@month_event", Convert.ToDecimal(splitResult[27].Trim()));
                                cmd.Parameters.AddWithValue("@day_event", Convert.ToDecimal(splitResult[28].Trim()));
                                cmd.Parameters.AddWithValue("@clientaccount", splitResult[29].Trim());
                                cmd.Parameters.AddWithValue("@duration", Convert.ToDecimal(Final_Time.Trim()));
                                cmd.Parameters.AddWithValue("@cost", Convert.ToDecimal(Cost.Trim()));
                                cmd.Parameters.AddWithValue("@mess", Mess.Trim());
                                cmd.Parameters.AddWithValue("@input_call", input_call);


                            }


                                cmd.Connection = sqlConnection1;
                                sqlConnection1.Open();
                                cmd.ExecuteNonQuery();
                                Console.WriteLine("Запись строки: " + GlobalCounter.ToString());
                           if (GlobalCounter==1259)
                            {
                                //Стоять здесь!
                            }


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
                       // Console.WriteLine("исключительная ситуация");


                    }
                    sqlConnection1.Close();
                }


                   
             

                 }
        }
        }


    }

