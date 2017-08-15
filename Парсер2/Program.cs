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
                download_file = "W:\\Подразделения\\Дир-ИТ\\Связь\\download\\mts.xml";
                Console.WriteLine("По умолчанию: назначен Файл загрузки МТС (формат XML) - W:\\Подразделения\\Дир-ИТ\\Связь\\download\\mts.xml");
                Console.WriteLine("--------ДЛЯ МТС ПОЛНЫЙ ПУТЬ ФАЙЛА ЗАГРУЗКИ МОЖНО ПЕРЕДАТь ПАРАМЕТРОМ КОМАНДНОЙ СТРОКИ------");
            }
            else
            {
                download_file = args[0];
                Console.WriteLine("Назначен файл зайгрузки МТС " + args[0]);

            }

            Console.WriteLine("Назначен Файл загрузки Теле2 (формат CSV, полученный из Excel файла, расширение - .csv)- W:\\Подразделения\\Дир-ИТ\\Связь\\download\\Tele2.csv");
            Console.WriteLine("Назначен Файл загрузки Мегафон (формат CSV, полученный из Excel файла, расширение .txt)- W:\\Подразделения\\Дир-ИТ\\Связь\\download\\Megafon.txt");
            Console.WriteLine("Если оператор МТС нажмите цифру 1, если Теле2 - 2, если Мегафон - 3, для выхода - 0");

            Oper = Console.ReadLine();
            if (Oper == "1") // работаем с МТС
            {
                // работаем с МТС          
                //   foreach ( var m in MTS.Load("mts.xml"));
                //    foreach (var m in MTS.Load("W:\\Подразделения\\Дир-ИТ\\Связь\\download\\mts.xml")) ;
                foreach (var m 
                    in MTS.Load(download_file)) ;
            }
            else
            {
                if (Oper == "2")  // работаем с Теле2
                {
                    foreach (var b  in Tele2.Load("W:\\Подразделения\\Дир-ИТ\\Связь\\download\\Tele2.csv")) ;
                }
                else
                if (Oper == "3")  // работаем с Мегафон
                {
                    foreach (var b in Megafon.Load("W:\\Подразделения\\Дир-ИТ\\Связь\\download\\Megafon.txt")) ;
                }


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


                if ((nType == XmlNodeType.Element) || (nType == XmlNodeType.EndElement))
                {

                    CurrentTag = textReader.Name; // Пропустим проблемный конечный элемент
                    if ((CurrentTag == "pd") || ((CurrentTag == "ss") && (nType== XmlNodeType.EndElement))) 
                    {
                        continue;
                    }

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
                        if (phone_number.Trim() == "79129845502")
                        {
                            //здесь!!
                        }

                        if (textReader.Name == "ss") // здесь абонентскач плата
                        {
                            if (phone_number=="79129845502")
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
          //  new System.Data.SqlClient.SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MobileBase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
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
            Decimal SummCalcCost = 0;
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
           //  new System.Data.SqlClient.SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MobileBase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
             new System.Data.SqlClient.SqlConnection("Data Source=b-sql-test;Initial Catalog=MobileBase;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");



            foreach (var line in File.ReadLines(path, Encoding.GetEncoding("windows-1251")).Select(l => l.Trim()))
            {
                Decimal Final_Cost = 0;

                //  if (line.Length == 0 && ret != null)// если подготвленные данные уже есть и достигнута  пустая строка
                if (ret != null && line.StartsWith("Всего по всем абонентам"))
                //if (ret != null && GlobalCounter>=20)
                {
                    Console.WriteLine("Обработано " + GlobalCounter.ToString() + " строк");
                    Console.WriteLine("Фактическсая сумма из расчета по расшифровке строк(Без НДС) " + SummCalcCost.ToString());
                    Console.WriteLine("Для сравнения реальную сумму см. из счет-фактуры (Без НДС) в документе c сайта Megafon");
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
                        Console.WriteLine("Обработка номера: " + CurrentPhone);
                        continue;
                    }

                } // здесь была строка с номером
                else // для других строк ловим данные
                {
                    string str_d = line.Substring(0, 6) + "20" + line.Substring(6, 2);
                    //  string str_d = "01.05.2017";

                    int Time_min = 0;
                    String Final_Time = "";

                    DateTime? d;
                    try
                    {
                        d = DateTime.ParseExact(str_d, "d", null);
                        if (ret != null)
                        {
                            // ret.Body.Add(line + CurrentPhone);
                     
                      
                        }
                    }
                    catch (SystemException)
                    {
                        d = null;
                        // Console.WriteLine("Ошибка распознания даты, "+CurrentPhone);
                        continue;

                    }

                    Day_event = line.Substring(0, 2);
                    GlobalCounter++;
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
                    else // разбор минут не в роуминге
                    {
                        if (splitResult[14].Length == 5)
                        {
                            if (splitResult[14].Substring(2, 1) == ":")
                            {
                                Time_min = Convert.ToInt32(splitResult[14].Substring(0, 2)) * 60 + Convert.ToInt32(splitResult[14].Substring(3, 2));
                            }
                            else
                                Time_min = 0;
                        }
                        else
                        {
                            if (splitResult[14].Length == 8)
                            {
                                if (splitResult[14].Substring(2, 1) == ":")
                                {
                                    Time_min = Convert.ToInt32(splitResult[14].Substring(0, 2)) * 3600 + Convert.ToInt32(splitResult[14].Substring(3, 2)) * 60 + Convert.ToInt32(splitResult[14].Substring(6, 2));
                                }
                                else
                                    Time_min = 0;
                            }
                        }

                        if (Time_min != 0)
                        {
                            Final_Time = Time_min.ToString();
                        }
                        else
                        {
                            if (splitResult[14] != "")
                            {
                                Final_Time = splitResult[14];
                            }
                            else
                                Final_Time = "1"; // подстраховка на тот случай, когда в количестыве SMS не указано "1 штука"

                        }
                    }
                    // Console.WriteLine("Прод/Объем: " + Final_Time);
                    // Console.WriteLine("Единица: " + splitResult[15]);
                    if (ItIsRouming)
                    {
                        Mess = splitResult[13];
                        if ((Mess == "Минута") || (Mess == "Минут"))
                        { Mess = "Секунда"; }
                    }
                    else
                    {
                        Mess = splitResult[15];
                        if ((Mess == "Минута") || (Mess == "Минут"))
                        { Mess = "Секунда"; }

                    }

                    if (ItIsRouming)
                    {
                        Cost = splitResult[16];
                        Final_Cost = Convert.ToDecimal(Cost.Trim());
                        Final_Cost = Final_Cost / Convert.ToDecimal(1.18);
                        SummCalcCost = SummCalcCost + Final_Cost;
                    }
                    else
                    {
                        Cost = splitResult[18];
                        Final_Cost = Convert.ToDecimal(Cost.Trim());
                        Final_Cost = Final_Cost / Convert.ToDecimal(1.18);
                        SummCalcCost = SummCalcCost + Final_Cost;
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

                    try
                    {

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
                            cmd.Parameters.AddWithValue("@cost", Final_Cost);
                            cmd.Parameters.AddWithValue("@mess", Mess.Trim());
                            cmd.Parameters.AddWithValue("@input_call", input_call);
                            cmd.Parameters.AddWithValue("@operator", "Megafon");


                        }
                        cmd.Connection = sqlConnection1;
                        sqlConnection1.Open();
                        cmd.ExecuteNonQuery();
                        sqlConnection1.Close();
                     

                        }
                    catch
                    {
                        sqlConnection1.Close();
                        Console.WriteLine("Ошибка Записи строки: " + GlobalCounter.ToString());
                        
                    }
                }





            }
        }
    }



    class Tele2
    {

        public IList<string> Body;
        public string PhoneNumber;

        public static IEnumerable<Tele2> Load(string path)
        {
            String CurrentPhone = ""; //здесь храним текущий обрабатываемый номер, сначала его еще нет
            int GlobalCounter = 0; // счетчик событий во всей загрузке
            Decimal SummCalcCost = 0;
            Tele2 ret = null;
            String Separator = ";"; //разделитель данных в формате CSV

       
            // !!!! Поля ниже следует вносить при каждой загрузке!!!!
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
            Console.WriteLine("Введителицевой счет и нажмите Enter , например: 1905909");
            try
            {
                Account_Number = Console.ReadLine();
            }
            catch
            {
                Console.WriteLine("Загрузка превана из-за ошибки ввода");
            }
            // !!!! Поля выше следует вносить при  каждой загрузке!!!!



            System.Data.SqlClient.SqlConnection sqlConnection1 =
           //     new System.Data.SqlClient.SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MobileBase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
             new System.Data.SqlClient.SqlConnection("Data Source=b-sql-test;Initial Catalog=MobileBase;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");


            foreach (var line in File.ReadLines(path, Encoding.GetEncoding("windows-1251")).Select(l => l.Trim()))
            {
                GlobalCounter++;
                String Place = "";
                String Mess = "";
                String Cost = "";
                int input_call = 0;
                String Day_event = "";
                String Time_event = "";
                String Service = "";
                String Callnumber = "";
                String Final_Time = "";
                decimal Final_Time_d = 0;
           //     String Month_event_subscriber = "";
                String Target_area = "";
                Decimal Final_Cost = Convert.ToDecimal(0);


                //  if (line.Length == 0 && ret != null)// если подготвленные данные уже есть и достигнута  пустая строка
                if (ret != null && line.StartsWith("Всего по всем абонентам"))
                //if (ret != null && GlobalCounter>=20)
                {
                    Console.WriteLine("Обработано " + GlobalCounter.ToString() + " строк");
                    Console.WriteLine("Фактическсая сумма из расчета по расшифровке строк(без НДС) " + SummCalcCost.ToString());
                    Console.WriteLine("Для сравнения реальную сумму см. из счет-фактуры (без НДС) в документе c сайта Tele2");
                    yield return ret;

                    ret = null;
                    continue;
                }
                // Пропустим незначимые строки
                if (line.Length < 11)
                {
                    continue;
                }

                // определим новый номер или продолжаем работать со старым
                if ((line.StartsWith("Абонент: ")) || (line.Substring(0, 11) == "Начисления:"))
                {
                    if (line.StartsWith("Абонент: "))
                    {
                        if (CurrentPhone == "")
                        {
                            ret = new Tele2 { Body = new List<string>(), PhoneNumber = CurrentPhone };
                        }

                        string Phone = line.Substring(9, 11); // выделяем из строки номер
                        if (CurrentPhone != Phone)
                        {
                            // здесь можно инициализирвоать новый объект для очередного номера телефона
                            CurrentPhone = Phone;
                            Console.WriteLine("Обработка номера: " + CurrentPhone);
                            continue;
                        }
                    }
                    else
                    {
                        //  При отладке обнаружил, что это дублирующее начисление, хотя здесь реализована интересная идея  использовать статическую функцию из другого класса!
                        //    if (line.StartsWith("Начисления:"))
                        //    {

                        //        DateTime? d;
                        //        try
                        //        {
                        //            int month_lenght = Month_event.ToString().Trim().Length;
                        //            if(month_lenght==1)
                        //            {
                        //                Month_event_subscriber = "0" + Month_event.ToString().Trim();
                        //            }
                        //            else
                        //            {
                        //                Month_event_subscriber = Month_event.ToString().Trim();

                        //            }

                        //            d = DateTime.ParseExact("01." + Month_event_subscriber + "." + Year_event.ToString().Trim(), "d", null);

                        //        }
                        //        catch (SystemException)
                        //        {
                        //            d = null;
                        //            Console.WriteLine("Ошибка распознания даты, "+CurrentPhone);
                        //            continue;

                        //        }
                        //        Service = "Subscriber";
                        //        Callnumber = "Subscriber";
                        //        Place = "";
                        //        Target_area = "";
                        //        Day_event = "01";
                        //        Time_event = "00:00:00";
                        //        string[] splitResult = Regex.Split(line, Separator);
                        //        Cost = splitResult[9];
                        //        Final_Cost = Convert.ToDecimal(Cost.Trim());
                        //        Final_Cost = Final_Cost / Convert.ToDecimal(1.18);
                        ////        SummCalcCost = SummCalcCost + Final_Cost;
                        //        Final_Time = "";
                        //        Mess = "";
                        //        input_call = 0;
                        //        String str_d = Day_event + "." + Month_event_subscriber + "." + Year_event.ToString();

                        //        //  Сделать вызов функции записи в БД по аналогии c МТС
                        //        int count_ret = 0;
                        //        try
                        //        {
                        //           count_ret = MTS.AddToDB(str_d, CurrentPhone, Time_event, Service, Target_area, Callnumber, Place, Year_event, Month_event, Day_event, Account_Number, Final_Time, Final_Cost.ToString(), Mess.Trim(), input_call, "Tele2");
                        //          }
                        //        catch
                        //        {
                        //            Console.WriteLine("Pапись расшифровки не добавлена в Базу Данных");
                        //        }

                        //        continue;
                        //    }
                    }

                } // здесь была строка с номером или аб. платой
                else // иначе пытаемся поймать поля с датой
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
                        }
                    }
                    catch (SystemException)
                    {
                        d = null;
                        // Console.WriteLine("Ошибка распознания даты, "+CurrentPhone);
                        continue;

                    }

                    Day_event = line.Substring(0, 2);
                    Time_event = line.Substring(9, 8);
               
                     string[] splitResult = Regex.Split(line, Separator);

                    //!!!!!! Ниже поля "Начисление:" - для них необходимо учесть особенности начислений по типу Тихомирва, номер: ...272
                    if (splitResult[1].StartsWith("Начисление:"))
                    {
                        Place = splitResult[1].Trim(); //Для Теле2 условно запишем в это поле Вид тарифа
                        Service = "Subscriber";
                        Callnumber = splitResult[6].Trim();
                        Final_Time_d = 0;
                        Mess = "";
                        input_call = 0;

                    }
                    else
                    {
                        Place = splitResult[1].Trim(); //Для Теле2 условно запишем в это поле Вид тарифа
                        Service = splitResult[2].Trim();
                        Callnumber = splitResult[4].Trim();
                  
                    }
                    // разбор минут 
                    if (splitResult[7].Length == 5) // минуты-секунды
                    {
                        if (splitResult[7].Substring(2, 1) == ":")
                        {
                            if (splitResult[7] == "00:00") // Это СМС
                            {
                                Final_Time_d = 1;
                                Mess = "Штука";
                            }
                            else  // в других случаях голосовой трафик
                            {
                                Final_Time_d = Convert.ToInt32(splitResult[7].Substring(0, 2)) * 60 + Convert.ToInt32(splitResult[7].Substring(3, 2));
                                Mess = "Секунда";
                            }
                        }
                        else
                            Final_Time_d =0;
                    }
                    else
                    {
                        if (splitResult[7].Length == 8) // часы-минуты-секунды
                        {
                            if (splitResult[7].Substring(2, 1) == ":")
                            {
                                Final_Time_d = Convert.ToInt32(splitResult[7].Substring(0, 2)) * 3600 + Convert.ToInt32(splitResult[7].Substring(3, 2)) * 60 + Convert.ToInt32(splitResult[7].Substring(6, 2));
                                Mess = "Секунда";
                            }
                            else
                                Final_Time_d = 0;
                        }

                    }

                    if ((splitResult[7].Length == 0) && (splitResult[2] == "Internet-трафик")) //А здесь интернет трафик
                    {
                            // Здесь нужно сделать разбор поля 4 - callnumber и полчить число байт а затем перевести в килобайты!!!
                            if (splitResult[4].IndexOf("байт") >= 0)
                            {
                                int pos_bait_start = 7;
                                int pos_bait_end = splitResult[4].IndexOf("байт");
                                Final_Time = splitResult[4].Substring(pos_bait_start, pos_bait_end-pos_bait_start);// подставить значение байт!!!!
                                Final_Time_d =Convert.ToDecimal(Final_Time.Trim())/1024;// переводим из строки в килобайты и числовое значение   
                                Final_Time = Final_Time_d.ToString();
                                Mess = "Килобайт";
                            }
                            
                    }

                        Cost = splitResult[9];
                        Final_Cost = Convert.ToDecimal(Cost.Trim());
                        Final_Cost = Final_Cost / Convert.ToDecimal(1.18);
                        SummCalcCost = SummCalcCost + Final_Cost; 

                    if (splitResult[2].Length >= 5)
                    {
                        if ((splitResult[2].Substring(0, 5) == "Исход") || (splitResult[2].Substring(0, 3) == "SMS"))
                        {
                            input_call = 0;
                        }
                        else
                        {
                            input_call = 1;
                        }
                        if((splitResult[2].LastIndexOf("SMS")>=0) && (Mess==""))
                        {
                            Mess = "Штука";
                            Final_Time_d = Convert.ToDecimal(1);
                        }
                    }

                    try
                    {

                        System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
                        cmd.CommandType = System.Data.CommandType.Text;
                       // cmd.CommandText = "INSERT MobileTable (phone_number) VALUES ('79026459606')";
                        cmd.CommandText = "INSERT MobileTable (phone_number,date_event,time_event,service,target_area,callnumber,call_area,year_event,month_event,day_event,clientaccount,duration,cost,mess,input_call,operator)" +
                            "VALUES (@phone_number,@date_event,@time_event,@service,@target_area,@callnumber,@call_area,@year_event,@month_event,@day_event,@clientaccount,@duration,@cost,@mess,@input_call,@operator)";
                        {
                            // Добавить параметры
                            cmd.Parameters.AddWithValue("@phone_number", CurrentPhone.Trim());//+
                            cmd.Parameters.AddWithValue("@date_event", d); //+
                            cmd.Parameters.AddWithValue("@time_event", Time_event.Trim());//+
                            cmd.Parameters.AddWithValue("@service", Service.Trim());//+
                            cmd.Parameters.AddWithValue("@target_area", Target_area);//+
                            cmd.Parameters.AddWithValue("@callnumber", Callnumber.Trim());//+
                            cmd.Parameters.AddWithValue("@call_area", Place.Trim()); //+
                            cmd.Parameters.AddWithValue("@year_event", Year_event);//+
                            cmd.Parameters.AddWithValue("@month_event", Month_event);//+
                            cmd.Parameters.AddWithValue("@day_event", Day_event.Trim());//+
                            cmd.Parameters.AddWithValue("@clientaccount", Account_Number.Trim());//+
                            cmd.Parameters.AddWithValue("@duration",Final_Time_d); //+
                            cmd.Parameters.AddWithValue("@cost", Final_Cost);//+
                            cmd.Parameters.AddWithValue("@mess", Mess.Trim());//+
                            cmd.Parameters.AddWithValue("@input_call", input_call);//+
                            cmd.Parameters.AddWithValue("@operator", "Tele2");//+


                        }
                        cmd.Connection = sqlConnection1;
                        sqlConnection1.Open();
                        cmd.ExecuteNonQuery();
                        sqlConnection1.Close();

                    }
                    catch
                    {
                        sqlConnection1.Close();
                        Console.WriteLine("Ошибка Записи строки: " + GlobalCounter.ToString());
                    }
                }

            }
        }
    }


}

    


