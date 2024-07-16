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
            _client.BaseAddress = new Uri("http://carboasmx.carbolandia.local:81/mioserv.asmx"); 
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
            }
            catch (Exception ex)
            {
                // Handle exception
                return StatusCode(500, ex.Message);
            }
            finally
            {
                _conn.Close();
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
            }
            catch (Exception ex)
            {
                // Handle exception
                return StatusCode(500, ex.Message);
            }
            finally
            {
                _conn.Close();
            }
            return Ok(menus);
        }
       
        [HttpGet("getMenuByIdTavolo"), Obsolete("Usiamo getMenu?")]
        public ActionResult<IEnumerable<Menu>> GetMenuByIdTavolo(int id_tavolo)
        {
            var namem = new List<Menu>();
            try
            {
                if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                SqlCommand comm = new SqlCommand($@"
                    with t as (
                        select ordini.id_menu, quantita
                        from ordini join menu 
                        on menu.id_menu=ordini.id_menu
                        where id_tavolata={id_tavolo})
                    select a.id_menu, a.descrizione, a.tipo, a.prezzo, a.occasione, a.stato, isnull(t.quantita, 0) 
                    from menu a  
                    full outer join t on t.id_menu = a.id_menu  
                    where stato ='True' 
                    order by descrizione", _conn);
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
                        Stato = bool.Parse(myReader[5].ToString()),
                       // QuantitaOrdinata = myReader.GetInt32(6)
                    };
                    namem.Add(m);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return StatusCode(500, ex.Message);
            }
            finally
            {
                _conn.Close();
            }
            return Ok(namem);
        }
        
        [HttpGet("getTavolateByIdOperatore"), Obsolete("Se non erro viene usato getTavolateOdierne")]
        public ActionResult<IEnumerable<Tavolata>> GetTavolateByIdOperatore(int id_operatore)
        {
            List<Tavolata> list = new List<Tavolata>();
            list=Tavolata.GetTavolateByIdOperatore(id_operatore);
            return Ok(list);
        }
        [HttpGet("getTavolateOdierne")]
        public ActionResult<IEnumerable<Tavolata>> GetTavolateOdierne()
        {
            List<Tavolata> list = new List<Tavolata>();
            list = Tavolata.GetTavolateOdierne();
            return Ok(list);
        }
        
        [HttpGet("getTestataConto"), Obsolete("Non credo che usiamo ancora questo per avere il conto.")]
        public ActionResult<TestaConto> GetTestataConto(int idtavolata)
        {
            TestaConto tc = new TestaConto();
            try
            {
                if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                SqlCommand comm = new SqlCommand($@"
                    select tavolata.descrizione, tavolata.data_ora_arrivo, tavolata.acconto, tavolata.sconto, 
                    sale.descrizione, operatori.nominativo 
                    from tavolata 
                    left join sale on tavolata.id_sala = sale.id_sala 
                    left join operatori on tavolata.id_operatore = operatori.id_operatore 
                    where tavolata.id_tavolata ={idtavolata}", _conn);
                SqlDataReader myReader = comm.ExecuteReader();
                while (myReader.Read())
                {
                    tc.IdTavolata = idtavolata;
                    tc.Tavolata = myReader[0].ToString();
                    tc.Data_ora = myReader[1].ToString();
                    tc.Acconto = float.Parse(myReader[2].ToString());
                    tc.Sconto = float.Parse(myReader[3].ToString());
                    tc.Sala = myReader[4].ToString();
                    tc.Cameriere = myReader[5].ToString();
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return StatusCode(500, ex.Message);
            }
            finally
            {
                _conn.Close();
            }
            return Ok(tc);
        }
        [HttpPost("setExtra"), 
            Obsolete("Ora la logica dovrebbe essere qualle di usare updateComanda, però questo compila una tabella a parte!")
            ]

        public IActionResult SetExtra(string id_pietanza, int quantita, float prezzo)
        {
            string sqlextra = $@"insert into extra (data, id_pietanza, prezzo, quantita) 
                                 values ('{DateTime.Now}', '{id_pietanza}', {prezzo}, {quantita})";
            if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
            SqlCommand comm = new SqlCommand(sqlextra, _conn);
            comm.ExecuteNonQuery();
            _conn.Close();
            return Ok();
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
                    select DATA_ORA=extra.data, DESCRIZIONE1=pietanze.descrizione, DESCRIZIONE2='SENZA TAVOLO',extra.prezzo, extra.quantita, ACCONTO=0, SCONTO=0,TOTALE=extra.prezzo*extra.quantita
                    from extra 
                    inner join pietanze on extra.id_pietanza=pietanze.id_pietanza 
                    where (extra.data BETWEEN convert(datetime, '{data2} 09:00' , 103) and dateadd(day,1,convert(datetime, '{data2} 04:00' , 103)))
                    union 
                    select DATA_ORA=tavolata.data_ora_arrivo, DESCRIZIONE1=pietanze.descrizione, DESCRIZIONE2='Tavolo '+tavolata.descrizione, pietanze.prezzo, ordini.quantita,ACCONTO=0, SCONTO=0, TOTALE=ordini.quantita*pietanze.prezzo
                    from tavolata 
                    join ordini on tavolata.id_tavolata=ordini.id_tavolata 
                    join pietanze on pietanze.id_pietanza=ordini.id_pietanza 
                    where (tavolata.data_ora_arrivo BETWEEN convert(datetime, '{data2} 09:00' , 103) and dateadd(day,1,convert(datetime, '{data2} 04:00' , 103))) and tavolata.stato = 3
                    union
                    select DATA_ORA=tavolata.data_ora_arrivo, DESCRIZIONE1='Tavolo '+tavolata.descrizione, DESCRIZIONE2=menu.descrizione, menu.prezzo,ordini.quantita,ACCONTO=0, SCONTO=0, TOTALE=ordini.quantita*menu.prezzo
                    from tavolata 
                    join ordini on tavolata.id_tavolata=ordini.id_tavolata 
                    join menu on menu.id_menu=ordini.id_menu 
                    where (tavolata.data_ora_arrivo BETWEEN convert(datetime, '{data2} 09:00' , 103) and dateadd(day,1,convert(datetime, '{data2} 04:00' , 103))) and tavolata.stato = 3
                    union 
                    select DATA_ORA=tavolata.data_ora_arrivo, DESCRIZIONE1=prestazioni_extra.descrizione,DESCRIZIONE2='Tavolo '+tavolata.descrizione,prestazioni_extra.prezzo, quantita=1, ACCONTO=0, SCONTO=0,TOTALE=prestazioni_extra.prezzo
                    from tavolata 
                    join prestazioni_extra on prestazioni_extra.idTavolata=tavolata.id_tavolata
                    where (tavolata.data_ora_arrivo BETWEEN convert(datetime, '{data2} 09:00' , 103) and dateadd(day,1,convert(datetime, '{data2} 04:00' , 103))) and tavolata.stato = 3
                    union 
                    select DATA_ORA=tavolata.data_ora_arrivo,DESCRIZIONE1='Tavolo '+tavolata.descrizione, DESCRIZIONE2='ACCONTO & SCONTO',prezzo=0,quantita=1,ACCONTO=tavolata.acconto,SCONTO=tavolata.sconto,TOTALE=0
                    from tavolata 
                    where (tavolata.data_ora_arrivo BETWEEN convert(datetime, '{data2} 09:00' , 103) and dateadd(day,1,convert(datetime, '{data2} 04:00' , 103))) and tavolata.stato = 3 and (acconto <> 0 or sconto <> 0)
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
        [HttpGet("getDettaglioContoTavolo")]
        public ActionResult<string> GetDettaglioContoTavolo(int idTavolo)
        {
            string html = "<table id='dettaglioConto'>";
            string sqlDettaglioTavolo = $@"
                select TIPO=convert(varchar,ordini.id_ordine), ID=ordini.id_pietanza, MENU_Portata=pietanze.descrizione, Quantita=quantita, Prezzo_Unitario=pietanze.prezzo, TOTALE=quantita*pietanze.prezzo 
                from ordini 
                join pietanze on ordini.id_pietanza=pietanze.id_pietanza 
                where id_tavolata={idTavolo}
                union 
                select convert(varchar,ordini.id_ordine), ordini.id_menu,menu.descrizione, quantita, menu.prezzo, quantita*menu.prezzo
                from ordini 
                join menu on menu.id_menu=ordini.id_menu
                where id_tavolata={idTavolo}
                union
                select 'E',convert(varchar(10),prestazioni_extra.ID),prestazioni_extra.descrizione, quantita=1, prestazioni_extra.prezzo, prestazioni_extra.prezzo 
                from prestazioni_extra 
                join tavolata on prestazioni_extra.idTavolata=tavolata.id_tavolata 
                where id_tavolata={idTavolo}";
            string sqlAcconto = $@"
                select tavolata.acconto, isnull(tavolata.sconto,0) 
                from tavolata 
                left join sale on tavolata.id_sala = sale.id_sala
                left join operatori on tavolata.id_operatore = operatori.id_operatore 
                where tavolata.id_tavolata ={idTavolo}";
            db db = new db();
            decimal acconto = 0; decimal sconto = 0;
            SqlDataReader r = db.getReader(sqlAcconto);
            r.Read(); acconto = r.GetSqlMoney(0).ToDecimal(); sconto = r.GetSqlMoney(1).ToDecimal();
            db.CloseReader();
            r = db.getReader(sqlDettaglioTavolo);
            while (r.Read())
            {
                string id_comanda = r[0].ToString();
                string disabledPrezzo = "disabled='disabled'";
                string disabledQuantita = "";
                if (r[0].ToString() == "E") { id_comanda = "Ex" + r[1].ToString(); disabledPrezzo = ""; disabledQuantita = "disabled='disabled'"; }
                html += $@"
                    <tr>
                        <td>{r[2].ToString()}</td>
                        <td><input type='text' id='txt_q_{id_comanda}' value='{r[3].ToString()}' {disabledQuantita} /></td>
                        <td><input type='text' id='txt_p_{id_comanda}' value='{r[4].ToString()}' {disabledPrezzo} /></td>
                        <td>{r[5].ToString()}</td>
                        <td><input type='button' id='btn_s_{id_comanda}' value='Salva' onclick='SalvaComanda(\""{id_comanda}\"");' /></td>
                        < td >< input type = 'button' id = 'btn_e_{id_comanda}' value = 'Elimina' onclick = 'EliminaComanda(\""{id_comanda}\"");' /></ td >
                    </ tr > ";
            }
            html += $@"
                <tr>
                    <td colspan='3'>Acconto:<input type='text' id='txt_acc_{idTavolo}' value='{acconto}' /></td>
                    <td colspan='3'>Sconto:<input type='text' id='txt_sco_{idTavolo}' value='{sconto}' /></td>
                </tr>";
            html += "<tr><td colspan='6' align='center'><input type='button' id='btn_salvatutto' value='Salva e Esci' onclick='salvaesci(\"" + idTavolo.ToString() + "\");'/></td></tr>";
            html += "</table>";
            db.Dispose();
            return Ok(html);
        }
        [HttpGet("getTotaleContoTavolo")]
        public ActionResult<string> GetTotaleContoTavolo(int idtavolo)
        {
            string sql = $@"
                with conto_dare as (
                    select TIPO=convert(varchar,ordini.id_ordine), 
                        ID=ordini.id_pietanza, 
                        MENU_Portata=pietanze.descrizione, 
                        Quantita=quantita, 
                        Prezzo_Unitario=pietanze.prezzo, 
                        TOTALE=quantita*pietanze.prezzo
                    from ordini 
                    join pietanze on ordini.id_pietanza=pietanze.id_pietanza 
                    where id_tavolata={idtavolo}
                    union 
                    select convert(varchar,ordini.id_ordine), ordini.id_menu,menu.descrizione, quantita, menu.prezzo, quantita*menu.prezzo
                    from ordini 
                    join menu on menu.id_menu=ordini.id_menu
                    where id_tavolata={idtavolo}
                    union
                    select 'E',convert(varchar(10),prestazioni_extra.ID),prestazioni_extra.descrizione, quantita=1, prestazioni_extra.prezzo, prestazioni_extra.prezzo 
                    from prestazioni_extra 
                    join tavolata on prestazioni_extra.idTavolata=tavolata.id_tavolata 
                    where id_tavolata={idtavolo}),  
                conto_avere as (
                    select tavolata.acconto, isnull(tavolata.sconto,0) as sconto  
                    from tavolata 
                    left join sale on tavolata.id_sala = sale.id_sala
                    left join operatori on tavolata.id_operatore = operatori.id_operatore 
                    where tavolata.id_tavolata ={idtavolo}), 
                totali as (
                    select Sum(a.TOTALE) as Totale, b.acconto, b.sconto 
                    from conto_dare a, conto_avere b  
                    group by b.acconto, b.sconto)
                select Totale - sconto - acconto from totali";
            db db = new db();
            SqlDataReader r = db.getReader(sql);
            r.Read();
            Decimal result = r.GetDecimal(0);
            db.Dispose();
            return Ok(string.Format("{0:0.00}", result));
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
                                                '{ta.Descrizione}'
                                                '{ta.Note}',    
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
                                                '{t.Descrizione}',
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
        [HttpPost("salvaPrestazioneExtra"), Obsolete]
        public IActionResult SalvaPrestazioneExtra(int IdPrestazione, string prezzo, bool delete)
        {
            string sql;
            if (delete)
            {
                sql = "delete prestazioni_extra where ID=" + IdPrestazione.ToString();
            }
            else
            {
                sql = "update prestazioni_extra set prezzo=" + prezzo.Replace(",", ".") + " where ID=" + IdPrestazione.ToString();
            }
            db db = new db();
            db.getReader(sql);
            db.Dispose();
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
        [HttpGet("getSaleHtml"), Obsolete]
        public ActionResult<string> GetSaleHtml()
        {
            db db = new db();
            string html = "";
            string sql = "select * from sale order by descrizione";
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                int coperti = CopertiImpegnatiBySala(r.GetInt32(0));
                html += $"<label><input type='radio' id='chk_{r[0].ToString()}' name='sale' onchange='setIdSala(this.id);' />{r[1].ToString()} ({coperti.ToString()}/{r[2].ToString()})</label> ";
            }
            db.Dispose();
            return Ok(html);
        }
        private int CopertiImpegnatiBySala(int idsala)
        {
            db db = new db();
            string sqlOra12 = $" and Datepart(HOUR, data_ora_arrivo) =12";
            string sqlOra19 = $" and Datepart(HOUR, data_ora_arrivo) =19";
            string ora = "";
            if (DateTime.Now.Hour >= 10 && DateTime.Now.Hour < 19) ora = "12:00"; else ora = "19:00";
            string sqlcoperti = $@"
                select isnull(sum(tavolata.adulti) + sum(tavolata.bambini),0) 
                from tavolata 	                       
                where SYSDATETIME() BETWEEN tavolata.data_ora_arrivo and dateadd(day, 1, tavolata.data_ora_arrivo) and id_sala={idsala.ToString()}";
            if (ora.Contains("12")) sqlcoperti += sqlOra12; else sqlcoperti += sqlOra19;
            SqlDataReader rc = db.getReader(sqlcoperti);
            rc.Read();
            int coperti = rc.GetInt32(0);
            db.Dispose();
            return coperti;
        }
        [HttpPost("setPagato"), Obsolete]
        public IActionResult SetPagato(int idtavolo, double importoContanti, double importoPOS)
        {
            db db = new db();
            string sql_ricerca = $"select * from pagamenti where id_tavolata = {idtavolo}";
            SqlDataReader r = db.getReader(sql_ricerca);
            string sql_cassa = "";
            if (r.HasRows)
            {
                sql_cassa = $"update pagamenti set conto_pos={importoPOS}, conto_contanti={importoContanti} where id_tavolata={idtavolo}";
            }
            else
            {
                sql_cassa = $@"
                    INSERT INTO [dbo].[pagamenti]
                    ([data_ora_registrazione], [id_tavolata], [conto_pos], [conto_contanti], [conto_altro])
                    VALUES (convert(datetime, '{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}', 103), {idtavolo}, {importoPOS.ToString().Replace(",", ".")}, {importoContanti.ToString().Replace(",", ".")}, 0.0)";
            }
            string sql = $"update tavolata set stato='3', data_ora_conto=getdate() where id_tavolata = {idtavolo}";
            db.CloseReader();
            db.Esegui(sql_cassa);
            db.Esegui(sql);
            db.Dispose();
            return Ok();
        }
        [HttpGet("creaPopupOperatori"), Obsolete]
        public ActionResult<string> CreaPopupOperatori()
        {
            string html = $@"
                <ul data-role='listview' id='lst_operatori' data-inset='true'>
                    <li data-role='list-divider'>Seleziona Operatore</li>";
            db db = new db();
            string sql = "select id_operatore, nominativo from operatori order by nominativo";
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                html += $"<li><input type='button' id='op_{r[0].ToString()}' value=\"{r[1].ToString()}\" onclick='cambiaOperatore(this.id);' /></li>";
            }
            html += "</ul>";
            db.Dispose();
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
        [HttpGet("getCorpoConto"), Obsolete]
        public ActionResult<IEnumerable<CorpoConto>> GetCorpoConto(int idtavolata)
        {
            var conto = new List<CorpoConto>();
            try
            {
                if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                SqlCommand comm = new SqlCommand($@"
                    select ID=ordini.id_pietanza, MENU_Portata=pietanze.descrizione, Quantita=quantita, Prezzo_Unitario=pietanze.prezzo, TOTALE=quantita*pietanze.prezzo 
                    from ordini 
                    join pietanze on ordini.id_pietanza=pietanze.id_pietanza 
                    where id_tavolata={idtavolata} 
                    union 
                    select ordini.id_menu,menu.descrizione, quantita, menu.prezzo, quantita*menu.prezzo 
                    from ordini 
                    join menu on menu.id_menu=ordini.id_menu 
                    where id_tavolata={idtavolata} 
                    union 
                    select convert(varchar(10),prestazioni_extra.ID),prestazioni_extra.descrizione, quantita=1, prestazioni_extra.prezzo, prestazioni_extra.prezzo 
                    from prestazioni_extra 
                    join tavolata on prestazioni_extra.idTavolata=tavolata.id_tavolata 
                    where id_tavolata={idtavolata}", _conn);
                SqlDataReader myReader = comm.ExecuteReader();
                float GT = 0;
                while (myReader.Read())
                {
                    CorpoConto cc = new CorpoConto
                    {
                        Id = myReader[0].ToString(),
                        Menu_portata = myReader[1].ToString(),
                        Quantita = float.Parse(myReader[2].ToString()),
                        Prezzo_unitario = float.Parse(myReader[3].ToString()),
                        Totale = float.Parse(myReader[4].ToString())
                    };
                    GT += cc.Totale;
                    cc.GranTotale = GT;
                    conto.Add(cc);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            _conn.Close();
            return Ok(conto);
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

        [HttpPost("setComande"), Obsolete("Questo invia una serie di comande ma non lo uso piu, uso setComanda (singola)")]
        public IActionResult SetComande([FromBody] List<Comande> comande)
        {
            foreach (Comande item in comande)
            {
                string VariazioneAllaPietanza = "";
                Pietanza p = GetPietanza(item.IdPietanza).Value;
                if (p.Descrizione.ToUpper().Trim() != item.Variazioni.ToUpper().Trim())
                {
                    VariazioneAllaPietanza = item.Variazioni.Replace("'", "''");
                }
                string sqlcomande = $"insert into comande (id_tavolata, id_pietanza, quantita, variazioni, ora_comanda) values ({item.IdTavolata}, '{item.IdPietanza}', {item.Quantita}, '{VariazioneAllaPietanza}', SYSDATETIME())";
                if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                SqlCommand comm = new SqlCommand(sqlcomande, _conn);
                comm.ExecuteNonQuery();
                if (item.IsExtra == 1)
                {
                    Ordine ordine = new Ordine(){ Id_tavolata=item.IdTavolata, Id_voce= item.IdPietanza, Quantita = item.Quantita, Note_pietanza = item.Variazioni  };
                    SetOrdine(ordine);
                }
                GC.Collect();
            }
            _conn.Close();
            return Ok();
        }

        [HttpGet("getPulsantiPerInvioInCucina"),Obsolete]
        public ActionResult<string> GetPulsantiPerInvioInCucina(int idTavolata)
        {
            db db = new db();
            string sql = $@"
                select tp.descrizione, tp.id_tipo
                from comande c,
                    tipi_pietanze tp,
                    pietanze p
                where c.id_tavolata = {idTavolata} 
                    and p.id_pietanza = c.id_pietanza
                    and tp.id_tipo = p.id_tipo
                    and c.stato = 'attesa'
                group by tp.descrizione, tp.id_tipo";
            SqlDataReader r = db.getReader(sql);
            string html = "<table style='overflow:auto;'><tr>";
            while (r.Read())
            {
                html += $"<td><input type='button' id='btn_t_{r[1].ToString()}' value='{r[0].ToString()}' onclick=\"CambiaStatoComande(this,'attesa')\" /></td>";
            }
            html += "</tr></table>";
            db.Dispose();
            return Ok(html);
        }
        
        [HttpPost("setStatoComanda"), Obsolete("inutile praticamente")]
        public async Task<IActionResult> SetStatoComandaAsync(string htmlButtonItemID, int idTavolata, string Stato, string oldStato)
        {
            // la stringa htmlButton distingue in qualche modo il TIPO PIETANZA
            // ad esempio se è "btn_t_9" si tratta di pietanza di tipo PIZZA

            //stampaComande( List<Comande> listaOrigine, string oldStato)
            try
            {
                    // Construct the URL with the required parameters
                    string url = _client.BaseAddress + $"/setStatoComanda?htmlButtonItemID={htmlButtonItemID}&idTavolata={idTavolata}&Stato={Stato}&oldStato={oldStato}";

                    // Execute the HTTP GET request
                    HttpResponseMessage response = await _client.GetAsync(url);

                    // Check if the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response as a string
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Return the response as the content of the HTTP request
                    return Ok(responseBody);
          }
          catch (HttpRequestException ex)
          {
                    // Handle HTTP request exceptions
                    return StatusCode(500, $"Errore durante la chiamata al servizio ASMX: {ex.Message}");
          }
        }
        private static void Print(string printerName, byte[] document)
        {
            NativeMethods.DOC_INFO_1 documentInfo;
            IntPtr printerHandle;
            documentInfo = new NativeMethods.DOC_INFO_1();
            documentInfo.pDataType = "RAW";
            documentInfo.pDocName = "Receipt";
            printerHandle = new IntPtr(0);
            if (NativeMethods.OpenPrinter(printerName.Normalize(), out printerHandle, IntPtr.Zero))
            {
                if (NativeMethods.StartDocPrinter(printerHandle, 1, documentInfo))
                {
                    int bytesWritten;
                    byte[] managedData;
                    IntPtr unmanagedData;
                    managedData = document;
                    unmanagedData = Marshal.AllocCoTaskMem(managedData.Length);
                    Marshal.Copy(managedData, 0, unmanagedData, managedData.Length);
                    if (NativeMethods.StartPagePrinter(printerHandle))
                    {
                        NativeMethods.WritePrinter(printerHandle, unmanagedData, managedData.Length, out bytesWritten);
                        NativeMethods.EndPagePrinter(printerHandle);
                    }
                    else
                    {
                        throw new Win32Exception();
                    }
                    Marshal.FreeCoTaskMem(unmanagedData);
                    NativeMethods.EndDocPrinter(printerHandle);
                }
                else
                {
                    throw new Win32Exception();
                }
                NativeMethods.ClosePrinter(printerHandle);
            }
            else
            {
                throw new Win32Exception();
            }
        }
        private byte[] GetDocument()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(ClassiStampa.AsciiControlChars.Escape);
                bw.Write('@');
                bw.LargeText("Jemaka");
                bw.FeedLines(3);
                bw.Finish();
                bw.Flush();
                return ms.ToArray();
            }
        }


        [HttpPost("stampaOrdine")]
        public async Task<IActionResult> StampaOrdine(List<Comanda> listaOrigine, string oldStato)
        {
            //stampaComande( List<Comande> listaOrigine, string oldStato)
            // STATO "inviato" oppure "ristampa"


            List<Comande> list= new List<Comande>();
            List<string> lista = new List<string>();
            foreach (Comanda comanda in listaOrigine) 
            {
                Tavolata t = new Tavolata(comanda.Id_tavolata);
                Comande c=new Comande(comanda.Id_comanda);
                c.DescrizioneTavolata = t.Descrizione;
                c.DescrizionPietanza = comanda.Pietanza.Descrizione;
                c.Operatore = "oper";
                c.Sala = "coperto";
                list.Add(c);
                lista.Add(comanda.Id_comanda.ToString());
                
            }
           

            try
            {
                 string url = _client.BaseAddress + $"/stampaComande?listaID={string.Join(',',lista)}&oldStato={oldStato}";

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

            return Ok();
        }
             
        [HttpGet("StampaContoTavolo")]
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

        public static string PrinterName(string NomeStampante)
        {
            return $@"\\{Environment.MachineName}\{NomeStampante}";
        }
        private void RestartIis()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "iisreset",
                Arguments = "/restart",
                CreateNoWindow = true,
                UseShellExecute = false
            }).WaitForExit();
        }

        [HttpGet("getOperatorebyNomeandById")]
        public ActionResult<Operatori> getOperatorebyNomeandById(string nominativo, string pin)
        {
            Operatori? op = Operatori.Create(nominativo, pin);
            if (op == null)
            {
                return NotFound("Credenziali non valide o Operatore non Attivo");
            }
            return op;
        }

    }
}
