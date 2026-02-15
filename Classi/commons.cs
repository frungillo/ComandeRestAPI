using System.Data.SqlClient;

namespace ComandeRestAPI.Classi
{
    public class commons
    {
        public static void setLogMessage(string id_operatore, string msg)
        {
            string sql = $@"
                            INSERT INTO [dbo].[log_eventi]
                                       ([data]
                                       ,[id_operatore]
                                       ,[evento]
                                       ,[note])
                                 VALUES
                                       (convert(datetime,'{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString().Replace(".", ":")}', 103)
                                       ,'{id_operatore}'
                                       ,'{msg.Replace("'", "''")}'
                                       ,'ness')
                            ";
            db db = new db();
            db.exe(sql);
            db.Dispose();
        }
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
            if (chiave=="ora_arrivo_cena" || chiave=="ora_fine_cena") valore=valore.Substring(0, 2); // prendo solo l'ora, non mi serve il minuto
            return valore;
        }
    }
}
