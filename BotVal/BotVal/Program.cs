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

namespace BotVal
{
    class Program
    {
        static SqlConnection connection = new SqlConnection("Server=tcp:azurez.database.windows.net,1433;Initial Catalog=Zakha_db;Persist Security Info=False;User ID=admnz;Password=a73IR00l;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        static TelegramBotClient client;
        static List<Client> chats = new List<Client>();

        static void Main(string[] args)
        {


            GetUsers();



            client = new TelegramBotClient("1322903188:AAHNRgjAopnGninojF8XWd8xtUO3EmjswIg");
            client.OnMessage += getMsg;
            client.StartReceiving();



            Timer task = new Timer(10000);
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
                        Console.WriteLine(reader.GetInt32(1)+" "+reader.GetDecimal(2));
                        chats.Add(new Client(reader.GetInt32(1), reader.GetDecimal(2)));
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


        private static void SendInf(object sender, ElapsedEventArgs e)
        {

            string URL = "https://api.privatbank.ua/p24api/pubinfo?exchange&coursid=5";
            XmlTextReader xmlread = new XmlTextReader(URL);
            xmlread.Read();
            string mess = "";
            while (xmlread.Read())
            {
                if (xmlread.AttributeCount>3)
                {
                    mess += xmlread.GetAttribute("ccy") + " " + xmlread.GetAttribute("base_ccy") + " Buy:" + xmlread.GetAttribute("buy") + " Sale:" + xmlread.GetAttribute("sale") + Environment.NewLine;
                }
            }
            for (int k = 0; k < chats.Count; k++)
            {
                chats[k].PingInterval();
                if (chats[k].CurrentInterval == 0)
                {
                    Console.WriteLine(chats[k].ClientId);
                    client.SendTextMessageAsync(chats[k].ClientId, mess + Environment.NewLine + DateTime.Now);
                    chats[k].ResetI();
                }
            }
            Console.WriteLine("___________________________");





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
                case "Set 1 minutes":
                    {
                        ResetInterval(e.Message.Chat.Id,60000);
                        client.SendTextMessageAsync(e.Message.Chat.Id, "I set interval of sending info at 1 minutes");
                    }
                    break;
                case "Set 30 minutes":
                    {
                        ResetInterval(e.Message.Chat.Id, 30*60000);
                        client.SendTextMessageAsync(e.Message.Chat.Id, "I set interval of sending info at 30 minutes");
                    }
                    break;
                case "Set 24 hours":
                    {
                        ResetInterval(e.Message.Chat.Id, 24*60*60000);
                        client.SendTextMessageAsync(e.Message.Chat.Id, "I set interval of sending info at 24 hours");
                    }
                    break;
                case "/help":
                    {
                        client.SendTextMessageAsync(e.Message.Chat.Id, "There all my commands: " + Environment.NewLine + "/menu- my settings" + Environment.NewLine + "/help- my commands");
                    }
                    break;
                case "/start":
                    {
                        client.SendTextMessageAsync(e.Message.Chat.Id, "There all my commands: " + Environment.NewLine + "/menu- my settings" + Environment.NewLine + "/help- my commands");
                    }
                    break;
                default:
                    {
                        client.SendTextMessageAsync(e.Message.Chat.Id, "Hello!");
                    }
                    break;
            }



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
        /// <summary>
        /// Add question in local data base file
        /// </summary>
        /// <param name="question">from user</param>
        /// <param name="path_to_db"> of information</param>

    }
}





