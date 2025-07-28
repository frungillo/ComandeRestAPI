using System.Data.SqlClient;
using System.Data;



namespace ComandeRestAPI.Classi
{
    class db : IDisposable
    {
        //public static string DataSource = "192.168.0.225,1433"; //"185.25.232.65,1433"; 
        public static string DataSource = "2.197.115.31,21433";
        //public static string DataSource = "carbolandia.tplinkdns.com";
        public static string DBName = "carbolandia";
        //public static string connStr() { return "Data Source=79.9.136.241:11433;Initial Catalog=comandeweb;Persist Security Info=True;User ID=jmk;Password=napoli.081"; }
        public static string connStr() { return $@"Data Source={DataSource};Initial Catalog={DBName};Persist Security Info=True;User ID=sa;Password=avellino.081;Max Pool Size=300;"; }
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
        public int exe(string sql)
        {
            try
            {
                SqlCommand c = new SqlCommand(sql, _conn);
                return c.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

                throw new Exception("Errore Update/insert/delete:" + ex.Message, ex);
            }

        }
        public async Task<SqlDataReader> getReaderAsync(string sql)
        {
            SqlCommand c = new SqlCommand(sql, _conn);

            _r = await c.ExecuteReaderAsync();
            return _r;
        }

    }
    public static class SqlDataReaderExstensions
    {
        public static DateTime? GetNullableDateTime(this SqlDataReader reader, int col)
        {
            // var col = reader.getordin();
            return reader.IsDBNull(col) ?
                        (DateTime?)null :
                        (DateTime?)reader.GetDateTime(col);
        }
    }


   
    public class DB_Index : Attribute
    {

    }

    public class DB_Exclude : Attribute
    {

    }
    public class DB_LongTime : Attribute { }
}
