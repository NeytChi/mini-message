using MySql.Data.MySqlClient;

namespace Common.NDatabase
{
    public abstract class Storage
    {
        public MySqlConnectionStringBuilder connectionstring;
        public string table_name;
        public string table;

        public void SetConnection(ref MySqlConnectionStringBuilder connectionstring)
        {
            this.connectionstring = connectionstring;
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
