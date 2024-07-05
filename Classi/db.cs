using System.Data.SqlClient;
using System.Data;

namespace ComandeRestAPI.Classi
{
    class db : IDisposable
    {
        public static string DataSource = "79.9.245.79,1433"; 
        public static string DBName = "carbolandia";
        //public static string connStr() { return "Data Source=79.9.136.241:11433;Initial Catalog=comandeweb;Persist Security Info=True;User ID=jmk;Password=napoli.081"; }
        public static string connStr() { return $@"Data Source={DataSource};Initial Catalog={DBName};Persist Security Info=True;User ID=sa;Password=avellino.081"; }
        private SqlConnection _conn;
        private SqlDataReader _r;

        public db()

        {
            _conn = new SqlConnection(db.connStr());
            try
            {
                _conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Errore Ist DB:" + ex.Message);
            }
        }

        public void Dispose()
        {
            _conn.Close();
        }

        public SqlDataReader getReader(string sql)
        {
            SqlCommand c = new SqlCommand(sql, _conn);
            _r = c.ExecuteReader();
            return _r;
        }

        public DataTable getDataTable(string sql)
        {
            SqlDataAdapter ta = new SqlDataAdapter(sql, _conn);
            DataTable dt = new DataTable();
            ta.Fill(dt);
            return dt;
        }

        public void CloseReader()
        {
            _r.Close();
        }


        public int Esegui(string sql)
        {
            SqlCommand c = new SqlCommand(sql, _conn);
            return c.ExecuteNonQuery();
        }


    }
}
