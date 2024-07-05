using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace ComandeRestAPI.Classi
{
    public class logEventi
    {
        private int _idUtente = 999;

        public logEventi()
        {
        }

        public logEventi(int idutente)
        {
            _idUtente = idutente;
        }

        /// <summary>
        /// Scrive una riga del log
        /// </summary>
        /// <param name="msg">Messaggio da scrivere nel log</param>
        /// <param name="note">Note da inserire per il messaggio [nessuna] è predefinito</param>
        public void Scrivi(string msg, string note = "nessuna")
        {
            db db = new db();
            string sql = $@"INSERT INTO [dbo].[log_eventi]
                                       ([data]
                                       ,[id_operatore]
                                       ,[evento]
                                       ,[note])
                                 VALUES
                                       (convert(datetime,'{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}', 103)
                                       ,{_idUtente}
                                       ,'{msg.Replace("'", "''")}'
                                       ,'{note.Replace("'", "''")}')";
            db.Esegui(sql);
            db.Dispose();

        }
    }

    public class TavolataMini
    {
        public int NumeroTavolo { get; set; }
        public int IdOperatore { get; set; }
        public string Descrizione { get; set; }
        public string Note { get; set; }
        public int Adulti { get; set; }
        public int Bambini { get; set; }
        public int IdSala { get; set; }

        // Costruttore
        public TavolataMini() { }

        
    }


    [Serializable]
    public class Tavolata
    {
        private int _id;
        private DateTime _dataOraArrivo;
        private DateTime _dataOraConto;
        private decimal _acconto;
        private int _id_stato;
        private Stato _stato;
        private int _id_operatore;
        private Operatori _operatore;
        private int _adulti;
        private int _bambini;
        private int _id_sala;
        private Sala _sala;
        private string _descrizione;
        private string _note;
        private decimal _sconto;
        private int _id_cliente;
        private decimal _preconto;
        private decimal _conto;
        private string _item;
        private int _numero_tavolo;
      
        public int Id { get => _id; set => _id = value; }
        public DateTime DataOraArrivo { get => _dataOraArrivo; set => _dataOraArrivo = value; }
        public DateTime DataOraConto { get => _dataOraConto; set => _dataOraConto = value; }
        public decimal Acconto { get => _acconto; set => _acconto = value; }
        public int Id_stato { get => _id_stato; set => _id_stato = value; }
        public Stato Stato { get => _stato; set => _stato = value; }
        public Operatori Operatore { get => _operatore; set => _operatore = value; }
        [JsonProperty("adulti")]
        public int Adulti { get => _adulti; set => _adulti = value; }
        [JsonProperty("bambini")]
        public int Bambini { get => _bambini; set => _bambini = value; }
        
        [JsonProperty("id_sala")]
        public int Id_sala { get => _id_sala; set => _id_sala = value; }
        public Sala Sala { get => _sala; set => _sala = value; }
        [JsonProperty("descrizione")]
        public string Descrizione { get => _descrizione; set => _descrizione = value; }
        [JsonProperty("note")]
        public string Note { get => _note; set => _note = value; }
        public decimal Sconto { get => _sconto; set => _sconto = value; }
        [JsonProperty("numero_tavolo")]
        public int Numero_tavolo { get => _numero_tavolo; set => _numero_tavolo = value; }
        public int Id_cliente { get => _id_cliente; set => _id_cliente = value; }
        public decimal Preconto { get => _preconto; set => _preconto = value; }
        public decimal Conto { get => _conto; set => _conto = value; }
        public string Item { get => _item; set => _item = value; }
        [JsonProperty("id_operatore")]
        public int Id_operatore { get => _id_operatore; set => _id_operatore = value; }

        public Tavolata()  { }

        public Tavolata(int id)
        {
            using (db db = new db())
            {
                SqlDataReader r = db.getReader($"select * from tavolata where id_tavolata={id}");
                if (r.HasRows)
                {
                    r.Read();
                    _id = (int)r[0];
                    _dataOraArrivo = (DateTime)r[1];
                    try
                    {
                        _dataOraConto = (DateTime)r[2];
                    }
                    catch { }
                    _acconto = decimal.Parse(r[3].ToString());
                    _id_stato = r.GetInt32(4);
                    Stato stato = new Stato(r.GetInt32(4));
                    try
                    {
                        Operatori user = new Operatori(r.GetInt32(5));
                        _operatore = user;
                    } catch { }
                    
                    _stato= stato;
                    
                    _adulti = (int)r[6];
                    _bambini = int.Parse(r[7].ToString());
                    _id_sala = r.GetInt32(8);
                    _sala = Sala.getSalaByID((int)r[8]);
                    _descrizione = r[9].ToString();
                    _note = r[10].ToString();
                    try{_sconto = decimal.Parse(r[11].ToString());} catch { }
                    _id_cliente = (int)r[12];
                    try { _preconto = decimal.Parse(r[13].ToString());} catch { }
                    try{ _sconto = decimal.Parse(r[14].ToString());}  catch { }
                    _item= r[15].ToString();
                    try { _numero_tavolo = (int)r[16]; } catch { }
                }
                else
                {

                    throw new Exception("Nessuna tavolata con questo ID");
                }
            }

        }



        public static List<Tavolata> GetTavolateByIdOperatore(int id_operatore)
        {
            List<Tavolata> t = new List<Tavolata>();
            // string endora = "";
            string ora = "";

            if (DateTime.Now.Hour >= 9 && DateTime.Now.Hour < 17) ora = "12:00"; else ora = "19:00";
            /*
            if (ora == "12:00") endora = $"convert(datetime, '{DateTime.Now.ToShortDateString()} 18:30' , 103)";
                else endora = $"dateadd(day, 1, convert(datetime, '{DateTime.Now.ToShortDateString()} 04:00', 103))";
            if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour < 6)
                endora = $"dateadd(day, 1, convert(datetime, '{DateTime.Now.ToShortDateString()} 04:00', 103))";
                */
            string sqlOra12 = $" and Datepart(HOUR, data_ora_arrivo) =12";
            string sqlOra19 = $" and Datepart(HOUR, data_ora_arrivo) =19";
            db db = new db(); //SYSDATETIME()
            string sql = $@"SELECT * from tavolata where CONVERT(VARCHAR(10),GETDATE(),103) = CONVERT(VARCHAR(10),data_ora_arrivo,103) and id_operatore = '{id_operatore}' ";

            if (ora.Contains("12")) sql += sqlOra12; else sql += sqlOra19;
            sql += "  order by descrizione";
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                t.Add(new Tavolata((int)r[0]));
            }
            db.Dispose();
            return t;

        }
        public static List<Tavolata> GetTavolateOdierne()
        {
            List<Tavolata> t = new List<Tavolata>();
            // string endora = "";
            string ora = "";

            if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 17) ora = "12:00"; else ora = "19:00";
            /*
            if (ora == "12:00") endora = $"convert(datetime, '{DateTime.Now.ToShortDateString()} 18:30' , 103)";
                else endora = $"dateadd(day, 1, convert(datetime, '{DateTime.Now.ToShortDateString()} 04:00', 103))";
            if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour < 6)
                endora = $"dateadd(day, 1, convert(datetime, '{DateTime.Now.ToShortDateString()} 04:00', 103))";
                */
            string sqlOra12 = $" and Datepart(HOUR, data_ora_arrivo) =12";
            string sqlOra19 = $" and Datepart(HOUR, data_ora_arrivo) =19";
            db db = new db(); //SYSDATETIME()
            string sql = $@"SELECT * from tavolata where  GETDATE() between DATEADD(hour,-9, data_ora_arrivo) and DATEADD(hour,9, data_ora_arrivo)";

            if (ora.Contains("12")) sql += sqlOra12; else sql += sqlOra19;
            sql += "  order by descrizione";
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                t.Add(new Tavolata((int)r[0]));
            }
            db.Dispose();
            return t;

        }
        /*
        public static List<Tavolata> getTavoliAttiviAlReparto(string idReparto)
        {

            List<Tavolata> ret = new List<Tavolata>();
            string endora = "";
            string ora = "";
            if (DateTime.Now.Hour >= 12 && DateTime.Now.Hour < 19) ora = "12:00"; else ora = "19:00";
            if (ora == "12:00") endora = $"convert(datetime, '{DateTime.Now.ToShortDateString()} 18:30' , 103)";
            else endora = $"dateadd(day, 1, convert(datetime, '{DateTime.Now.ToShortDateString()} 04:00', 103))";
            db db = new db();
            SqlDataReader r = db.getReader($"select * from tavolata where data_ora_arrivo  SYSDATETIME()			 BETWEEN data_ora_arrivo and dateadd(day, 1, data_ora_arrivo) ");
            while (r.Read())
            {
                Tavolata t = new Tavolata(r.GetInt32(0));
                t.ComandeReparto = Comande.getComandePerReparto(idReparto, t.Id);
                if (t.ComandeReparto.Count > 0) ret.Add(t);
            }
            db.Dispose();
            return ret;
    }


        */
    }

    [Serializable]
    public class Sala
    {
        private int _id;
        private string _descrizione;
        private int _coperti;

        public int Id { get => _id; set => _id = value; }
        public string Descrizione { get => _descrizione; set => _descrizione = value; }
        public int Coperti { get => _coperti; set => _coperti = value; }

        public static Sala getSalaByID(int id)
        {
            db db = new db();
            SqlDataReader r = db.getReader($"Select *  from sale where id_sala={id.ToString()}");
            if (r.HasRows)
            {
                r.Read();
                Sala s = new Sala() { Id = (int)r[0], Descrizione = (string)r[1], Coperti = (int)r[2] };
                db.Dispose();
                return s;
            }
            else
            {
                db.Dispose();
                return null;
            }
        }


    }

    public class Reparti
    {
        public Reparti() { }
        public Reparti(string idRep)
        {
            db db = new db();
            SqlDataReader r = db.getReader($"select * from reparti where id_reparto='{idRep}'");
            r.Read();
            this.id_reparto = idRep;
            this.descrizione = r[1].ToString();
            this.ip_stampante = r[2].ToString();
            this.nomestampante = r[3].ToString();
            db.Dispose();
        }
        public string id_reparto { get; set; }
        public string descrizione { get; set; }
        public string ip_stampante { get; set; }
        public string nomestampante { get; set; }
    }

    [Serializable]
    public class Comande
    {
        private int _idComande;
        private string _stato;
        private int _idTavolata;
        private string _descrizioneTavolata;
        private string _idPietanza;
        private string _descrizionPietanza;
        private int _quantita;
        private string _variazioni;
        private DateTime _OraComanda;
        private string _operatore;
        private int _isExtra;
        private string _sala;

        public Comande() { }
        public Comande(int id)
        {
            db db = new db();
            SqlDataReader r = db.getReader("select * from comande where id_comanda=" + id.ToString());
            r.Read();

            _idComande = r.GetInt32(0);
            _stato = r[1].ToString();
            _idTavolata = r.GetInt32(2);
            _idPietanza = r[3].ToString();
            _quantita = r.GetInt32(4);
            _variazioni = r[5].ToString();
            try
            {
                _OraComanda = r.GetDateTime(6);
            }
            catch (Exception ex) { }
            db.Dispose();
        }

        public int IdComande { get => _idComande; set => _idComande = value; }
        public string Stato { get => _stato; set => _stato = value; }
        public int IdTavolata { get => _idTavolata; set => _idTavolata = value; }
        public string IdPietanza { get => _idPietanza; set => _idPietanza = value; }
        public int Quantita { get => _quantita; set => _quantita = value; }
        public string Variazioni { get => _variazioni; set => _variazioni = value; }
        public int IsExtra { get => _isExtra; set => _isExtra = value; }
        public DateTime OraComanda { get => _OraComanda; set => _OraComanda = value; }
        public string DescrizioneTavolata { get => _descrizioneTavolata; set => _descrizioneTavolata = value; }
        public string DescrizionPietanza { get => _descrizionPietanza; set => _descrizionPietanza = value; }
        public string Operatore { get => _operatore; set => _operatore = value; }
        public string Sala { get => _sala; set => _sala = value; }

        public static Dictionary<string, List<Comande>> getComandeByStatoPerRep(string stato, int idTavolata)
        {
            Dictionary<string, List<Comande>> lstRet = new Dictionary<string, List<Comande>>();

            db db = new db();
            string sqlRep = $@"select rp.descrizione , rp.id_reparto
                             from comande c,
	                            reparti  rp,
	                            pietanze p
                            where c.id_tavolata = {idTavolata}
	                            and p.id_pietanza = c.id_pietanza
	                            and rp.id_reparto = p.reparto
                                and c.stato = '{stato}'
                            group by rp.descrizione, rp.id_reparto";

            SqlDataReader rRep = db.getReader(sqlRep);
            while (rRep.Read())
            {
                List<Comande> lst = new List<Comande>();
                db db2 = new db();


                string sql = $@"select * from comande where  id_tavolata={idTavolata} and 
	                            id_pietanza in (select id_pietanza from pietanze where reparto = '{rRep[1].ToString()}') 
	                            and stato = '{stato}' order by id_pietanza";


                SqlDataReader r = db2.getReader(sql);
                while (r.Read())
                {

                    Comande c = new Comande();
                    c.IdComande = r.GetInt32(0);
                    c.Stato = r[1].ToString();
                    c.IdTavolata = r.GetInt32(2);
                    Tavolata tav = new Tavolata(c.IdTavolata);
                    c._idPietanza = r[3].ToString();
                    c._descrizionPietanza = new Pietanza(c.IdPietanza).Descrizione;
                    c._descrizioneTavolata = tav.Descrizione;

                    c._operatore = tav.Operatore.Nominativo;
                    c.Quantita = r.GetInt32(4);
                    c.Variazioni = r[5].ToString();
                    try
                    {
                        c.OraComanda = r.GetDateTime(6);
                    }
                    catch (Exception ex) { }
                    c.Sala = tav.Sala.Descrizione;

                    lst.Add(c);
                }
                lstRet.Add(rRep[1].ToString(), lst);
                db2.Dispose();
            }

            db.Dispose();
            return lstRet;
        }

        public static bool CheckVariazioneStato(string statoClient, int idComanda)
        {
            Comande c = new Comande(idComanda);
            if (c.Stato == statoClient) return false; else return true;
        }


        public static List<Comande> getComandePerReparto(string idreparto, int idTavolata)
        {

            List<Comande> listRet = new List<Comande>();
            db db = new db();
            string sql = $@" select * from comande 
	                            where   
	                            id_pietanza in (select id_pietanza from pietanze where reparto = '{idreparto}')
	                            and stato not in ('attesa','annullato','pronto') 
	                            and id_tavolata in 
                                (select id_tavolata from tavolata t 
                                    where id_tavolata={idTavolata.ToString()}";
            //t.data_ora_arrivo = convert(datetime, '{timestamp.ToShortDateString()}',103))
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                Comande c = new Comande();
                c.IdComande = r.GetInt32(0);
                c.Stato = r[1].ToString();
                c.IdTavolata = r.GetInt32(2);
                c._idPietanza = r[3].ToString();
                c.Quantita = r.GetInt32(4);
                c.Variazioni = r[5].ToString();
                try
                {
                    c.OraComanda = r.GetDateTime(6);
                }
                catch (Exception ex) { }
                listRet.Add(c);
            }
            db.Dispose();
            return listRet;
        }
        /*
        
         */
    }




    [Serializable]
    public class Comanda
    {
        public int Id_comanda { get; set; }
        public string Stato { get; set; }
        public int Id_tavolata { get; set; }
        public string Id_pietanza { get; set; }
        public int Quantita { get; set; }
        public string Variazioni { get; set; }
        [AllowNull]
        public DateTime? Ora_comanda { get; set; }
        [AllowNull]
        public DateTime? Ora_stampa { get; set; }
        [AllowNull]
        public Pietanza? Pietanza { get; set; }

        public Comanda(int id_comanda)
        {
            Id_comanda = id_comanda;
            CaricaDatiDaDatabase();
        }

        private void CaricaDatiDaDatabase()
        {
            db db = new db();
            string query = $"SELECT * FROM comande WHERE id_comanda = {Id_comanda}";

            
            try
            {

                SqlDataReader reader = db.getReader(query);

                if (reader.Read())
                {
                    Stato = reader["stato"].ToString();
                    Id_tavolata = (int)reader["id_tavolata"];
                    Id_pietanza = reader["id_pietanza"].ToString();
                    Quantita = (int)reader["quantita"];
                    Variazioni = reader["variazioni"].ToString();
                    Ora_comanda = reader["ora_comanda"] as DateTime?;
                    Ora_stampa = reader["ora_stampa"] as DateTime?;
                    Pietanza = new Pietanza(Id_pietanza);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore durante il caricamento dei dati: " + ex.Message);
            }
            
        }
    }



    [Serializable]
    public struct StatoComande
    {
        private string _id_comanda;
        private string _stato;
        private string _id_pietanza;
        private string _desc_pietanza;
        private string _quantita;
        private string _variazioni;
        private string _id_tipo;
        private string _desc_tipo;
        private string _id_reparto;
        private string _reparto;

        public string Id_comanda { get => _id_comanda; set => _id_comanda = value; }
        public string Stato { get => _stato; set => _stato = value; }
        public string Id_pietanza { get => _id_pietanza; set => _id_pietanza = value; }
        public string Quantita { get => _quantita; set => _quantita = value; }
        public string Variazioni { get => _variazioni; set => _variazioni = value; }
        public string Id_tipo { get => _id_tipo; set => _id_tipo = value; }
        public string Id_reparto { get => _id_reparto; set => _id_reparto = value; }
        public string Reparto { get => _reparto; set => _reparto = value; }
        public string Desc_pietanza { get => _desc_pietanza; set => _desc_pietanza = value; }
        public string Desc_tipo { get => _desc_tipo; set => _desc_tipo = value; }
    }
    [Serializable]
    public class TestaConto
    {
        private int _idTavolata;
        private string _tavolata;
        private string _data_ora;
        private float _acconto;
        private float _sconto;
        private string _sala;
        private string _cameriere;

        public string Tavolata { get => _tavolata; set => _tavolata = value; }
        public string Data_ora { get => _data_ora; set => _data_ora = value; }
        public float Acconto { get => _acconto; set => _acconto = value; }
        public float Sconto { get => _sconto; set => _sconto = value; }
        public string Sala { get => _sala; set => _sala = value; }
        public string Cameriere { get => _cameriere; set => _cameriere = value; }
        public int IdTavolata { get => _idTavolata; set => _idTavolata = value; }
    }
    [Serializable]
    public struct InsertExtra
    {
        private DateTime _data;
        private string _idpietanza;
        private float _prezzo;
        private int _quantita;

        public DateTime Data { get => _data; set => _data = value; }
        public string Idpietanza { get => _idpietanza; set => _idpietanza = value; }
        public float Prezzo { get => _prezzo; set => _prezzo = value; }
        public int Quantita { get => _quantita; set => _quantita = value; }
    }
    [Serializable]
    public class IncassoGiorno
    {
        private DateTime _dateTime;
        private string _descrizione1;
        private string _descrizione2;
        private float _prezzo;
        private float _quantita;
        private float _acconto;
        private float _sconto;
        private float _totale;

        public DateTime DateTime { get => _dateTime; set => _dateTime = value; }
        public string Descrizione1 { get => _descrizione1; set => _descrizione1 = value; }
        public string Descrizione2 { get => _descrizione2; set => _descrizione2 = value; }
        public float Prezzo { get => _prezzo; set => _prezzo = value; }
        public float Quantita { get => _quantita; set => _quantita = value; }
        public float Acconto { get => _acconto; set => _acconto = value; }
        public float Sconto { get => _sconto; set => _sconto = value; }
        public float Totale { get => _totale; set => _totale = value; }

    }
    [Serializable]
    public class CorpoConto
    {
        private string _id;
        private string _menu_portata;
        private float _quantita;
        private float _prezzo_unitario;
        private float _totale;
        private float _granTotale;

        public string Id { get => _id; set => _id = value; }
        public string Menu_portata { get => _menu_portata; set => _menu_portata = value; }
        public float Quantita { get => _quantita; set => _quantita = value; }
        public float Prezzo_unitario { get => _prezzo_unitario; set => _prezzo_unitario = value; }
        public float Totale { get => _totale; set => _totale = value; }
        public float GranTotale { get => _granTotale; set => _granTotale = value; }
    }
    [Serializable]
    public class ordine
    {
        private int _id_ordine;
        private int _id_tavolata;
        private string _id_voce;
        private int _quantita;
        private string _note_pietanza;
        private string _stato;
        private Object? _voce;
        private Comanda? _comanda;
        

        public int Id_tavolata { get => _id_tavolata; set => _id_tavolata = value; }
        public string Id_voce { get => _id_voce; set => _id_voce = value; }
        public int Quantita { get => _quantita; set => _quantita = value; }
        public string Note_pietanza { get => _note_pietanza; set => _note_pietanza = value; }
        public int Id_ordine { get => _id_ordine; set => _id_ordine = value; }
        public string Stato { get => _stato; set => _stato = value; }
        public Object? Voce { get => _voce; set => _voce = value; }
        public Comanda? Comanda { get => _comanda; set => _comanda = value; }

        public ordine() { }
    }
    /*
     * CREO CLASSI DI SERVIZIO PER LA CLASSE PIETANZA
     */
    public class TipiPietanze 
    {
        private int _id_tipo;
        private string _descrizione;
        private string _image;

        public int Id_tipo { get => _id_tipo; set => _id_tipo = value; }
        public string Descrizione { get => _descrizione; set => _descrizione = value; }
        public string Image { get => _image; set => _image = value; }

        public TipiPietanze() { }
        public TipiPietanze(int id) 
        {
            db db = new db();
            SqlDataReader r = db.getReader($"select * from tipi_pietanze where id_tipo={id}");
            if (r.HasRows)
            {
                r.Read();
                Id_tipo = r.GetInt32(0);
                Descrizione = r[1].ToString();
                Image = r[2].ToString();
               
            }
            else
            {
                throw new Exception("Nessun Tipo_Pietanza con questo ID");
            }
            db.Dispose();
        }
    
    }
  
    [Serializable]
    public class Pietanza
    {
        private string _id_pietanza;
        private string _descrizione;
        private float _prezzo;
        private string _reparto;
        private int _attivo;
        private int _id_tipo;
        private Reparti _repartoClasse;
        private TipiPietanze _tipo_pietanze;

        public string Id_pietanza { get => _id_pietanza; set => _id_pietanza = value; }
        public string Descrizione { get => _descrizione; set => _descrizione = value; }
        public float Prezzo { get => _prezzo; set => _prezzo = value; }
        public string Reparto { get => _reparto; set => _reparto = value; }
        public int Attivo { get => _attivo; set => _attivo = value; }
        public int Id_tipo { get => _id_tipo; set => _id_tipo = value; }
        public Reparti RepartoClasse { get => _repartoClasse; set => _repartoClasse = value; }
        public TipiPietanze Tipo_pietanze { get => _tipo_pietanze; set => _tipo_pietanze = value; }

        public Pietanza() { }

        public Pietanza(string ID)
        {
            db db = new db();
            SqlDataReader r = db.getReader($"select * from pietanze where id_pietanza='{ID}'");
            if (r.HasRows)
            {
                r.Read();
                Id_pietanza = r[0].ToString();
                Descrizione = r[1].ToString();
                Prezzo = float.Parse(r[2].ToString());
                Reparto = r[3].ToString();
                Attivo = r.GetInt32(4);
                Id_tipo = r.GetInt32(5);
                RepartoClasse= new Reparti(r[3].ToString());
                Tipo_pietanze = new TipiPietanze(r.GetInt32(5));
            }
            else
            {
                throw new Exception("Nessuna Pietanza con questo ID");
            }
            db.Dispose();
        }

        public static List<Pietanza> GetPietanzeAttive() 
        {
            List<Pietanza> list = new List<Pietanza>();
       
            db db = new db(); 
            string sql = $@"SELECT * from pietanze where attivo=1 order by reparto, descrizione";

      
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                list.Add(new Pietanza(r[0].ToString()));
            }
            db.Dispose();
            return list;
        }
      
    }
    [Serializable]
    public struct trovato //usato nel metodo TROVATO  per restituire un booleano se esiste un record con ID_OPERATORE - PIN per login cameriere
    {
        private Boolean _trovato;

        public bool Trovato { get => _trovato; set => _trovato = value; }
        public string Nominativo { get; set; }

    }
    [Serializable]
    public class Menu
    {
        private string _id_menu;
        private string _descrizione;
        private string _tipo;
        private float _prezzo;
        private string _occasione;
        private bool _stato;
        //private int _quantita;

        public string Id_menu { get => _id_menu; set => _id_menu = value; }
        public string Descrizione { get => _descrizione; set => _descrizione = value; }
        public string Tipo { get => _tipo; set => _tipo = value; }
        public float Prezzo { get => _prezzo; set => _prezzo = value; }
        public string Occasione { get => _occasione; set => _occasione = value; }
        public bool Stato { get => _stato; set => _stato = value; }
        //public int QuantitaOrdinata { get => _quantita; set => _quantita = value; }

        public Menu() { }
        public Menu(string id_menu)
        {
            _id_menu = id_menu;
            CaricaDatiDaDatabase();
        }

        private void CaricaDatiDaDatabase()
        {
            db db = new db();
            string query = $"SELECT Descrizione, Tipo, Prezzo, Occasione, Stato FROM Menu WHERE Id_menu ='{Id_menu}'" ;

          
            
            try
            {
                
                SqlDataReader reader = db.getReader(query);

                if (reader.Read())
                {
                    _descrizione = reader["Descrizione"].ToString();
                    _tipo = reader["Tipo"].ToString();
                    _prezzo = float.Parse(reader["Prezzo"].ToString());
                    _occasione = reader["Occasione"].ToString();
                    _stato = bool.Parse(reader["Stato"].ToString());
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore durante il caricamento dei dati: " + ex.Message);
            }
            
        }
    }
   
    [Serializable]
    public struct Menudettaglio
    {
        private string _id_pietanza;
        private string _descrizione;
        //private float _prezzo;
        private string _reparto;
        private int _stato;
        private int _id_tipo;
        private string _num_alternanza;

        public string Id_pietanza { get => _id_pietanza; set => _id_pietanza = value; }
        public string Descrizione { get => _descrizione; set => _descrizione = value; }
        //public float Prezzo { get => _prezzo; set => _prezzo = value; }
        public string Reparto { get => _reparto; set => _reparto = value; }
        public int Stato { get => _stato; set => _stato = value; }
        public int Id_tipo { get => _id_tipo; set => _id_tipo = value; }
        public string Num_alternanza { get => _num_alternanza; set => _num_alternanza = value; }
    }
    
    [Serializable]
    public class Operatori 
    {
        private int id_operatore;
        private string nominativo;
        private string pin;
        private bool attivo;

        public int Id_operatore { get => id_operatore; set => id_operatore = value; }
        public string Nominativo { get => nominativo; set => nominativo = value; }
        public string Pin { get => pin; set => pin = value; }
        public bool Attivo { get => attivo; set => attivo = value; }

        public Operatori() { }
        public Operatori(int id) 
        {
            db db = new db();
            SqlDataReader r = db.getReader($"select * from operatori where id_operatore={id}");
            if (r.HasRows)
            {
                r.Read();
                id_operatore = r.GetInt32(0);
                nominativo = r[1].ToString();
                pin = r[2].ToString();
                attivo =r.GetBoolean(3);
            }
            else
            {
                throw new Exception("Nessun Operatore con questo ID");
            }
            db.Dispose();
        }
        public static Operatori? Create(string nominativo, string pin)
        {
            db db = new db();
            SqlDataReader r = db.getReader($"select * from operatori where nominativo='{nominativo}' and pin='{pin}' and attivo='true'");
            if (r.HasRows)
            {
                r.Read();
                var operatore = new Operatori
                {
                    id_operatore = r.GetInt32(0),
                    nominativo = r[1].ToString(),
                    pin = r[2].ToString(),
                    attivo = r.GetBoolean(3)
                };
                db.Dispose();
                return operatore;
            }
            else
            {
                db.Dispose();
                return null;
            }
        }

    }

    [Serializable]
    public class Stato 
    {
        private int id_stato;
        private string descrizione;
        private string colore;
        private string colore_codice;

        public int Id_stato { get => id_stato; set => id_stato = value; }
        public string Descrizione { get => descrizione; set => descrizione = value; }
        public string Colore { get => colore; set => colore = value; }
        public string Colore_codice { get => colore_codice; set => colore_codice = value; }

        public Stato() { }
        public Stato(int id)
        {
            db db = new db();
            SqlDataReader r = db.getReader($"select * from stato where id_stato={id}");
            if (r.HasRows)
            {
                r.Read();
                id_stato = r.GetInt32(0);
                descrizione = r[1].ToString();
                colore= r[2].ToString();
                colore_codice = r[2].ToString();
            }
            else
            {
                throw new Exception("Nessuno Stato con questo ID");
            }
            db.Dispose();
        }

    }
}
