using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace OtzariaTestApp
{
    public class SqliteDataBase : IDisposable
    {
        public SQLiteConnection connection;

        public SqliteDataBase()
        {
            connection = new SQLiteConnection("Data Source=database.sqlite3");
            if (!File.Exists("./database.sqlite3"))
            {
                SQLiteConnection.CreateFile("database.sqlite3");
                Console.WriteLine("DataBase File Created");
            }

            OpenConnection();
        }

        public void Dispose() => CloseConnection();

        public void OpenConnection()
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();

                //new SQLiteCommand("begin", connection).ExecuteNonQuery();
            }              
        }

        public void CloseConnection()
        {
            if (connection.State != ConnectionState.Closed)
            {
                //new SQLiteCommand("end", connection).ExecuteNonQuery();
                connection.Close();
            }   
        }

        public void ExecuteQuery(string query)
        {
            using (var command = new SQLiteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
