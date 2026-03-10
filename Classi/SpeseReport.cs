using System.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ComandeRestAPI.Classi
{
    public class SpeseReport
    {
        private DateTime data_ora_registrazione;
        private string note;
        private double pos;
        private double contanti;

        public DateTime Data_ora_registrazione { get => data_ora_registrazione; set => data_ora_registrazione = value; }
        public string Note { get => note; set => note = value; }
        public double Pos { get => pos; set => pos = value; }
        public double Contanti { get => contanti; set => contanti = value; }

        public SpeseReport() { }
        public static List<SpeseReport> getSpeseReportFiltro(string filtro) 
        {
            db db = new db();
            string sql = $@"select data_ora_registrazione, note,  conto_pos as POS, conto_contanti as contanti from pagamenti where {filtro} ";
            List<SpeseReport> lst = new List<SpeseReport>();
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                SpeseReport sp = new SpeseReport();
                sp.Data_ora_registrazione = Convert.ToDateTime(r["data_ora_registrazione"].ToString());
                sp.Note = r["note"].ToString();
                sp.Pos = Convert.ToDouble(r["POS"].ToString());
                sp.Contanti = Convert.ToDouble(r["contanti"].ToString());
                lst.Add(sp);
            }
            db.Dispose();
            return lst;
        }
    }
}
