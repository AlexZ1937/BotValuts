using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using System.Timers;
using System.Collections.Generic;
using System.Xml;
using System.Data.SqlClient;

namespace TeleBot
{
    class Program
    {
        static SqlConnection connection = new SqlConnection("Server=tcp:azurez.database.windows.net,1433;Initial Catalog=Zakha_db;Persist Security Info=False;User ID=admnz;Password=a73IR001;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        static TelegramBotClient client;
        static List<long> chats = new List<long>();

        static void Main(string[] args)
        {


            GetUsers();



            client = new TelegramBotClient("1322903188:AAHNRgjAopnGninojF8XWd8xtUO3EmjswIg");
            client.OnMessage += getMsg;
            client.StartReceiving();



            Timer task = new Timer(1800000);
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
                        chats.Add(reader.GetInt32(1));
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





            using (SqlCommand command = new SqlCommand($"INSERT INTO Client([ChatId],[Interval],[IsUSD],[IsEur],[ISRUB],[ISBTC],[CMessage]) VALUES ({Idclient},60000,true,true,true,true,'Hello')", connection))
            {
                try
                {

                    command.ExecuteNonQuery();
                    chats.Add(Idclient);
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
                if (xmlread.GetAttribute("buy").ToString().Length > 0)
                {
                    mess += xmlread.GetAttribute("ccy") + " " + xmlread.GetAttribute("base_ccy") + " Buy:" + xmlread.GetAttribute("buy") + " Sale:" + xmlread.GetAttribute("sale") + Environment.NewLine;
                }
            }
            for (int k = 0; k < chats.Count; k++)
            {
                Console.WriteLine(chats[k]);
                client.SendTextMessageAsync(chats[k], mess + Environment.NewLine + DateTime.Now);
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
                            new KeyboardButton("Set Interval"),
                            new KeyboardButton("Set Valuts"),
                            new KeyboardButton("Set Word")
                        });
                        client.SendTextMessageAsync(e.Message.Chat.Id, "Choose", replyMarkup: somekey);
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

        public static bool HaveHim(long nwe)
        {
            for (int k = 0; k < chats.Count; k++)
            {
                if (chats[k] == nwe)
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





