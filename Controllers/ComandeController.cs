using ComandeRestAPI.Classi;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using CrystalDecisions.CrystalReports.Engine;
using System.Net.Http;

using static ComandeRestAPI.Classi.ClassiStampa;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Serialization;
using CrystalDecisions.ReportAppServer.CommonControls;
using System.Reflection;

namespace ComandeRestAPI.Controllers
{
   
    


    [Route("api/[controller]")]
    [ApiController]
    public class ComandeController : ControllerBase
    {

        private readonly HttpClient _client;
        private readonly SqlConnection _conn;
        private readonly IWebHostEnvironment _env;

        // Single constructor with all dependencies
        public ComandeController(IHttpClientFactory clientFactory, IWebHostEnvironment env, SqlConnection conn)
        {
            _client = clientFactory.CreateClient();
            _client.BaseAddress = new Uri("http://192.168.0.225:81/mioserv.asmx"); 
            //_client.BaseAddress = new Uri("http://localhost:56515/mioserv.asmx");
            _env = env;
            _conn = conn ?? new SqlConnection(db.connStr());
        }
   
        [HttpGet("getPietanza")]
        public ActionResult<Pietanza> GetPietanza(string Id_pietanza)
        {
            if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
            SqlCommand comm = new SqlCommand("select * from pietanze where id_pietanza=@Id_pietanza", _conn);
            comm.Parameters.AddWithValue("@Id_pietanza", Id_pietanza);
            SqlDataReader r = comm.ExecuteReader();
            Pietanza pie = new Pietanza();
            while (r.Read())
            {
                pie.Id_pietanza = r[0].ToString().Trim();
                pie.Descrizione = r[1].ToString().Trim();
                pie.Reparto = r[3].ToString().Trim();
            }
            r.Close();
            _conn.Close();
            return Ok(pie);
        }
        [HttpGet("getPietanze")]
        public ActionResult<IEnumerable<Pietanza>> GetPietanze(string descrizione)
        {
            var names = new List<Pietanza>();
            
            try
            {
                if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                SqlCommand comm = new SqlCommand("select * from pietanze where descrizione like @descrizione order by descrizione", _conn);
                comm.Parameters.AddWithValue("@descrizione", "%" + descrizione.ToUpper().Trim() + "%");
                SqlDataReader myReader = comm.ExecuteReader();
                while (myReader.Read())
                {
                    Pietanza p = new Pietanza
                    {
                        Id_pietanza = myReader[0].ToString(),
                        Descrizione = myReader[1].ToString(),
                        Prezzo = float.Parse(myReader[2].ToString()),
                        Reparto = myReader[3].ToString(),
                        Attivo = int.Parse(myReader[4].ToString()),
                        Id_tipo = int.Parse(myReader[5].ToString())
                    };
                    names.Add(p);
                }
                myReader.Close();
                _conn.Close();
            }
            catch (Exception ex)
            {
                // Handle exception
            
                _conn.Close();
                return StatusCode(500, ex.Message);
            }
            finally
            {
                GC.Collect();
                
            }
            return Ok(names);
        }
        [HttpGet("getPietanzeAttive")]
        public ActionResult<IEnumerable<Pietanza>> GetPietanzeAttive()
        {
            
            return Ok(Pietanza.GetPietanzeAttive());
        }
        [HttpGet("getMenu")]
        public ActionResult<IEnumerable<Menu>> GetMenu()
        {
            var menus = new List<Menu>();
            try
            {
                if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                SqlCommand comm = new SqlCommand("select * from menu where stato ='True' order by descrizione", _conn);
                SqlDataReader myReader = comm.ExecuteReader();
                while (myReader.Read())
                {
                    Menu m = new Menu
                    {
                        Id_menu = myReader[0].ToString(),
                        Descrizione = myReader[1].ToString(),
                        Prezzo = float.Parse(myReader[3].ToString()),
                        Tipo = myReader[2].ToString(),
                        Occasione = myReader[4].ToString(),
                        Stato = bool.Parse(myReader[5].ToString())
                    };
                    menus.Add(m);
                }
                myReader.Close();
                _conn.Close();
            }
            catch (Exception ex)
            {
                // Handle exception
                _conn.Close();
                return StatusCode(500, ex.Message);
            }
            finally
            {
                GC.Collect();
            }
            return Ok(menus);
        }
        [HttpGet("getTavolateOdierne")]
        public ActionResult<IEnumerable<Tavolata>> GetTavolateOdierne()
        {
            List<Tavolata> list = new List<Tavolata>();
            list = Tavolata.GetTavolateOdierne();
            return Ok(list);
        }
        [HttpGet("getTavolateByData")] // usata app Gestore
        public ActionResult<IEnumerable<Tavolata>> GetTavolateByData(DateTime data)
        {
            List<Tavolata> list = new List<Tavolata>();
            list = Tavolata.GetTavolateByData(data);
            return Ok(list);
        }

        //
        //getScontrinoTavolo
        [HttpGet("getScontrinoTavolo")] // usata app Gestore
        public ActionResult<IEnumerable<Scontrino>> getScontrinoTavoloa(int id_tavolata)
        {
            List<Scontrino> list = new List<Scontrino>();
            list = Scontrino.getScontrinoTavolo(id_tavolata);
            return Ok(list);
        }
        [HttpGet("getStoricoTavolibyCliente")] // usata app Gestore
        public ActionResult<IEnumerable<TavoliStorico>> getStoricoTavolibyCliente(int id_cliente)
        {
            List<TavoliStorico> list = new List<TavoliStorico>();
            list = Cliente.getStoricoTavoli(id_cliente);
            return Ok(list);
        }
        [HttpGet("getIncassoDettaglio")]
        public ActionResult<IEnumerable<IncassoGiorno>> GetIncassoDettaglio(string data)
        {
            var incasso = new List<IncassoGiorno>();
            try
            {
                if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                string data2 = DateTime.Parse(data).ToShortDateString();
                string sql = $@"
                    select DATA_ORA=extra.data,  DESCRIZIONE1='SENZA TAVOLO', DESCRIZIONE2=pietanze.descrizione,extra.prezzo, extra.quantita, ACCONTO=0, SCONTO=0,TOTALE=extra.prezzo*extra.quantita
                    from extra 
                    inner join pietanze on extra.id_pietanza=pietanze.id_pietanza 
                    where (extra.data BETWEEN convert(datetime, '{data2} 09:00' , 103) and dateadd(day,1,convert(datetime, '{data2} 04:00' , 103)))
                    union 
                    select DATA_ORA=tavolata.data_ora_arrivo,  DESCRIZIONE1='Tavolo '+tavolata.descrizione, DESCRIZIONE2=pietanze.descrizione, pietanze.prezzo, ordini.quantita,ACCONTO=0, SCONTO=0, TOTALE=ordini.quantita*pietanze.prezzo
                    from tavolata 
                    join ordini on tavolata.id_tavolata=ordini.id_tavolata 
                    join pietanze on pietanze.id_pietanza=ordini.id_pietanza 
                    where (tavolata.data_ora_arrivo BETWEEN convert(datetime, '{data2} 09:00' , 103) and dateadd(day,1,convert(datetime, '{data2} 04:00' , 103))) and (tavolata.stato = 3 or tavolata.stato = 4)
                    union
                    select DATA_ORA=tavolata.data_ora_arrivo, DESCRIZIONE1='Tavolo '+tavolata.descrizione, DESCRIZIONE2=menu.descrizione, menu.prezzo,ordini.quantita,ACCONTO=0, SCONTO=0, TOTALE=ordini.quantita*menu.prezzo
                    from tavolata 
                    join ordini on tavolata.id_tavolata=ordini.id_tavolata 
                    join menu on menu.id_menu=ordini.id_menu 
                    where (tavolata.data_ora_arrivo BETWEEN convert(datetime, '{data2} 09:00' , 103) and dateadd(day,1,convert(datetime, '{data2} 04:00' , 103))) and (tavolata.stato = 3 or tavolata.stato = 4)
                    union 
                    select DATA_ORA=tavolata.data_ora_arrivo, DESCRIZIONE1='Tavolo '+tavolata.descrizione,DESCRIZIONE2=prestazioni_extra.descrizione, prestazioni_extra.prezzo, quantita=1, ACCONTO=0, SCONTO=0,TOTALE=prestazioni_extra.prezzo
                    from tavolata 
                    join prestazioni_extra on prestazioni_extra.idTavolata=tavolata.id_tavolata
                    where (tavolata.data_ora_arrivo BETWEEN convert(datetime, '{data2} 09:00' , 103) and dateadd(day,1,convert(datetime, '{data2} 04:00' , 103))) and (tavolata.stato = 3 or tavolata.stato = 4)
                    union 
                    select DATA_ORA=tavolata.data_ora_arrivo,DESCRIZIONE1='Tavolo '+tavolata.descrizione, DESCRIZIONE2='ACCONTO & SCONTO',  prezzo=0,quantita=1,ACCONTO=tavolata.acconto,SCONTO=tavolata.sconto,TOTALE=0
                    from tavolata 
                    where (tavolata.data_ora_arrivo BETWEEN convert(datetime, '{data2} 09:00' , 103) and dateadd(day,1,convert(datetime, '{data2} 04:00' , 103))) and (acconto <> 0 or sconto <> 0)
                    order by DATA_ORA";
                SqlCommand comm = new SqlCommand(sql, _conn);
                SqlDataReader myReader = comm.ExecuteReader();
                while (myReader.Read())
                {
                    IncassoGiorno ig = new IncassoGiorno
                    {
                        DateTime = myReader.GetDateTime(0),
                        Descrizione1 = myReader[1].ToString(),
                        Descrizione2 = myReader[2].ToString(),
                        Prezzo = float.Parse(myReader[3].ToString()),
                        Quantita = float.Parse(myReader[4].ToString()),
                        Acconto = float.Parse(myReader[5].ToString()),
                        Sconto = float.Parse(myReader[6].ToString()),
                        Totale = float.Parse(myReader[7].ToString())
                    };
                    incasso.Add(ig);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            _conn.Close();
            return Ok(incasso);
        }

        [HttpGet("getTotaleIncassoDataOra")]  // usata app Gestore - inserita il 28/07/2025
        public ActionResult<List<SintesiIncasso>> getTotaleIncassoDataOra(string data) 
        {
            List<SintesiIncasso> list = new List<SintesiIncasso>();
            list.Add(SintesiIncasso.getSintesiIncasobyDataOra(data,"12:00"));
            list.Add(SintesiIncasso.getSintesiIncasobyDataOra(data, "19:00"));
            return list;
        }

        [HttpGet("getTotaleTipoIncassoDataOra")]  // usata app Gestore - inserita il 28/07/2025
        public ActionResult<List<SintesiTipoIncasso>> getTotaleTipoIncassoDataOra(string data)
        {
            List<SintesiTipoIncasso> list = new List<SintesiTipoIncasso>();
            list.Add(SintesiTipoIncasso.getSintetesiTipoIncassobyDataOra(data, "12:00"));
            list.Add(SintesiTipoIncasso.getSintetesiTipoIncassobyDataOra(data, "19:00"));

            return list;
        }

        [HttpGet("getTotaleIncassato")]
        public ActionResult<float> GetTotaleIncassato(string data)
        {
            DateTime dt = DateTime.Now;
            if (DateTime.Now.Hour < 11) dt = dt.AddDays(-1);
            if (data == "") data = dt.ToShortDateString();
            float incasso = 0;

            foreach (IncassoGiorno item in GetIncassoDettaglio(data).Value)
            {
                incasso += item.Totale - item.Acconto - item.Sconto;
            }
            return Ok(incasso);
        }
        [HttpGet("getTotaleContoTavolo")] // usata app Gestore
        public ActionResult<string> GetTotaleContoTavolo(int idtavolo)
        {
            
            return Ok(string.Format("{0:0.00}", Tavolata.getTotaleContoTavolo(idtavolo)));
        }
        [HttpGet("getTavolatabyID")]
        public ActionResult<Tavolata> GetTavolatabyID(int id)
        {
           
            return Ok(new Tavolata(id));
        }
        [HttpPost("aggiornaTavolo")]
        public IActionResult AggiornaTavolo([FromBody] Tavolata ta)
        {
            //if (ta.Acconto == "") acconto = "0.0";
            //if (sconto == "") sconto = "0.0";
            string sql = @$"update tavolata set 
                            acconto={ta.Acconto}, 
                            stato={ta.Id_stato},
                            adulti={ta.Adulti},
                            bambini={ta.Bambini},
                            id_sala={ta.Id_sala},
                            descrizione='{ta.Descrizione}',
                            note='{ta.Note}',
                            sconto={ta.Sconto},
                            id_cliente={ta.Id_cliente},         
                            preconto={ta.Preconto},
                            conto={ta.Conto},
                            numero_tavolo={ta.Numero_tavolo}
                            where id_tavolata={ta.Id}";
            db db = new db();
            db.getReader(sql);
            db.Dispose();
            return Ok();
        }

        [HttpPost("updateSTatoTavolata")]
        public IActionResult UpdateStatoTavolata(int id_tavolata, int stato)
        {
            string sql = $@"update tavolata set stato={stato} where id_tavolata={id_tavolata}";
            db db = new db();
            db.getReader(sql);
            db.Dispose();
            return Ok();
        }

        [HttpPost("aggiornaTavolataMini")]
        public IActionResult AggiornaTavolataMini([FromBody] TavolataMini ta, int id_tavolata)
        {
            //if (ta.Acconto == "") acconto = "0.0";
            //if (sconto == "") sconto = "0.0";
            string sql = @$"update tavolata set 
                            adulti={ta.Adulti},
                            bambini={ta.Bambini},
                            id_sala={ta.IdSala},
                            descrizione='{ta.Descrizione}',
                            note='{ta.Note}',
                            numero_tavolo={ta.NumeroTavolo}
                            where id_tavolata={id_tavolata}";
            db db = new db();
            db.getReader(sql);
            db.Dispose();
            return Ok();
        }

        [HttpPost("insertTavolata")]
        public IActionResult insertTavolata([FromBody] Tavolata ta)
        {
            
            
            string sql = @$"insert into tavolata (
                                                convert(date,{DateTime.Now},
                                                null,
                                                {ta.Acconto},
                                                {ta.Stato},
                                                {ta.Id_operatore},
                                                {ta.Adulti},
                                                {ta.Bambini},
                                                {ta.Id_sala},
                                                '{ta.Descrizione.Replace("'", "''")}'
                                                '{ta.Note.Replace("'", "''")}',    
                                                {ta.Sconto},
                                                {ta.Id_cliente},
                                                {ta.Preconto},
                                                {ta.Conto},
                                                '{ta.Item}',
                                                {ta.Numero_tavolo})";

            db db = new db();
            db.getReader(sql);
            db.Dispose();
            return Ok();

        }
        [HttpPost("creaTavolata")]
        public IActionResult creaTavolata([FromBody] TavolataMini t)
        {
            string ora = "";
            if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 17) ora = $"convert(datetime, '{DateTime.Now.ToShortDateString()} 12:00' , 103)";
            else ora = $"convert(datetime, '{DateTime.Now.ToShortDateString()} 19:00', 103)";
           
          
            string sql = @$"insert into tavolata (data_ora_arrivo,id_operatore, stato, descrizione, adulti, bambini, numero_tavolo, note, id_sala)
                            values (
                                                {ora},
                                                {t.IdOperatore},
                                                1,
                                                '{t.Descrizione.ToUpper().Replace("'","''")}',
                                                {t.Adulti},
                                                {t.Bambini},
                                                {t.NumeroTavolo},
                                                '{t.Note}',
                                                {t.IdSala})";

            db db = new db();
            db.getReader(sql);
            db.Dispose();
            return Ok();
        }
         [HttpDelete("deleteTavolata/{id_tavolata}")] // usata app Gestore
        public ActionResult<bool> deletetavolata(int id_tavolata)
        {
            try
            {
                Tavolata.deleteTavolata(id_tavolata);
                // cancello anche eventuali Acconti dalla tabella Pagamenti
                //cancella tutti i record che contengono idtav
                var db = new db();
                string sql = $@"delete from pagamenti where id_tavolata={id_tavolata}";
                SqlDataReader r = db.getReader(sql);
                int id_p = r.RecordsAffected;
                db.Dispose();
                return Ok(true);
            }
            catch 
            {
                return Ok(false);
            }
            
        }
        [HttpPost("creaPrenotazione")] // usata app Gestore
        public async Task<IActionResult> creaPrenotazione([FromBody] TavolataMini2 t)
        {
            double acconto=t.Acconto;
            string ora = $"convert(datetime, '{t.Data_ora_arrivo}', 103)";
            string sql = @$"insert into tavolata (data_ora_arrivo,id_cliente, stato, descrizione, adulti, bambini, note, id_sala,item, acconto)
                            values (
                                                {ora},
                                                {t.IdCliente},
                                                {t.Stato},
                                                '{t.Descrizione.ToUpper().Replace("'", "''")}',
                                                {t.Adulti},
                                                {t.Bambini},
                                               '{t.Note.Replace("'", "''")}',
                                                {t.IdSala},
                                               '{t.Item}',
                                                {t.Acconto})  SELECT SCOPE_IDENTITY()";

            db db = new db();
            SqlDataReader r = db.getReader(sql);
            r.Read();
            int index = int.Parse(r[0].ToString());
            db.Dispose();

            // ASPETTA 1 SECONDO PRIMA DI SCRIVERE IL LOG
            await Task.Delay(1000);

            // INSERISCO UN LOG PER LA PRENOTAZIONE
            string msg = $@"Inserimento prenotazione per il  {t.Data_ora_arrivo} a nome {t.Descrizione} in sala {t.IdSala} con id_tavolata:{index.ToString()}";
            string sql2 = $@"
                            INSERT INTO [dbo].[log_eventi]
                                       ([data]
                                       ,[id_operatore]
                                       ,[evento]
                                       ,[note])
                                 VALUES
                                       (convert(datetime,'{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString().Replace(".", ":")}', 103)
                                       ,'CarboRicc'
                                       ,'{msg.Replace("'", "''")}'
                                       ,'Note da Carboricc')";
            db db2 = new db();
            db2.exe(sql2);
            db2.Dispose();

            if (acconto > 0) // devoinserire anche una registrazione in tabella Pagamenti
            {
                Pagamenti p = new Pagamenti();
                p.Id_tavolata = index;
                p.Data_ora_registrazione = DateTime.Now;
                p.Conto_contanti = acconto; // diamo per default che l'acconto sia in contanti.....
                p.Tipo = 2;
                p.Conto_altro = 0;
                p.Conto_pos = 0;
                p.Note = "Registrazione di Acconto da App Gestori";
                Pagamenti.insert(p);

            }
            
            return Ok();
        }
      /*
        [HttpPost("stampaScontrino")] // usata app Gestore
        public async Task<IActionResult> StampaPDF([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File mancante");

            var tempPath = Path.Combine(Path.GetTempPath(), file.FileName);

            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            try
            {
                // Chiama la funzione per stampare
                StampaPdf(tempPath);
                return Ok("Stampa avviata");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante la stampa: {ex.Message}");
            }
        }
        public void StampaPdf(string filePath)
        {
            var acrobatPath = @"C:\Program Files\Adobe\Acrobat DC\Acrobat\Acrobat.exe";
        
            var process = new Process();
            process.StartInfo.FileName = acrobatPath;
            process.StartInfo.Arguments = $"/h /t \"{filePath}\" \"POS-CASSA\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
        }*/

        [HttpPost("updatePrenotazione")] // usata app Gestore
        public async Task<IActionResult> updatePrenotazione([FromBody] TavolataMini2 t)
        {
            if (t.Stato != 5) t.Stato = 1;
            string ora = $"convert(datetime, '{t.Data_ora_arrivo}', 103)";
            string sql = @$"update tavolata 
                            set note='{t.Note.Replace("'", "''")}',
                                adulti={t.Adulti},
                                bambini={t.Bambini},
                                id_sala={t.IdSala},
                                data_ora_arrivo={ora},
                                stato={t.Stato},
                                acconto={t.Acconto},
                                descrizione='{t.Descrizione.ToUpper().Replace("'", "''")}',
                                item='{t.Item}'
                            where id_tavolata={t.Id_tavolata}";

            db db = new db();
            db.getReader(sql);
            db.Dispose();

            // ASPETTA 1 SECONDO PRIMA DI SCRIVERE IL LOG
            await Task.Delay(1000);

            // INSERISCO UN LOG PER LA PRENOTAZIONE
            string msg = $@"Modifica prenotazione per il  {t.Data_ora_arrivo} a nome {t.Descrizione} in sala {t.IdSala} con id_tavolata:{t.Id_tavolata.ToString()}";
            string sql2 = $@"
                            INSERT INTO [dbo].[log_eventi]
                                       ([data]
                                       ,[id_operatore]
                                       ,[evento]
                                       ,[note])
                                 VALUES
                                       (convert(datetime,'{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString().Replace(".", ":")}', 103)
                                       ,'CarboRicc'
                                       ,'{msg.Replace("'", "''")}'
                                       ,'Note da Carboricc')";
            db db2 = new db();
            db2.exe(sql2);
            db2.Dispose();
            return Ok();
        }
        [HttpGet("getTavoliConto")]
        public ActionResult<string> GetTavoliConto(string ora)
        {
            db db = new db();
            string sqlOra12 = $" and Datepart(HOUR, data_ora_arrivo) =12";
            string sqlOra19 = $" and Datepart(HOUR, data_ora_arrivo) =19";
            string sql = $@"
                select tavolata.*, isnull(nominativo, 'non ass') as nominativo, sale.descrizione
                from tavolata 
                left join operatori on  tavolata.id_operatore = operatori.id_operatore
                inner join sale on tavolata.id_sala = sale.id_sala
                where SYSDATETIME() BETWEEN tavolata.data_ora_arrivo  and dateadd(day, 1, tavolata.data_ora_arrivo)";
            if (ora.Contains("12")) sql += sqlOra12; else sql += sqlOra19;
            sql += "order by 10,15";
            string html = "";
            SqlDataReader r = db.getReader(sql);
            int cnt = 1;
            html += "<table>";
            while (r.Read())
            {
                if (cnt == 1) html += "<tr>";
                html += "<td>";
                string style = "";
                if (r[4].ToString() == "4") { style = "style='background-color:deepskyblue;'"; }
                if (r[4].ToString() == "3") { style = "style='background-color:green;'"; }
                if (r[4].ToString() == "2") { style = "style='background-color:yellow;'"; }
                html += $@"
                    <a href='#popupMenu_{r[0].ToString()}' data-rel='popup' data-transition='slideup' {style}
                        class='ui-btn ui-corner-all ui-shadow ui-btn-inline ui-icon-gear ui-btn-icon-left ui-btn-a'
                    >Tavolo<br />{r[9].ToString()}</a>
                    <div data-role='popup' id='popupMenu_{r[0].ToString()}' data-theme='a'>
                        <ul data-role='listview' id='lst_{r[0].ToString()}' data-inset='true' style='min-width:210px;'>
                            <li data-role='list-divider'>Operatore {r[13].ToString()}</li>
                            <li data-role='list-divider'>Sala {r[14].ToString()}</li>
                            <li data-role='list-divider' id='coperti_{r[0].ToString()}'>Coperti {r[6].ToString()} + {r[7].ToString()}</li>
                            <li><input type='button' id='c_{r[0].ToString()}' value='Conto' onclick='stampaConto(this.id);' /></li>
                            <li><input type='button' id='d_{r[0].ToString()}' value='Dettagli' onclick='dettaglitav(this.id);'/></li>
                            <li><input type='button' id='p_{r[0].ToString()}' value='Pagato' onclick='SetPagato(this.id);'/></li>
                            <li><input type='button' id='o_{r[0].ToString()}' value='Operatore' onclick='apriPopupOperatore(this.id);'/></li>
                        </ul>
                    </div>";
                html += "</td>";
                cnt++;
                if (cnt == 6) { html += "</tr>"; cnt = 1; }
            }
            db.Dispose();
            if (!html.EndsWith("</tr>")) html += "</tr>";
            html += "</table>";
            return Ok(html);
        }
        [HttpPost("setCoperti")]
        public IActionResult SetCoperti(int idTavolo, string coperti)
        {
            int cAdutli; int cBamb;
            string[] tmp = coperti.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
            cAdutli = int.Parse(tmp[0]);
            cBamb = int.Parse(tmp[1]);
            db db = new db();
            string sql = $"update tavolata set adulti={cAdutli}, bambini={cBamb}, stato='1' where id_tavolata = {idTavolo}";
            db.getReader(sql);
            db.Dispose();
            return Ok();
        }
        [HttpPost("setSala")]
        public IActionResult SetSala(int idTavolata, int idSala)
        {
            string sql = $@"update tavolata set id_sala={idSala} where id_tavolata={idTavolata}";
            if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
            SqlCommand comm = new SqlCommand(sql, _conn);
            comm.ExecuteNonQuery();
            _conn.Close();
            return Ok();
        }
        [HttpPost("setOperatore")]
        public IActionResult SetOperatore(int idTavolata, string Operatore)
        {
            string sql = $@"update tavolata set id_operatore='{Operatore}' where id_tavolata={idTavolata}";
            if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
            SqlCommand comm = new SqlCommand(sql, _conn);
            comm.ExecuteNonQuery();
            _conn.Close();
            return Ok();
        }
        [HttpPost("demoteTavolo")]
        public IActionResult DemoteTavolo(int idTavolata)
        {
            string sql = $@"update tavolata set stato='0' where id_tavolata={idTavolata}";
            if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
            SqlCommand comm = new SqlCommand(sql, _conn);
            comm.ExecuteNonQuery();
            _conn.Close();
            return Ok();
        }
        [HttpGet("getMenudettaglio")]
        public ActionResult<IEnumerable<Menudettaglio>> GetMenudettaglio(string idmenu)
        {
            var namemd = new List<Menudettaglio>();
            try
            {
                if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                SqlCommand comm = new SqlCommand($@"
                    select crea_menu.id_pietanza, pietanze.descrizione, pietanze.reparto, pietanze.id_tipo, crea_menu.num_alternanza 
                    from crea_menu 
                    join pietanze on crea_menu.id_pietanza=pietanze.id_pietanza 
                    where crea_menu.id_menu = '{idmenu.ToUpper().Trim()}' and pietanze.attivo=1 
                    order by pietanze.id_tipo", _conn);
                SqlDataReader myReader = comm.ExecuteReader();
                while (myReader.Read())
                {
                    Menudettaglio m = new Menudettaglio
                    {
                        Id_pietanza = myReader[0].ToString().Trim(),
                        Descrizione = myReader[1].ToString().Trim(),
                        Reparto = myReader[2].ToString().Trim(),
                        Id_tipo = int.Parse(myReader[3].ToString().Trim()),
                        Num_alternanza = myReader[4].ToString().Trim()
                    };
                    namemd.Add(m);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                _conn.Close();
            }
            return Ok(namemd);
        }
        [HttpGet("getTipiPietanze")]
        public ActionResult<List<TipiPietanze>> GetPulsantiPietanze()
        {
            List<TipiPietanze> list = new List<TipiPietanze>();

            db db = new db();
            string sql = $@" select * from tipi_pietanze where (select count(*) from pietanze where attivo=1 and pietanze.id_tipo = tipi_pietanze.id_tipo) >= 1 order by descrizione";
            SqlDataReader r = db.getReader(sql);
          
            while (r.Read())
            {
                list.Add(new  TipiPietanze(r.GetInt32(0)));
            }
           
            return Ok(list);
        }
        [HttpGet("getPietanzeByTipo")]
        public ActionResult<IEnumerable<Pietanza>> GetPietanzeByTipo(int idTipo)
        {
            List<Pietanza> lst = new List<Pietanza>();
            db db = new db();
            string sql = $"select id_pietanza from pietanze where id_tipo = {idTipo} and attivo=1 order by descrizione";
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                Pietanza p = new Pietanza(r[0].ToString());
                lst.Add(p);
            }
            return Ok(lst);
        }
        [HttpGet("getOrdini")]
        public ActionResult<Ordine[]> getOrdiniByTavolata(int idTavolata)
        {
            List<Ordine> ret = new List<Ordine>();
            string sql = "select * from ordini where id_tavolata=" + idTavolata;
            db db = new db();
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                Ordine p = new Ordine();
                p.Id_ordine = (int)r[0];
                p.Id_tavolata = (int)r[1];
                if (string.IsNullOrEmpty(r[2].ToString())) { p.Id_voce = r[5].ToString(); p.Voce = new Menu(p.Id_voce);   } else { p.Id_voce = r[2].ToString(); p.Voce = new Pietanza(p.Id_voce); }
                p.Note_pietanza = r[3].ToString();
                p.Quantita = (int)r[4];
                if(r[6] != null && IsNumeric(r[6].ToString())) p.Comanda = new Comanda(Convert.ToInt32(r[6]));
                ret.Add(p);

            }
            db.Dispose();
            return Ok(ret);
        }
        private bool IsNumeric(string input)
        {
            return double.TryParse(input, out _);
        }
        [HttpDelete("deleteOrdine/{idOrdine}")]
        public ActionResult<bool> deleteOrdine(int idOrdine)
        {
            string sql = "delete ordini where id_ordine=" + idOrdine;
            if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
            SqlCommand comm = new SqlCommand(sql, _conn);
            comm.ExecuteNonQuery();
            _conn.Close();
            return Ok(true);
        }
        [HttpPost("setOrdine")]
        public ActionResult<int> SetOrdine([FromBody] Ordine ordine)
        {
            int id_ordine = -1;
            //int statotavolo = CheckStatoTavolo(id_tavolata);
            //db dbc = new db();
            string sqlordini = $"insert into ordini (id_tavolata, id_pietanza, quantita, note_pietanza, stato) " +
                $"values ({ordine.Id_tavolata}, '{ordine.Id_voce}', {ordine.Quantita}, '{ordine.Note_pietanza}', '{ordine.Stato}');SELECT SCOPE_IDENTITY();";
            if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
            if (ordine.Id_voce.StartsWith("M"))
            {
                sqlordini = $"insert into ordini (id_tavolata, id_menu, quantita, note_pietanza) " +
                    $"values ({ordine.Id_tavolata}, '{ordine.Id_voce}', {ordine.Quantita}, '{ordine.Note_pietanza}');SELECT SCOPE_IDENTITY();";
                /*
                SqlDataReader r1 = dbc.getReader($"select id_ordine from ordini where id_tavolata={ordine.Id_tavolata} and id_menu='{ordine.Id_voce}'");
                int id_ordine = -1;
                if (r1.HasRows)
                {
                    r1.Read();
                    id_ordine = r1.GetInt32(0);
                    dbc.Dispose();
                }
                if (id_ordine > -1)
                {
                    sqlordini = $"update ordini set quantita={ordine.Quantita} where id_ordine={id_ordine}";
                }
                */
            }
            SqlCommand comm = new SqlCommand(sqlordini, _conn);
            object ret = comm.ExecuteScalar();
            if (ret != null)
            {
                id_ordine = Convert.ToInt32( ret);
            } 
            _conn.Close();
            db db = new db();
            db.getReader($"update tavolata set stato ='2' where id_tavolata ={ordine.Id_tavolata} "); //imposto il tavolo in servizio, se non lo è già
            db.Dispose();
            return Ok(id_ordine);
        }
        [HttpPost("updateOrdine")]
        public ActionResult<int> UpdateOrdine([FromBody] Ordine ordine)
        {
            
            string sqlordini = $"update ordini set quantita = {ordine.Quantita}," +
                $" note_pietanza = '{ordine.Note_pietanza.Replace("'","''")}', " +
                $" stato= '{ordine.Stato}' " +
                $" where  id_ordine={ordine.Id_ordine}";
            if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
            
            SqlCommand comm = new SqlCommand(sqlordini, _conn);
            int ret = comm.ExecuteNonQuery();
            
            _conn.Close();
            return Ok(ret);
        }
        /// <summary>
        /// [Vecchio] aggiorna i dati dell'ordine
        /// </summary>
        /// <param name="idOrdine"></param>
        /// <param name="quantita"></param>
        /// <param name="delete"></param>
        /// <returns></returns>
        [HttpPost("salvaOrdine"), Obsolete]
        public IActionResult SalvaOrdine(int idOrdine, int quantita, bool delete)
        {
            string sql;
            if (delete)
            {
                sql = "delete ordini where id_ordine=" + idOrdine.ToString();
            }
            else
            {
                sql = "update ordini set quantita=" + quantita.ToString() + " where id_ordine=" + idOrdine.ToString();
            }
            db db = new db();
            db.getReader(sql);
            db.Dispose();
            return Ok();
        }
        private int CheckStatoTavolo(int idTsavolo)
        {
            int ret = -1;
            db db = new db();
            SqlDataReader r = db.getReader("select stato from tavolata where id_tavolata=" + idTsavolo.ToString());
            if (r.HasRows)
            {
                r.Read();
                ret = int.Parse(r[0].ToString());
            }
            db.Dispose();
            return ret;
        }
        [HttpPost("setTavolata")]
        public IActionResult SetTavolata(string nome, int adulti, int bambini, int id_sala)
        {
            int max;
            string ora = "";
            if (DateTime.Now.Hour >= 10 && DateTime.Now.Hour < 19) ora = "12:00"; else ora = "19:00";
            if (ora == "12:00") ora = $"convert(datetime, '{DateTime.Now.ToShortDateString()} 12:00' , 103)";
            else ora = $"convert(datetime, '{DateTime.Now.ToShortDateString()} 19:00', 103)";
            db db = new db();
            SqlDataReader r = db.getReader($"select max(id_tavolata) from tavolata");
            r.Read();
            max = (int)r[0] + 1;
            db.CloseReader();
            SqlDataReader s = db.getReader($"insert into tavolata (id_tavolata, descrizione, data_ora_arrivo, adulti, bambini, id_sala, stato, id_cliente) values ({max}, '{nome.Replace("'", "''")}', {ora}, {adulti}, {bambini}, {id_sala}, '1', 172)");
            db.Dispose();
            return Ok();
        }
        [HttpGet("getComande")]
        public ActionResult<Comanda[]> getComande(int id_tavolata) {
            db db = new db();
            List<Comanda> comande = new List<Comanda>();
            string sql = "select id_comanda from comande where id_tavolata=" + id_tavolata;
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                Comanda c = new Comanda(r.GetInt32(0));
                comande.Add(c);
            }
            db.CloseReader();
            db.Dispose();
            GC.Collect();
            return Ok(comande.ToArray());
        }
        [HttpPost("setComanda")]
        public ActionResult<int> SetComanda([FromBody] Comanda comanda)
        {
            string VariazioneAllaPietanza = "";
            VariazioneAllaPietanza = comanda.Variazioni?.Replace("'", "''").Trim();

            string SqlIdOrd = comanda.id_ordine == null ? "null" : comanda.id_ordine.ToString();
            string sqlcomande = $"insert into comande (id_tavolata, id_pietanza, quantita, variazioni, ora_comanda, stato, id_ordine) values ({comanda.Id_tavolata}, " +
                $"'{comanda.Id_pietanza}', {comanda.Quantita}, '{VariazioneAllaPietanza}', SYSDATETIME(), '{comanda.Stato}', {SqlIdOrd});SELECT SCOPE_IDENTITY();";
            if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
            SqlCommand comm = new SqlCommand(sqlcomande, _conn);
            int idComanda= Convert.ToInt32( comm.ExecuteScalar());
            GC.Collect();
            _conn.Close();
            return Ok(idComanda);
        }
        [HttpPost("updateComanda")]
        public ActionResult<bool> UpdateComanda([FromBody] Comanda comanda)
        {
            try
            {
                string VariazioneAllaPietanza = "";
                string sql_ora_stampa = comanda.Stato == "stampata" ? ",ora_stampa=sysdatetime() " : " ";
                //Pietanza p = GetPietanza(comanda.Id_pietanza).Value;
                VariazioneAllaPietanza = comanda.Variazioni.Replace("'", "''").Trim();

                string sqlcomande = $"update comande set quantita={comanda.Quantita}, " +
                    $"variazioni='{VariazioneAllaPietanza}', " +
                    $"stato = '{comanda.Stato}' " +
                    sql_ora_stampa +
                    $"where id_comanda = {comanda.Id_comanda}";
                if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                SqlCommand comm = new SqlCommand(sqlcomande, _conn);
                bool result = Convert.ToBoolean(comm.ExecuteNonQuery());
                GC.Collect();
                _conn.Close();
                return Ok(result);
            }
            catch (Exception ex) {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpDelete("deleteComanda/{idComanda}")]
        public ActionResult<bool> deleteComanda(int idComanda)
        {
            string sql = "delete comande where id_comanda=" + idComanda;
            if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
            SqlCommand comm = new SqlCommand(sql, _conn);
            comm.ExecuteNonQuery();
            _conn.Close();
            return Ok(true);
        }
        [HttpPost("stampaOrdine")] //USATA
        public async Task<IActionResult> StampaOrdine(List<Comanda> listaOrigine, string oldStato, int idOperatore)
        {
            //stampaComande( List<Comande> listaOrigine, string oldStato)
            // STATO "inviato" oppure "ristampa"


            List<Comande> list= new List<Comande>();
            List<string> lista = new List<string>();
            foreach (Comanda comanda in listaOrigine) 
            {
                Tavolata t = new Tavolata(comanda.Id_tavolata);
                Comande c=new Comande(comanda.Id_comanda);
                if(c.IdComande == -1)
                {
                    logEventi log = new logEventi();
                    log.Scrivi("Errore nella Stampaordine perche ID comande è negativo, listaorigine contiene " + listaOrigine.Count + " elementi e oldstato è " + oldStato);
                    return StatusCode(500, "Errore ID comanda negativo perche non trovato nel DB passato" + comanda.Id_comanda);
                }
                c.DescrizioneTavolata = t.Descrizione;
                c.DescrizionPietanza = comanda.Pietanza.Descrizione;
                c.Operatore = "oper";
                c.Sala = "coperto";
                list.Add(c);
                lista.Add(comanda.Id_comanda.ToString());
                
            }
           

            try
            {
                 string url = _client.BaseAddress + $"/stampaComande?listaID={string.Join(',',lista)}&oldStato={oldStato}&idOperatore={idOperatore}";

                // Execute the HTTP GET request
                HttpResponseMessage response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return Ok("Richiesta inviata con successo.");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Errore durante l'invio della richiesta: {responseContent}");
                }

               
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request exceptions
                return StatusCode(500, $"Errore durante la chiamata al servizio ASMX: {ex.Message}");
            }

            
        }
        [HttpGet("StampaContoTavolo")] //USATA
        public async Task<IActionResult> StampaContoTavolo(string idTavolo)
        {
            try
            {
                // Costruisci l'URL completo per il metodo StampaContoTavolo del servizio ASMX
                string url = _client.BaseAddress + $"/StampaContoTavolo?idTavolo={idTavolo}";

                // Esegui la chiamata HTTP GET (o POST a seconda del metodo del servizio ASMX)
                HttpResponseMessage response = await _client.GetAsync(url);

                // Controlla se la richiesta ha avuto successo
                response.EnsureSuccessStatusCode();

                // Leggi la risposta come stringa
                string responseBody = await response.Content.ReadAsStringAsync();

                // Ritorna la risposta come contenuto della richiesta HTTP
                return Ok(responseBody);
            }
            catch (HttpRequestException ex)
            {
                // Gestisci le eccezioni di HTTP request
                return StatusCode(500, $"Errore durante la chiamata al servizio ASMX: {ex.Message}");
            }
        }
        [HttpPost("StampaPreContoTavolo")]
        public async Task<IActionResult> StampaPreContoTavolo(string idTavolo)
        {
              try
            {
                // Costruisci l'URL completo per il metodo StampaContoTavolo del servizio ASMX
                string url = _client.BaseAddress + $"/StampaPreContoTavolo?idTavolo={idTavolo}";

                // Esegui la chiamata HTTP GET (o POST a seconda del metodo del servizio ASMX)
                HttpResponseMessage response = await _client.GetAsync(url);

                // Controlla se la richiesta ha avuto successo
                response.EnsureSuccessStatusCode();

                // Leggi la risposta come stringa
                string responseBody = await response.Content.ReadAsStringAsync();

                // Ritorna la risposta come contenuto della richiesta HTTP
                return Ok(responseBody);
            }
            catch (HttpRequestException ex)
            {
                // Gestisci le eccezioni di HTTP request
                return StatusCode(500, $"Errore durante la chiamata al servizio ASMX: {ex.Message}");
            }
        }
        [HttpGet("getOperatorebyNomeandById")] //USATA
        public ActionResult<Operatori> getOperatorebyNomeandById(string nominativo, string pin)
        {
            Operatori? op = Operatori.Create(nominativo, pin);
            if (op == null)
            {
                return NotFound("Credenziali non valide o Operatore non Attivo");
            }
            return op;
        }

        [HttpPost("hasExtra")]
        public ActionResult<bool[]> HasExtra([FromBody] int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return BadRequest("Nessun ID fornito.");
            }

            try
            {
                List<bool> results = new List<bool>();
                foreach (int id in ids)
                {
                    results.Add(Classi.Tavolata.checkExtra(id));
                }

                // Converte la lista in un array per la restituzione
                return Ok(results.ToArray());
            }
            catch (Exception ex)
            {
                // Gestione degli errori
                return StatusCode(500, $"Errore durante il controllo degli extra: {ex.Message}");
            }
        }

        /*SEZIONE RELATIVA ALLA TABELLA CLIENTI*/

        /*METODI USATI IN:
         - APP GESTORE 
        */

        [HttpGet("getClientebyTelefono")]
        public ActionResult<string> getClientebyTelefono(string telefono)
        {
           return Ok(Cliente.checkTelefono(telefono));  
        }
        [HttpGet("insertCliente")]
        public ActionResult<int> insertCliente(string nome, string cognome, string telefono)
        {
            Cliente c=new Cliente();
            string id_cliente=Cliente.checkTelefono(telefono); // PER EVITARE INSERIMENTO NUMERO DUPLICATO
            if (id_cliente=="NON TROVATO") 
            {
                Cliente cliente = new Cliente();
                cliente.Nome = nome.ToUpper();
                cliente.Cognome = cognome.ToUpper();
                cliente.Telefono = telefono;
                cliente.Attivo = 1;
                cliente.Data_reg = DateTime.Now;
                cliente.Note = "Inserito da App Gestori";
                cliente.Email = "";
                int id = Cliente.insert(cliente);
                return Ok(id);
            }
            else return Convert.ToInt32(id_cliente);
           
        }

        [HttpGet("getClientebyID")]
        public ActionResult<Cliente> getClientebyID(int id)
        {
            return Ok(new Cliente(id));
        }

        [HttpGet("getAllCliente")]
        public ActionResult<Cliente> getAllCliente(string filtro)
        {
            List<Cliente> list = new List<Cliente>();   
            list=Cliente.GetAllCliente(filtro);
            return Ok(list);
        }

        [HttpGet("getTopTavolate")]
        public ActionResult<ClienteMini> getTopTavolate()
        {
            List<ClienteMini> list = new List<ClienteMini>();
            list = ClienteMini.GetTopTavolate();
            return Ok(list);
        }
        [HttpGet("getTopConto")]
        public ActionResult<ClienteMini> getTopConto()
        {
            List<ClienteMini> list = new List<ClienteMini>();
            list = ClienteMini.GetTopConto();
            return Ok(list);
        }

        /*SEZIONE RELATIVA ALLA TABELLA SPESA*/
        /*USATA IN App Gestori         */

        [HttpGet("getSpeseALL")] // usata app Gestore
        public ActionResult<IEnumerable<Spesa>> getSpeseALL()
        {
            List<Spesa> list = new List<Spesa>();
            list = Spesa.getAll();
            return Ok(list);
        }
        [HttpPost("updateSpesa")]
        public ActionResult<bool> updateSpesa([FromBody] Spesa spesa)
        {
            try 
            { 
                spesa.update();
                return Ok(true);
            }
            catch 
            {
                return Ok(false);
            }

      
        }
        [HttpDelete("deleteSpesa/{idSpesa}")]
        public ActionResult<bool> deleteSpesa(int idSpesa)
        {
            Spesa s = new Spesa(idSpesa);
            try
            {
                s.delete();
                return Ok(true);
            }
            catch 
            {
                return Ok(false) ;
            }
        }

        [HttpPost("insertSpesa")] // usata app Gestore
        public IActionResult insertSpesa([FromBody] Spesa s)
        {
            try
            {
                s.insert();
                return Ok();
            }
            catch 
            {
                return Ok(false);
            }
        }

        /* SEZIONE RELATIVA ALLA TABELLA PRESTAZIONI_EXTRA*/

        [HttpGet("getPrestazioniExtrabyTavolata")] // usata app Gestore
        public ActionResult<IEnumerable<Prestazioni_extra>> getPrestazioniExtrabyTavolata(int id)
        {
            List<Prestazioni_extra> list = new List<Prestazioni_extra>();
            list = Prestazioni_extra.getAll($" idtavolata={id}");
            return Ok(list);
        }
        [HttpPost("updatePrestazioniExtra")]
        public ActionResult<bool> updatePrestazioniExtra([FromBody] Prestazioni_extra pe)
        {
            try
            {
                pe.update();
                return Ok(true);
            }
            catch
            {
                return Ok(false);
            }


        }
        [HttpDelete("deletePrestazioneExtra/{id}")]
        public ActionResult<bool> deletePrestazioneExtra(int id)
        {
           Prestazioni_extra pe = new Prestazioni_extra(id);
            try
            {
                pe.delete();
                return Ok(true);
            }
            catch
            {
                return Ok(false);
            }
        }

        [HttpPost("insertPrestazioneExtra")] // usata app Gestore
        public IActionResult insertPrestazioneExtra([FromBody] Prestazioni_extra pe)
        {
            try
            {
                pe.insert();
                return Ok();
            }
            catch
            {
                return Ok(false);
            }
        }
    }
}
