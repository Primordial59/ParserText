using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;




namespace Парсер2
{
    class Program
    {
        static void Main(string[] args)
        {
            String Oper = "0";
            String download_file = "";

            if (args.Length == 0)
            {
                download_file = "W:\\Подразделения\\Дир - ИТ\\Связь\\download\\mts.xml";
                Console.WriteLine("Назначен Файл загрузки МТС (формат XML) - W:\\Подразделения\\Дир-ИТ\\Связь\\download\\mts.xml");
                Console.WriteLine("--------ДЛЯ МТС ПОЛНЫЙ ПУТЬ ФАЙЛА ЗАГРУЗКИ МОЖНО ПЕРЕДАТь ПАРАМЕТРОМ КОМАНДНОЙ СТРОКИ------");
            }
            else
            {
                download_file = args[0];
                Console.WriteLine("Назначен файл зайгрузки " + args[0]);

            }

            Console.WriteLine("Назначен Файл загрузки Мегафон (формат CSV, полученный из Excel файла)- W:\\Подразделения\\Дир-ИТ\\Связь\\download\\Megafon.txt");
            Console.WriteLine("Если оператор МТС нажмите цифру 1, если Мегафон - 2, для выхода - 0");
            Oper = Console.ReadLine();
            if (Oper == "1") // работаем с МТС
            {
                // работаем с МТС          
                //   foreach ( var m in MTS.Load("mts.xml"));
                //    foreach (var m in MTS.Load("W:\\Подразделения\\Дир-ИТ\\Связь\\download\\mts.xml")) ;
                foreach (var m in MTS.Load(download_file)) ;
            }
            else
            {
                if (Oper == "2")  // работаем с Мегафон
                {
                    foreach (var b in Megafon.Load("W:\\Подразделения\\Дир-ИТ\\Связь\\download\\Megafon.txt")) ;
                }
                else
                {
                    Console.WriteLine("Работа завершена");
                }
            }
            

        }
    }

    class MTS
    {
        
        public static IEnumerable<MTS> Load(string path)
        {

            int Year_event = 2017;
            int Month_event = 0;
            String Account_Number = "";

            int GlobalCounter = 0; // счетчик событий во всей загрузке
            String CurrentTag = "";
            String phone_number = "";
            Decimal CalcCost = 0;
            Decimal BillCost = 0;
            Decimal SummCalcCost = 0; //накопитель суммы по счету из расчетных данных
            Decimal SummBillCost = 0; //Накопитель суммы по счету из итоговых полей счета

            MTS ret = null;
            ret = new MTS { };

            XmlTextReader textReader = new XmlTextReader(path);
            textReader.Read();
            while (textReader.Read())
            {
                // Объявляем и обнуляем перемпенные при каждой считанной строке
                String date_event = "";
                String call_number = "";
                String call_area = "";
                String target_area = "";
                int input_call = 0;
                String service = "";
                String duration = "";
                String cost = "";
                String Mess = "";
                String Day_event = "";
                int Time_min = 0;
                String Final_Time = "";
                String t = "";
                Decimal delta = 0;


                // Тип всех узлов это Элементы
                XmlNodeType nType = textReader.NodeType;
                if (nType == XmlNodeType.Element)
                {
                    CurrentTag = textReader.Name;
                    if (ret != null && CurrentTag == "f") // финальная проверка на необходимость завершения работы со счетом и вывод итогов
                    {
                        Console.WriteLine("Обработано " + GlobalCounter.ToString() + " строк");
                        Console.WriteLine("Фактическсая сумма из расчета по расшифровке строк(Без НДС) " + SummCalcCost.ToString());
                        Console.WriteLine("Формальная сумма из расчета по документу (Без НДС) " + SummBillCost.ToString());
                        Console.WriteLine("Для сравнения реальную сумму см. из счет-фактуры (Без НДС) в документе c сайта МТС");

                        yield return ret;
                    }

                    if (CurrentTag == "b") // Это шапка документа из нее берег глобальные данные
                    {

                        Account_Number = textReader.GetAttribute("an");

                        if (textReader.GetAttribute("ed").Length == 10)
                        {
                            String temp = textReader.GetAttribute("ed");
                            Year_event = Convert.ToInt32(temp.Substring(6, 4));
                            Month_event = Convert.ToInt32(temp.Substring(3, 2));
                        }
                    }

                    // В таком элементе храянться номера абонента для аб. платы
                    if (textReader.Name == "tp")
                    {
                        if (textReader.GetAttribute("t") == "Сетевой ресурс")
                        {

                            if ((phone_number != textReader.GetAttribute("n")) && phone_number != "")
                            //прошла смена номера
                            // мы должны для старого номера абонента проверить накопленную сумму, сравнив ее с суумой по итоговому полю из счета для этого номера
                            // и если будет обнаружена разница - ее нужно будет записать в БД корректирующей записью: delta
                            {
                                delta = BillCost - CalcCost;
                                Console.WriteLine("Дельта =" + delta.ToString() + " на номере: " + phone_number + " Вычислено= " + CalcCost + " Начислено= " + BillCost);
                                CalcCost = 0;
                                  
                                    //Собственно добавка в базу
                                    if (delta != 0)
                                    {

                                        String de_p = textReader.GetAttribute("ed");
                                        String ph_p = phone_number;
                                        int ye_p = Year_event;
                                        int me_p = Month_event;
                                        String ac_p = Account_Number;
                                        String delta_p = delta.ToString();
                                        int gcou_p = GlobalCounter;

                                        String da_p = de_p.Substring(0, 2);
                                        int count_ret = 0;
                                        try { 
                                            count_ret = AddToDB(de_p, ph_p, "00:00:00", "Correct", " ", "Correct", " ", ye_p, me_p, da_p, ac_p, "0", delta_p, " ", 0, "MTS");
                                            GlobalCounter = GlobalCounter + 1;
                                            SummCalcCost = SummCalcCost + delta;
                                            }
                                        catch
                                            {
                                            Console.WriteLine("Корректирующая запись не добавлена в Базу Данных");
                                            }
                                    }
                            }

                            if (textReader.GetAttribute("n") != phone_number)
                            {
                                BillCost = Convert.ToDecimal(textReader.GetAttribute("awt"));
                                SummBillCost = SummBillCost + BillCost;

                            }

                        }

        
                        phone_number = textReader.GetAttribute("n"); //актуализируем поле текущего номера абонента
                        
                    }
                }

                // В таком элементе храянться номера абонента для собственно биллинга
                if (textReader.Name == "ds")
                {
                    if (textReader.GetAttribute("type") == "Сетевой ресурс")
                    {
                        phone_number = textReader.GetAttribute("n");
                    }
                }
                // В таком элементе храняться строки-расшифровки вызовов, каждую из них запишем в БД
                if ((textReader.Name == "i") || (textReader.Name == "ss"))
                {
                    if ((((textReader.AttributeCount >= 11) && (textReader.AttributeCount <= 15)) && (textReader.Name == "i")) || (textReader.Name == "ss"))
                    {
                        //if (phone_number.Trim()== "79129829555")
                        //{
                        //    //здесь!!
                        //}

                        if (textReader.Name == "ss") // здесь абонентскач плата
                        {
                            if (phone_number=="791224963905")
                            {
                                //здесь!
                            }
                            date_event = textReader.GetAttribute("sd"); //
                            Day_event = date_event.Substring(0, 2);     //
                            call_number = "Subscriber";                 //
                            service = textReader.GetAttribute("n");   //
                            cost = textReader.GetAttribute("awt");    //
                            if (cost != "")
                            {
                                CalcCost = CalcCost + Convert.ToDecimal(cost);
                                SummCalcCost = SummCalcCost + Convert.ToDecimal(cost);
                            }

                            if (textReader.GetAttribute("ed") == null)
                            { 
                               Console.WriteLine("Есть изменение условий на номере:" + phone_number); 
                            }

                        }
                        else  // здесь собственно строки расшифровки (биллинг)
                        {
                            date_event = textReader.GetAttribute("d"); //
                            Day_event = date_event.Substring(0, 2);  //
                            call_number = textReader.GetAttribute("n"); //
                            call_area = textReader.GetAttribute("zp");  //
                            target_area = textReader.GetAttribute("zv");
                            service = textReader.GetAttribute("s");  //
                            cost = textReader.GetAttribute("c");  //
                            if (cost != "")
                            {
                                CalcCost = CalcCost + Convert.ToDecimal(cost);
                                SummCalcCost = SummCalcCost + Convert.ToDecimal(cost);
                            }

                            duration = textReader.GetAttribute("du");
                            if (call_number.Length >= 3)
                            {
                                if (call_number.Substring(0, 3) == "<--")
                                {
                                    input_call = 1;
                                }
                                else
                                {
                                    input_call = 0;
                                }
                            }
                            if (duration.Length >= 2 && duration.EndsWith("Kb"))
                            {
                                Mess = "Килобайт";
                            }
                            else
                            {
                                if (service.Trim() == "Телеф.")
                                {
                                    Mess = "Секунда";
                                }
                                else
                                {
                                    Mess = "Штука";
                                }
                            }
                            // Выделим минуты голосового трафика
                            if (duration.Length == 4 && duration.Substring(1, 1) == ":")
                            {
                                Time_min = Convert.ToInt32(duration.Substring(0, 1)) * 60 + Convert.ToInt32(duration.Substring(2, 2));
                            }
                            if (duration.Length == 5 && duration.Substring(2, 1) == ":")
                            {
                                Time_min = Convert.ToInt32(duration.Substring(0, 2)) * 60 + Convert.ToInt32(duration.Substring(3, 2));
                            }

                            if (Time_min != 0)
                            {
                                Final_Time = Time_min.ToString();
                            }
                            else  // Выделим килобайты интернет трафика
                            {
                                if (duration.EndsWith("Kb"))
                                {
                                    int LenDur = duration.Trim().Length;
                                    int LenWast = LenDur - 2;
                                    Final_Time = duration.Substring(0, LenWast);
                                }
                                else
                                    Final_Time = duration;
                            }
                            // Еще одна проверка на вшивость, если пролез символ : не допускаем его появления!
                            if (Final_Time.IndexOf(":") >= 0)
                            {
                                Final_Time = "0";
                            }
                        }

                        string str_d = date_event.Substring(0, 10);

                        if (date_event.Length == 18)
                        {
                            t = date_event.Substring(11, 7);
                        }
                        else
                        {
                            if (date_event.Length == 19)
                            {
                                t = date_event.Substring(11, 8);
                            }
                        }
                       // Нижде попытка пополнить БД
                        int count_ret = 0;
                        try
                        {
                            
                            count_ret = AddToDB(str_d, phone_number , t.Trim(), service.Trim(), target_area.Trim(), call_number.Trim(), call_area.Trim(), Year_event, Month_event, Day_event.Trim(), Account_Number, Final_Time, cost.Trim(), Mess.Trim(), input_call, "MTS");
                            GlobalCounter = GlobalCounter + 1;

                        }
                        catch
                        {
                            Console.WriteLine("Pапись расшифровки не добавлена в Базу Данных");
                        }

                    }

                }
            }
        }

        public static int AddToDB(String de, String ph, String tm, String sr, String ta, String cn, String ca, int yev, int mev, String dev, String an, String du, String co, String ms, int ic, String op)
        {
            int ret_num = 0;
            System.Data.SqlClient.SqlConnection sqlConnection1 =
            // new System.Data.SqlClient.SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MobileBase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
             new System.Data.SqlClient.SqlConnection("Data Source=b-sql-test;Initial Catalog=MobileBase;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");


            string str_d = de.Substring(0, 10); // получили дату

            DateTime? d;
            try
            {
                d = DateTime.ParseExact(str_d, "d", null);

                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "INSERT MobileTable (phone_number,date_event,time_event,service,target_area,callnumber,call_area,year_event,month_event,day_event,clientaccount,duration,cost,mess,input_call,operator)" +
                    "VALUES (@phone_number,@date_event,@time_event,@service,@target_area,@callnumber,@call_area,@year_event,@month_event,@day_event,@clientaccount,@duration,@cost,@mess,@input_call,@operator)";
                {
                    // Добавить параметры
                    cmd.Parameters.AddWithValue("@phone_number", ph.Trim());// получили номер
                    cmd.Parameters.AddWithValue("@date_event", d); // дата вычеслена вданной функции
                    cmd.Parameters.AddWithValue("@time_event", tm.Trim()); //полуили время
                    cmd.Parameters.AddWithValue("@service", sr.Trim()); //получили сервис
                    cmd.Parameters.AddWithValue("@target_area", ta.Trim()); // получили решгон назначения 
                    cmd.Parameters.AddWithValue("@callnumber", cn.Trim()); // получили номер  оппонента
                    cmd.Parameters.AddWithValue("@call_area", ca.Trim()); // получили регион оппонента
                    cmd.Parameters.AddWithValue("@year_event", yev); // получили год
                    cmd.Parameters.AddWithValue("@month_event", mev); //получили месяц
                    cmd.Parameters.AddWithValue("@day_event", Convert.ToDecimal(dev)); // день события
                    cmd.Parameters.AddWithValue("@clientaccount", an);  // лицевой счет
                    if (du == "")
                                {
                                    cmd.Parameters.AddWithValue("@duration", 0); // продолжительность    = 0
                                }
                    else
                                {
                                    cmd.Parameters.AddWithValue("@duration", Convert.ToDecimal(du.Trim())); // продолжительность    
                                }
                    
                    cmd.Parameters.AddWithValue("@cost", Convert.ToDecimal(co.Trim())); // стоимость
                    cmd.Parameters.AddWithValue("@mess", ms.Trim()); //единица измерения
                    cmd.Parameters.AddWithValue("@input_call", ic); //признак входного звонка(сообщения)
                    cmd.Parameters.AddWithValue("@operator", op.Trim()); // Оператор связи

                }
                cmd.Connection = sqlConnection1;
                sqlConnection1.Open();
                cmd.ExecuteNonQuery();
                ret_num = 1;
            }
            catch (SystemException)
            {
                d = null;
                 Console.WriteLine("Ошибка записи в БД - исключительная ситуация!");

            }
            sqlConnection1.Close();
            return ret_num;

        }

    }






    class Megafon
    {

        public IList<string> Body;
        public string PhoneNumber;

        public static IEnumerable<Megafon> Load(string path)
        {
            String CurrentPhone = ""; //здесь храним текущий обрабатываемый номер, сначала его еще нет
            int GlobalCounter = 0; // счетчик событий во всей загрузке
            Megafon ret = null;
            String Separator = ";"; //разделитель данных в формате CSV
            bool ItIsRouming = false;
            String Place = "";
            String Mess = "";
            String Cost = "";
            int input_call = 0;
            String Day_event = "";

            // !!!! Поля ниже следует корректировать перед каждой загрузкой!!!!
            int Year_event = 0;
            Console.WriteLine("Введите год и нажмите Enter, например: 2017");
            try
            {
                Year_event = Convert.ToInt32(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Загрузка превана из-за ошибки ввода");
            }

            int Month_event = 0;

            Console.WriteLine("Введите месяц числом и нажмите Enter, например: 5");
            try
            {
                Month_event = Convert.ToInt32(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Загрузка превана из-за ошибки ввода");
            }
            String Account_Number = "";
            Console.WriteLine("Введителицевой счет и нажмите Enter , например: 73711191");
            try
            {
                Account_Number = Console.ReadLine();
            }
            catch
            {
                Console.WriteLine("Загрузка превана из-за ошибки ввода");
            }
            // !!!! Поля выше следует корректировать перед каждой загрузкой!!!!



            System.Data.SqlClient.SqlConnection sqlConnection1 =
           // new System.Data.SqlClient.SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MobileBase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
               new System.Data.SqlClient.SqlConnection("Data Source=b-sql-test;Initial Catalog=MobileBase;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");



            foreach (var line in File.ReadLines(path, Encoding.GetEncoding("windows-1251")).Select(l => l.Trim()))
            {
                //  if (line.Length == 0 && ret != null)// если подготвленные данные уже есть и достигнута  пустая строка
                if (ret != null && line.StartsWith("Всего по всем абонентам"))
                //if (ret != null && GlobalCounter>=20)
                {
                    yield return ret;
                    ret = null;
                    continue;
                }

                // определим новый номер или продолжаем рабоать со старым
                if (line.StartsWith("Детализация"))
                {
                    if (CurrentPhone == "")
                    {
                        ret = new Megafon { Body = new List<string>(), PhoneNumber = CurrentPhone };
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
                    string str_d = line.Substring(0, 6) + "20" + line.Substring(6, 2);
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

                            String line2 = line + CurrentPhone + ";" + Year_event.ToString() + ";" + Month_event.ToString() + ";" + Day_event + ";" + Account_Number;
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
                                if (Mess == "Минута")
                                { Mess = "Секунда"; }
                            }
                            else
                            {
                                Mess = splitResult[15];
                                if (Mess == "Минута")
                                { Mess = "Секунда"; }

                            }

                            if (ItIsRouming)
                            {
                                Cost = splitResult[16];
                            }
                            else
                            {
                                Cost = splitResult[18];
                            }
                            // Console.WriteLine("Стоимость: " + splitResult[18]);
                            // Console.WriteLine("Номер: " + splitResult[25]);
                            // Console.WriteLine("Год: " + splitResult[26]);
                            // Console.WriteLine("Месяц: " + splitResult[27]);
                            // Console.WriteLine("День: " + splitResult[28]);
                            // Console.WriteLine("Лицевой счет: " + splitResult[29]);
                            if (splitResult[5].Substring(0, 4) == "Вход")
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
                            cmd.CommandText = "INSERT MobileTable (phone_number,date_event,time_event,service,target_area,callnumber,call_area,year_event,month_event,day_event,clientaccount,duration,cost,mess,input_call,operator)" +
                                "VALUES (@phone_number,@date_event,@time_event,@service,@target_area,@callnumber,@call_area,@year_event,@month_event,@day_event,@clientaccount,@duration,@cost,@mess,@input_call,@operator)";
                            {
                                // Добавить параметры
                                cmd.Parameters.AddWithValue("@phone_number", splitResult[25].Trim());
                                cmd.Parameters.AddWithValue("@date_event", d);
                                cmd.Parameters.AddWithValue("@time_event", splitResult[3].Trim());
                                cmd.Parameters.AddWithValue("@service", splitResult[5].Trim());
                                cmd.Parameters.AddWithValue("@target_area", splitResult[6].Trim());
                                cmd.Parameters.AddWithValue("@callnumber", splitResult[9].Trim());
                                cmd.Parameters.AddWithValue("@call_area", Place.Trim());
                                cmd.Parameters.AddWithValue("@year_event", Year_event);
                                cmd.Parameters.AddWithValue("@month_event", Month_event);
                                cmd.Parameters.AddWithValue("@day_event", splitResult[28].Trim());
                                cmd.Parameters.AddWithValue("@clientaccount", Account_Number.Trim());
                                cmd.Parameters.AddWithValue("@duration", Convert.ToDecimal(Final_Time.Trim()));
                                cmd.Parameters.AddWithValue("@cost", Convert.ToDecimal(Cost.Trim()));
                                cmd.Parameters.AddWithValue("@mess", Mess.Trim());
                                cmd.Parameters.AddWithValue("@input_call", input_call);
                                cmd.Parameters.AddWithValue("@operator", "Megafon");


                            }
                            cmd.Connection = sqlConnection1;
                            sqlConnection1.Open();
                            cmd.ExecuteNonQuery();
                            Console.WriteLine("Запись строки: " + GlobalCounter.ToString());
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

    


