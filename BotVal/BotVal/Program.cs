using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using System.Timers;
using System.Collections.Generic;
using System.Xml;
using System.Data.SqlClient;
using BotVal;
using Microsoft.Office.Interop.Excel;
using System.Linq;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace BotVal
{
    class Program
    {
        static double intervalping = 60000;
        static SqlConnection connection = new SqlConnection("Server=tcp:azurez.database.windows.net,1433;Initial Catalog=Zakha_db;Persist Security Info=False;User ID=admnz;Password=a73IR00l;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        static TelegramBotClient client;
        static List<Client> chats = new List<Client>();

        static void Main(string[] args)
        {
            
            GetUsers();
            


            client = new TelegramBotClient("1322903188:AAHNRgjAopnGninojF8XWd8xtUO3EmjswIg");
            client.OnMessage += getMsg;
           
            client.StartReceiving();



            Timer task = new Timer(intervalping);
            task.Elapsed += SendInf;
            task.Start();
            Console.Read();
        }

        private static void GetUsers()
        {

            connection.Open();

            using (SqlCommand command = new SqlCommand($"SELECT*FROM Client", connection))
            {
                try
                {
                    //command.Parameters.Add(new SQLiteParameter("@id_q", id_question));
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {                    
                        chats.Add(new Client(reader.GetInt32(1), reader.GetDecimal(2),reader.GetString(7)));
                    }
                    //connection.Close();


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            connection.Close();


        }





        private static void CreateDataBase(string path)
        {

            connection.Open();

            using (SqlCommand command = new SqlCommand("CREATE TABLE IF NOT EXISTS Client" +
                "([id] INTEGER PRIMARY KEY AUTOINCREMENT," +
                "[chatId] INTEGER NOT NULL UNIQUE);", connection))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            connection.Close();

        }

        private static void AddClient(long Idclient)
        {

            connection.Open();





            using (SqlCommand command = new SqlCommand($"INSERT INTO Client([ChatId],[Interval],[IsUSD],[IsEur],[ISRUB],[ISBTC],[CMessage]) VALUES ({Idclient},60000,1,1,1,1,'Hello')", connection))
            {
                try
                {

                    command.ExecuteNonQuery();
                    chats.Add(new Client(Idclient));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }


            connection.Close();

        }


        private static void UpdateClient(decimal interval, int IsUSD, int IsEUR, int IsRUB, int IsBTC,string word, long chatId)
        {

            connection.Open();

            using (SqlCommand command = new SqlCommand($"UPDATE [Client] SET [Interval]={interval},[IsUSD]={IsUSD},[IsEur]={IsEUR}," +
                $"[ISRUB]={IsRUB},[ISBTC]={IsBTC},[CMessage]='{word}' Where [ChatId]={chatId}", connection))
            {
                try
                {

                    command.ExecuteNonQuery();
                  
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            connection.Close();

        }

       

        private static void SendInf(object sender, ElapsedEventArgs e)
        {

            string URL = "https://api.privatbank.ua/p24api/pubinfo?exchange&coursid=5";
            XmlTextReader xmlread = new XmlTextReader(URL);
            xmlread.Read();
            List<string> mess = new List<string>();
            while (xmlread.Read())
            {
                if (xmlread.AttributeCount>3)
                {
                    mess.Add(xmlread.GetAttribute("ccy") + " " + xmlread.GetAttribute("base_ccy") + " Buy:" + xmlread.GetAttribute("buy") + " Sale:" + xmlread.GetAttribute("sale") + Environment.NewLine);                }
            }
            
           

            for (int k = 0; k < chats.Count; k++)
            {
                chats[k].PingInterval(Convert.ToDecimal(intervalping));
                if (chats[k].CurrentInterval == 0)
                {
                    Console.WriteLine(chats[k].ClientId);
                    string undermess = "";
                    if(chats[k].IsUSD==1)
                    {
                        undermess += mess[0];

                    }
                    if (chats[k].IsEUR == 1)
                    {
                        undermess += mess[1];
                    }
                    if (chats[k].IsRUB == 1)
                    {
                        undermess += mess[2];
                    }
                    if (chats[k].IsBTC == 1)
                    {
                        undermess += mess[3];
                    }


                    client.SendTextMessageAsync(chats[k].ClientId, undermess + Environment.NewLine + DateTime.Now);
                    chats[k].ResetI();
                }
            }
            Console.WriteLine("___________________________"+ DateTime.Now);


            if (DateTime.Now.Hour == 15 && DateTime.Now.Minute == 30)
            {
                ExcelEz();
                Console.WriteLine("ExcelDataSaved"+DateTime.Now);
               
            }
        
           
        }


     





        /// <summary>
        /// chech is exist by way
        /// </summary>
        /// <param name="path">Path to DataBase file</param>
        /// <returns></returns>

        /// <summary>
        /// create empty database file by path
        /// </summary>
        /// <param name="path">Path to DataBase file</param>

        /// <summary>
        /// event for inner message in bot from user
        /// </summary>
        /// <param name="sender">Same Bisness logik entity</param>
        /// <param name="e">Params of inner msg</param>
        /// 


        private static void getMsg(object sender, MessageEventArgs e)
        {
            Console.WriteLine($"{e.Message.Text}");
            if (!HaveHim(e.Message.Chat.Id))
            {
                
                AddClient(e.Message.Chat.Id);
            }

            
                e.Message.Text = e.Message.Text.Replace("\\/", "");
          



            switch (e.Message.Text)
            {
                case "/menu":
                    {
                        var somekey = new ReplyKeyboardMarkup(new[]
                        {
                            new KeyboardButton("Set Interval"),//AAO Kostil
                            new KeyboardButton("Set Valuts"),
                            new KeyboardButton("Set Word"),

                        });
                        client.SendTextMessageAsync(e.Message.Chat.Id, "Choose", replyMarkup: somekey);
                    }
                    break;
                case "Set Word":
                    {

                        client.SendTextMessageAsync(e.Message.Chat.Id, "Answer on THIS message by new word");
                    }
                    break;
                case "Set Interval":
                    {
                        var somekey = new ReplyKeyboardMarkup(new[]
                        {
                            new KeyboardButton("Set 1 minutes"),
                            new KeyboardButton("Set 30 minutes"),
                            new KeyboardButton("Set 24 hours")
                        });

                        client.SendTextMessageAsync(e.Message.Chat.Id, "Choose", replyMarkup: somekey);
                    }
                    break;
                case "Set Valuts":
                    {
                        SendValuts(e);
                    }
                    break;
                case "USD":
                    {
                        if(GetClient(e.Message.Chat.Id).IsUSD==1)
                        {
                            GetClient(e.Message.Chat.Id).IsUSD = 0;
                        }
                        else
                        {
                            Client tmp = GetClient(e.Message.Chat.Id);
                            tmp.IsUSD = 1;
                            UpdateClient(tmp.Interval, tmp.IsUSD, tmp.IsEUR, tmp.IsRUB, tmp.IsBTC, tmp.word, tmp.ClientId);
                        }
                        SendValuts(e);
                    }
                    break;
                case "EUR":
                    {

                        if (GetClient(e.Message.Chat.Id).IsEUR == 1)
                        {
                            GetClient(e.Message.Chat.Id).IsEUR = 0;
                        }
                        else
                        {
                            Client tmp = GetClient(e.Message.Chat.Id);
                            tmp.IsEUR = 1;
                            UpdateClient(tmp.Interval, tmp.IsUSD, tmp.IsEUR, tmp.IsRUB, tmp.IsBTC, tmp.word, tmp.ClientId);
                        }
                        SendValuts(e);
                    }
                    break;
                case "RUB":
                    {

                        if (GetClient(e.Message.Chat.Id).IsRUB == 1)
                        {
                            GetClient(e.Message.Chat.Id).IsRUB = 0;
                        }
                        else
                        {
                            Client tmp = GetClient(e.Message.Chat.Id);
                            tmp.IsRUB = 1;
                            UpdateClient(tmp.Interval, tmp.IsUSD, tmp.IsEUR, tmp.IsRUB, tmp.IsBTC, tmp.word, tmp.ClientId);
                        }
                        SendValuts(e);
                    }
                    break;
                case "BTC":
                    {

                        if (GetClient(e.Message.Chat.Id).IsBTC == 1)
                        {
                            GetClient(e.Message.Chat.Id).IsBTC = 0;
                        }
                        else
                        {
                            Client tmp = GetClient(e.Message.Chat.Id);
                            tmp.IsBTC  = 1;
                            UpdateClient(tmp.Interval, tmp.IsUSD, tmp.IsEUR, tmp.IsRUB, tmp.IsBTC, tmp.word, tmp.ClientId);
                        }
                        SendValuts(e);
                    }
                    break;
                case "Set 1 minutes":
                    {
                        Client tmp = GetClient(e.Message.Chat.Id);
                        UpdateClient(60000, tmp.IsUSD, tmp.IsEUR, tmp.IsRUB, tmp.IsBTC, tmp.word, tmp.ClientId);
                        ResetInterval(e.Message.Chat.Id, 60000);
                        client.SendTextMessageAsync(e.Message.Chat.Id, "I set interval of sending info at 1 minutes");
                    }
                    break;
                case "Set 30 minutes":
                    {
                        Client tmp = GetClient(e.Message.Chat.Id);
                        UpdateClient(30 * 60000, tmp.IsUSD, tmp.IsEUR, tmp.IsRUB, tmp.IsBTC, tmp.word, tmp.ClientId);
                        ResetInterval(e.Message.Chat.Id, 30 * 60000);
                        client.SendTextMessageAsync(e.Message.Chat.Id, "I set interval of sending info at 30 minutes");
                    }
                    break;
                case "Set 24 hours":
                    {
                        Client tmp = GetClient(e.Message.Chat.Id);
                        UpdateClient(24 * 60 * 60000, tmp.IsUSD, tmp.IsEUR, tmp.IsRUB, tmp.IsBTC, tmp.word, tmp.ClientId);
                        ResetInterval(e.Message.Chat.Id, 24 * 60 * 60000);
                        client.SendTextMessageAsync(e.Message.Chat.Id, "I set interval of sending info at 24 hours");
                    }
                    break;
                case "/help":
                    {
                        client.SendTextMessageAsync(e.Message.Chat.Id, "There all my commands: " + Environment.NewLine + "/menu- my settings" + Environment.NewLine + "/help- my commands"+ Environment.NewLine + "/get- get Excel report ");
                    }
                    break;
                case "/get":
                    {
                        var somekey = new ReplyKeyboardMarkup(new[]
                         {
                            new KeyboardButton("Get info for 1 day"),
                            new KeyboardButton("Get info for 5 days"),
                            new KeyboardButton("Get info for 1 week"),
                            new KeyboardButton("Get info for 2 weeks"),
                            new KeyboardButton("Get info for mounth")
                        });

                        client.SendTextMessageAsync(e.Message.Chat.Id, "Choose", replyMarkup: somekey);
                    }
                    break;
                case "/start":
                    {
                        client.SendTextMessageAsync(e.Message.Chat.Id, "There all my commands: " + Environment.NewLine + "/menu- my settings" + Environment.NewLine + "/help- my commands");
                    }
                    break;
                case "Get info for 1 day":
                    {
                        
                        GenerateExcelByDate(1);
                        FileStream fileStream = System.IO.File.Open("C:\\Users\\admnz\\source\\repos\\BotValuts\\BotVal\\BotVal\\bin\\Debug\\tmp.xlsx", FileMode.Open);

                        client.SendDocumentAsync(e.Message.Chat.Id, new InputOnlineFile(fileStream, "hello.xlsx"));
                        //fileStream.Close();

                        //System.IO.File.Delete("C:\\Users\\admnz\\source\\repos\\BotValuts\\BotVal\\BotVal\\bin\\Debug\\tmp.xlsx");

                    }
                    break;
                case "Get info for 5 days":
                    {
                        /*await*/ GenerateExcelByDate(5);
                        FileStream fileStream = System.IO.File.Open("C:\\Users\\admnz\\source\\repos\\BotValuts\\BotVal\\BotVal\\bin\\Debug\\tmp.xlsx", FileMode.Open);

                        client.SendDocumentAsync(e.Message.Chat.Id, new InputOnlineFile(fileStream, "hello.xlsx"));
                    }
                    break;
                case "Get info for 1 week":
                    {
                        GenerateExcelByDate(7);
                        FileStream fileStream = System.IO.File.Open("C:\\Users\\admnz\\source\\repos\\BotValuts\\BotVal\\BotVal\\bin\\Debug\\tmp.xlsx", FileMode.Open);

                        client.SendDocumentAsync(e.Message.Chat.Id, new InputOnlineFile(fileStream, "hello.xlsx"));
                    }
                    break;
                case "Get info for 2 weeks":
                    {
                        GenerateExcelByDate(14);
                        FileStream fileStream = System.IO.File.Open("C:\\Users\\admnz\\source\\repos\\BotValuts\\BotVal\\BotVal\\bin\\Debug\\tmp.xlsx", FileMode.Open);

                        client.SendDocumentAsync(e.Message.Chat.Id, new InputOnlineFile(fileStream, "hello.xlsx"));
                    }
                    break;
                case "Get info for mounth":
                    {
                        GenerateExcelByDate(30);
                        FileStream fileStream = System.IO.File.Open("C:\\Users\\admnz\\source\\repos\\BotValuts\\BotVal\\BotVal\\bin\\Debug\\tmp.xlsx", FileMode.Open);
                        
                            client.SendDocumentAsync(e.Message.Chat.Id, new InputOnlineFile(fileStream, "hello.xlsx"));
                        
                    }
                    break;
                default:
                    {

                        try
                        {
                            if (e.Message.ReplyToMessage.Text == "Answer on THIS message by new word")
                            {
                                Client tmp = GetClient(e.Message.Chat.Id);
                                UpdateClient(tmp.Interval, tmp.IsUSD, tmp.IsEUR, tmp.IsRUB, tmp.IsBTC, e.Message.Text, tmp.ClientId);
                                GetClient(e.Message.Chat.Id).word = e.Message.Text;
                            }
                            else
                            {
                                client.SendTextMessageAsync(e.Message.Chat.Id, GetClient(e.Message.Chat.Id).word);
                            }
                        }
                        catch (NullReferenceException ex)
                        {
                            client.SendTextMessageAsync(e.Message.Chat.Id, GetClient(e.Message.Chat.Id).word);
                        }
                    }
                    break;
            }


           

        }





        public static /*async Task<Task>*/ void GenerateExcelByDate(int daycount)
        {



            //try
            //{
                Application ex;
                Worksheet sheet;
                ex = new Application();
                ex.Visible = true;
                //Количество листов в рабочей книге
                //Добавить рабочую книгу
                Workbook workBook;
                string[,] nums2 = new string[daycount * 4, 5];
                if (System.IO.File.Exists("C:\\Users\\admnz\\source\\repos\\BotValuts\\BotVal\\BotVal\\bin\\Debug\\test.xlsx"))
                {
                    workBook = ex.Workbooks.Open("C:\\Users\\admnz\\source\\repos\\BotValuts\\BotVal\\BotVal\\bin\\Debug\\test.xlsx");
                    sheet = ex.Worksheets.get_Item(1);
                    int tmpi = 0;
                    int filed = 0;

                    for (int i = 1; i < sheet.Cells.Rows.Count; i++)
                    {

                        if (sheet.Cells[i, 1].Text == String.Format(""))
                        {
                            filed = i;
                            break;
                        }
                    }
                    int tmpk = filed - daycount * 4;
                    if (filed < daycount*4)
                    {
                        tmpk = 2;

                    }


                    for (int i = tmpk; i < filed; i++)
                    {

                        for (int j = 1; j <= 5; j++)
                        {
                            nums2[tmpi, j - 1] = sheet.Cells[i, j].Text;

                        }
                        tmpi++;


                    }


                    workBook.Close();
                    ex.Quit();







                    ex = new Application();
                    workBook = ex.Workbooks.Add(Type.Missing);
                    sheet = ex.Worksheets.get_Item(1);



                    ex.SheetsInNewWorkbook = 1;
                    workBook = ex.Workbooks.Add(Type.Missing);
                    sheet = ex.Worksheets.get_Item(1);
                    sheet.Activate();
                    ex.DisplayAlerts = false;
                    //Получаем первый лист документа (счет начинается с 1)
                    //Название листа (вкладки снизу)
                    sheet.Name = "Info";
                    sheet.Cells[1, 1] = String.Format("First Valut");
                    sheet.Cells[1, 2] = String.Format("Second Valut");
                    sheet.Cells[1, 3] = String.Format("SELL");
                    sheet.Cells[1, 4] = String.Format("BUY");
                    sheet.Cells[1, 5] = String.Format("Date");
                    tmpk = daycount * 4 + 1;
                    if (filed < daycount*4)
                    {
                        tmpk = filed;
                    }
                    for (int i = 2; i < tmpk; i++)
                    {
                        for (int j = 1; j <= 5; j++)
                        {

                            sheet.Cells[i, j] = String.Format(nums2[i - 2, j - 1]);
                        }
                    }

                    workBook.SaveAs("C:\\Users\\admnz\\source\\repos\\BotValuts\\BotVal\\BotVal\\bin\\Debug\\tmp.xlsx");
                    workBook.Close();
                    ex.Quit();


                }
                else
                {
                    ExcelEz();
                    GenerateExcelByDate(daycount);
                }






            //return new Task(new System.Action(()=>{ Console.WriteLine("async"); }));







            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}


        }





        static public void SendValuts(MessageEventArgs e)
        {
            Client tmp = GetClient(e.Message.Chat.Id);
            var somekey = new ReplyKeyboardMarkup(new[]
            {
                            new KeyboardButton("USD"+((tmp.IsUSD==1)?"\\/":"")),
                            new KeyboardButton("EUR"+((tmp.IsEUR==1)?"\\/":"")),
                            new KeyboardButton("RUB"+((tmp.IsRUB==1)?"\\/":"")),
                            new KeyboardButton("BTC"+((tmp.IsBTC==1)?"\\/":""))
                        });

            client.SendTextMessageAsync(e.Message.Chat.Id, "Choose", replyMarkup: somekey);
        }

        static public void ResetInterval(long ChatId,decimal interval)
        {
            for(int h=0;h<chats.Count;h++)
            {
                if(chats[h].ClientId==ChatId)
                {
                    chats[h].ChangeInterval(interval);
                    break;
                }
            }
        }

        public static Client GetClient(long chatId)
        {
            for (int k = 0; k < chats.Count; k++)
            {

                if (chats[k].ClientId == chatId)
                {

                    return chats[k];
                }
            }
            return null;
        }

        public static bool HaveHim(long nwe)
        {
            for (int k = 0; k < chats.Count; k++)
            {
                
                if (chats[k].ClientId == nwe)
                {
                    
                    return true;
                }
            }
            return false;
        }







        static public void ExcelEz()
        {
            Application ex;
            Worksheet sheet;
            ex = new Application();

            ex.Visible = true;

            //Количество листов в рабочей книге
            //Добавить рабочую книгу
            Workbook workBook;




            string URL = "https://api.privatbank.ua/p24api/pubinfo?exchange&coursid=5";
            XmlTextReader xmlread = new XmlTextReader(URL);

            string[,] nums2 = { { "", "", "", "", DateTime.Now.ToString() }, { "", "", "", "", DateTime.Now.ToString() }, { "", "", "", "", DateTime.Now.ToString() }, { "", "", "", "", DateTime.Now.ToString() } };
            int k = 0;
            while (xmlread.Read())
            {

                if (xmlread.AttributeCount > 3)
                {
                    nums2[k, 0] = xmlread.GetAttribute("ccy");
                    nums2[k, 1] = xmlread.GetAttribute("base_ccy");
                    nums2[k, 2] = xmlread.GetAttribute("buy");
                    nums2[k, 3] = xmlread.GetAttribute("sale");
                    k++;
                }
            }


            if (System.IO.File.Exists("C:\\Users\\admnz\\source\\repos\\BotValuts\\BotVal\\BotVal\\bin\\Debug\\test.xlsx"))
            {
                workBook = ex.Workbooks.Open("C:\\Users\\admnz\\source\\repos\\BotValuts\\BotVal\\BotVal\\bin\\Debug\\test.xlsx");
                sheet = ex.Worksheets.get_Item(1);
                int tmpi = 0;
                int filed = 0;

                for (int i = 1; i < sheet.Cells.Rows.Count; i++)
                {

                    if (sheet.Cells[i, 1].Text == String.Format(""))
                    {
                        filed = i;
                        break;
                    }
                }


                for (int i = filed; i <= filed + 3; i++)
                {

                    for (int j = 1; j <= 5; j++)
                    {
                      
                        string tm = String.Format(nums2[tmpi, j - 1]);
                        Console.WriteLine();
                        sheet.Cells[i, j] = tm;
                    }
                    tmpi++;
                
                }

                workBook.Save();
                workBook.Close();
                ex.Quit();

            }
            else
            {

                Console.WriteLine("This is NEW");

                ex.SheetsInNewWorkbook = 1;
                workBook = ex.Workbooks.Add(Type.Missing);
                sheet = ex.Worksheets.get_Item(1);
                sheet.Activate();
                ex.DisplayAlerts = false;
                //Получаем первый лист документа (счет начинается с 1)
                //Название листа (вкладки снизу)
                sheet.Name = "Отчет за 13.12.2017";
                sheet.Cells[1, 1] = String.Format("First Valut");
                sheet.Cells[1, 2] = String.Format("Second Valut");
                sheet.Cells[1, 3] = String.Format("SELL");
                sheet.Cells[1, 4] = String.Format("BUY");
                sheet.Cells[1, 5] = String.Format("Date");

                for (int i = 2; i <= 5; i++)
                {
                    for (int j = 1; j <= 5; j++)
                    {
                        
                        sheet.Cells[i, j] = String.Format(nums2[i - 2, j - 1]);
                    }
                }

                workBook.SaveAs("C:\\Users\\admnz\\source\\repos\\BotValuts\\BotVal\\BotVal\\bin\\Debug\\test.xlsx");
                workBook.Close();
                ex.Quit();
            }


        }
        /// <summary>
        /// Add question in local data base file
        /// </summary>
        /// <param name="question">from user</param>
        /// <param name="path_to_db"> of information</param>

    }
}










