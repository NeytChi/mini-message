using System;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Common.NDatabase
{
    public abstract class Storage
    {
        public MySqlConnection connection;
        public Semaphore s_locker;
        public string table_name;
        public string table;

        public void SetConnection(ref MySqlConnection connection)
        {
            this.connection = connection;
        }
        public void SetTableName(string table_name)
        {
            this.table_name = table_name;
        }
        public void SetTable(string table)
        {
            this.table = table;
        }
        public string GetTableName()
        {
            return table_name;
        }
        public string GetTable()
        {
            return table;
        }
    }
}
