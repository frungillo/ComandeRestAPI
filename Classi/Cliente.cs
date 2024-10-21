using System.Data.SqlClient;
using System.Net.Mail;
using System.Xml.Linq;
using System;

namespace ComandeRestAPI.Classi
{
    [Serializable]
    public class Cliente
    {
        private int _id_cliente;
        private string _nome;
        private string _cognome;
        private string _telefono;
        private string? _email;
        private DateTime? _data_reg;
        private int _attivo;
        private string _note;
        private string _sesso;
        private DateTime? _data_nascita;
        private string _password;

        public int Id_cliente { get => _id_cliente; set => _id_cliente = value; }
        public string Nome { get => _nome; set => _nome = value; }
        public string Cognome { get => _cognome; set => _cognome = value; }
        public string Telefono { get => _telefono; set => _telefono = value; }
        public string? Email { get => _email; set => _email = value; }
        public DateTime? Data_reg { get => _data_reg; set => _data_reg = value; }
        public int Attivo { get => _attivo; set => _attivo = value; }
        public string Note { get => _note; set => _note = value; }
        public string Sesso { get => _sesso; set => _sesso = value; }
        public DateTime? Data_nascita { get => _data_nascita; set => _data_nascita = value; }
        public string Password { get => _password; set => _password = value; }
        public Cliente() { }
        public Cliente(int ID)
        {
            db db = new db();
            SqlDataReader r = db.getReader($"select * from clienti where id_cliente={ID}");
            if (r.HasRows)
            {
                r.Read();
                _id_cliente = (int)r[0];
                _nome = r[1].ToString();
                _cognome = r[2].ToString();
                _telefono = r[3].ToString();
                _email = r[4].ToString();
                _data_reg = r.GetNullableDateTime(5);
                _attivo = (int)r[6];
                _note = r[7].ToString();
                _sesso = r[8].ToString();
                _data_nascita = r.GetNullableDateTime(9);
                _password = r[10].ToString();

            }
            else
            {
                throw new Exception("Nessun Cliente con questo ID");
            }
            db.Dispose();

        }
        public static List<Cliente> GetAllCliente(string filter = "")
        {
            List<Cliente> lst = new List<Cliente>();
            db db = new db();
            string sql = $"select id_cliente from clienti {filter} order by cognome, nome";
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                Cliente c = new Cliente((int)r[0]);
                lst.Add(c);
            }
            db.Dispose();
            return lst;
        }
        public static List<int> GetIdCliente(string filter = "")
        {
            List<int> lstID = new List<int>();
            db db = new db();
            string sql = $@" select c.id_cliente, c.age, c.sesso from (
                             select  DATEDIFF(YY,data_nascita,GETDATE()) as age , id_cliente, sesso
                             from clienti
                             where data_nascita !='' ) c
                             {filter} ";
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                int id = (int)r[0];
                lstID.Add(id);
            }
            db.Dispose();
            return lstID;
        }
        public static string[] SuggestTel(string number_partial)
        {
            string[] elenco = new string[] { };
            List<string> elenc = new List<string>();
            db db = new db();
            string sql = $"select distinct telefono from clienti where telefono like '{number_partial}%' order by telefono";
            SqlDataReader r = db.getReader(sql);

            while (r.Read())
            {

                elenc.Add(r[0].ToString());
                GC.Collect();
            }
            db.Dispose();
            elenco = elenc.ToArray();
            return elenco;

        }
        public static int checkMail(string mail)
        {
            //ritorna l'id_cliente se la mail già esiste in tabella Clienti -----Replace("'", "''")
            var db = new db();
            string sql = $@"select id_cliente from clienti where email='{mail}'";
            SqlDataReader r = db.getReader(sql);
            if (r.HasRows)
            {
                r.Read();
                int id = (int)r[0];
                db.Dispose();
                return id;

            }
            db.Dispose();
            return 0;
        }
        public static int contaDaApp()
        {
            //ritorna il numero di clienti che si sono registrati da App secondo la stringa "Registrato da App'
            var db = new db();
            string sql = $@"select count(id_cliente) from clienti where note like '%Registrato da App%' and attivo=1";
            SqlDataReader r = db.getReader(sql);
            if (r.HasRows)
            {
                r.Read();
                int id = (int)r[0];
                db.Dispose();
                return id;

            }
            db.Dispose();
            return 0;
        }
        public static int countTavolate(int idc)
        {
            var db = new db();
            string sql = $@"select Count(*) from tavolata where id_cliente={idc}";
            SqlDataReader r = db.getReader(sql);
            if (r.HasRows)
            {
                r.Read();
                int id = (int)r[0];
                db.Dispose();
                return id;

            }
            db.Dispose();
            return 0;

        }
        public static string checkTelefono(string telefono)
        {
            //ritorna il nome e cognome se il telefono già esiste in tabella Clienti -----Replace("'", "''")
            var db = new db();
            string sql = $@"select id_cliente from clienti where telefono='{telefono}'";
            SqlDataReader r = db.getReader(sql);
            if (r.HasRows)
            {
                r.Read();
                int id = (int)r[0];
                db.Dispose();
                return id.ToString();

            }
            db.Dispose();
            return "NON TROVATO";

        }
        public static void deleteCliente(int idc)
        {
            var db = new db();
            string sql = $@"delete from clienti where id_cliente={idc}";
            SqlDataReader r = db.getReader(sql);

            db.Dispose();
            return;

        }
        public static int insert(Cliente c)
        {
            db db = new db();
            string data = $"convert(datetime,'{c.Data_nascita?.ToShortDateString()}',103)";
            if (c.Data_nascita == null) data = "null";
            string data2 = $"convert(datetime,'{c.Data_reg?.ToShortDateString()}',103)";
            if (c.Data_reg == null) data2 = "null";
            if (c.Email.Length == 0) c.Email="";

            string sql = $@"insert into clienti 
             values('{c.Nome.ToUpper().Replace(",", ".")}',
                     '{c.Cognome.ToUpper().Replace(",", ".")}',
                     '{c.Telefono.Trim()}',
                     '{c.Email.Trim()}',
                     {data2},
                     {c.Attivo},
                    '{c.Note.Replace("'", "''")}',
                    '{c.Sesso}', 
                     {data},
                     '{c.Password}'
                      ) SELECT SCOPE_IDENTITY()";

            try
            {
                //db.exe(sql);

                SqlDataReader r = db.getReader(sql);
                r.Read();
                int index = int.Parse(r[0].ToString());
                db.Dispose();
                //await SetGoogleContact(c.Nome, c.Cognome, c.Email, c.Telefono);
                return index;

            }
            catch (Exception ex)
            {

                throw new Exception("Errore Salvataggio Cliente:" + ex.Message);
            }
        }
        public static void update(Cliente c)
        {
            db db = new db();
            string data = $"convert(datetime,'{c.Data_nascita?.ToShortDateString()}',103)";
            if (c.Data_nascita == null) data = "null";
            string data2 = $"convert(datetime,'{c.Data_reg?.ToShortDateString()}',103)";
            if (c.Data_reg == null) data2 = "null";

            string sql = $@"update clienti set  
                      nome='{c.Nome.Replace(",", ".")}',
                     cognome='{c.Cognome.Replace(",", ".")}',
                     telefono='{c.Telefono.Trim()}',
                     email='{c.Email.Trim()}',
                     data_reg={data2},
                     attivo={c.Attivo},
                     note='{c.Note.Replace("'", "''")}',
                     sesso='{c.Sesso.Replace(",", ".")}',
                     data_nascita={data},
                     password='{c.Password}'
                     where id_cliente={c.Id_cliente}";
            try
            {
                db.exe(sql);
                db.CloseReader();
                db.Dispose();

            }
            catch (Exception ex)
            {

                throw new Exception("Errore Salvataggio Cliente:" + ex.Message);
            }
        }

       



        public static List<TavoliStorico> getStoricoTavoli(int id_cliente)
        {
            List<TavoliStorico> list = new List<TavoliStorico>();

            var db = new db();
            string sql = $@"select 
                    convert(date, T.data_ora_arrivo, 103) as Data, 
	                 CASE 
                        WHEN convert(time, T.data_ora_arrivo) = '12:00:00' THEN 'PRANZO'
                        WHEN convert(time, T.data_ora_arrivo) = '19:00:00' THEN 'CENA'
                        ELSE 'ALTRO'
                    END as Pasto,
                    T.Adulti, 
                    T.Bambini, 
                    S.Descrizione as Stato, 
                    T.Conto, 
                    T.Sconto
   
                from 
                    tavolata T 
                join 
                    stato S 
                    on T.stato = S.id_stato
                where 
                    id_cliente = {id_cliente};";
            SqlDataReader r = db.getReader(sql);
    
            while (r.Read())
            {
               TavoliStorico t= new TavoliStorico();
                t.Data = r.GetDateTime(0);
                t.Pasto = r.GetString(1);
                t.Adulti=r.GetInt32(2);
                t.Bambini=r.GetInt32(3);
                t.Stato=r.GetString(4);
                t.Conto=r.GetDecimal(5);
                t.Sconto=r.GetDecimal(6);
                
                list.Add(t);
            }
            db.Dispose();
            return list;
           
        }
    }

}
