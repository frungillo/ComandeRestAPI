using System.Data.SqlClient;

namespace ComandeRestAPI.Classi
{
    public class commons
    {

        public static string recuperoParametri(string chiave)
        {
            // recupera i parametri di timing dalla tabella parametri_WA

            string sql = $@"select valore from parametri_wa  where chiave='{chiave}' ";
            string valore = "";
            db db = new db();
            SqlDataReader r = db.getReader(sql);
            if (r.HasRows)
            {
                r.Read();
                valore = r[0].ToString();

            }
            db.Dispose();
            return valore;
        }
    }
}
