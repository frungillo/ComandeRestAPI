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
using static ComandeRestAPI.Classi.ClassiStampa;

namespace ComandeRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComandeController : ControllerBase
    {
        private readonly SqlConnection _conn = new SqlConnection(db.connStr());

        private readonly IWebHostEnvironment _env;

        public ComandeController(IWebHostEnvironment env, SqlConnection conn)
        {
            _env = env;
            _conn = conn;
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

        [HttpGet("getMenuByIdTavolo")]
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
                        QuantitaOrdinata = myReader.GetInt32(6)
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

        [HttpGet("getTestataConto")]
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

        [HttpPost("setExtra")]
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

        [HttpPost("aggiornaTavolo")]
        public IActionResult AggiornaTavolo(int idTavolata, string acconto, string sconto)
        {
            if (acconto == "") acconto = "0.0";
            if (sconto == "") sconto = "0.0";
            string sql = $"update tavolata set acconto={acconto.Replace(",", ".")}, sconto={sconto.Replace(",", ".")} where id_tavolata={idTavolata}";
            db db = new db();
            db.getReader(sql);
            db.Dispose();
            return Ok();
        }

        [HttpPost("salvaOrdine")]
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

        [HttpPost("salvaPrestazioneExtra")]
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

        [HttpGet("getSaleHtml")]
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

        [HttpPost("setPagato")]
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

        [HttpGet("creaPopupOperatori")]
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

        [HttpGet("getCorpoConto")]
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

        [HttpGet("getPulsantiPietanze")]
        public ActionResult<string> GetPulsantiPietanze()
        {
            string html = "";
            db db = new db();
            string sql = $@"
                select * from tipi_pietanze 
                where (select count(*) from pietanze where attivo=1 and pietanze.id_tipo = tipi_pietanze.id_tipo) >= 1 
                order by descrizione";
            SqlDataReader r = db.getReader(sql);
            html = "<table style='overflow:auto;'><tr>";
            while (r.Read())
            {
                html += $"<td><input type='button' id='btn_gp_{r[0].ToString()}' value='{r[1].ToString()}' onclick=\"getPietanzeByTipo({r[0].ToString()})\" /></td>";
            }
            html += "</tr></table>";
            db.Dispose();
            return Ok(html);
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

        [HttpPost("setOrdine")]
        public ActionResult<string> SetOrdine(int id_tavolata, string id_voce, int quantita, string note_pietanza)
        {
            int statotavolo = CheckStatoTavolo(id_tavolata);
            db dbc = new db();
            string sqlordini = $"insert into ordini (id_tavolata, id_pietanza, quantita, note_pietanza) values ({id_tavolata}, '{id_voce}', {quantita}, '{note_pietanza}')";
            if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
            if (id_voce.StartsWith("M"))
            {
                sqlordini = $"insert into ordini (id_tavolata, id_menu, quantita, note_pietanza) values ({id_tavolata}, '{id_voce}', {quantita}, '{note_pietanza}')";
                SqlDataReader r1 = dbc.getReader($"select id_ordine from ordini where id_tavolata={id_tavolata} and id_menu='{id_voce}'");
                int id_ordine = -1;
                if (r1.HasRows)
                {
                    r1.Read();
                    id_ordine = r1.GetInt32(0);
                    dbc.Dispose();
                }
                if (id_ordine > -1)
                {
                    sqlordini = $"update ordini set quantita={quantita} where id_ordine={id_ordine}";
                }
            }
            SqlCommand comm = new SqlCommand(sqlordini, _conn);
            comm.ExecuteNonQuery();
            _conn.Close();
            db db = new db();
            db.getReader($"update tavolata set stato ='2' where id_tavolata ={id_tavolata} ");
            db.Dispose();
            return Ok(id_voce.ToString());
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

        [HttpPost("setComande")]
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
                    SetOrdine(item.IdTavolata, item.IdPietanza, item.Quantita, VariazioneAllaPietanza);
                }
                GC.Collect();
            }
            _conn.Close();
            return Ok();
        }

        [HttpGet("getPulsantiPerInvioInCucina")]
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

        [HttpPost("setStatoComanda")]
        public IActionResult SetStatoComanda(string htmlButtonItemID, int idTavolata, string Stato, string oldStato)
        {
            db db = new db();
            string sql = "";
            if (htmlButtonItemID.StartsWith("btn_t"))
            {
                string idTipoPietanzaDaVariare = htmlButtonItemID.Replace("btn_t_", "");
                sql = $@"
                    update comande set stato = '{Stato}'
                    where id_pietanza in (select id_pietanza from pietanze where pietanze.id_tipo = {idTipoPietanzaDaVariare})
                        and id_tavolata = {idTavolata} and stato = '{oldStato}'";
                db.getReader(sql);
            }
            else if (htmlButtonItemID.StartsWith("btn_c"))
            {
                string idComanda = htmlButtonItemID.Replace("btn_c_", "");
                sql = $@"
                    update comande set stato = '{Stato}'
                    where id_comanda = {idComanda}
                        and id_tavolata = {idTavolata} and stato = '{oldStato}'";
                db.getReader(sql);
            }
            else if (htmlButtonItemID.StartsWith("btn_elim"))
            {
                string idComanda = htmlButtonItemID.Replace("btn_elim_", "");
                sql = $@"
                    update comande set stato = 'annullato'
                    where id_comanda = {idComanda}
                        and id_tavolata = {idTavolata}";
                db.getReader(sql);
            }
            else if (htmlButtonItemID.StartsWith("btn_r"))
            {
                string idComanda = htmlButtonItemID.Replace("btn_r_", "");
                sql = $@"
                    update comande set stato = 'ristampa'
                    where id_comanda = {idComanda}
                        and id_tavolata = {idTavolata} and stato = '{oldStato}'";
                db.getReader(sql);
                Stato = "ristampa";
            }
            db.Dispose();
            if (Stato == "inviato" || Stato == "ristampa")
            {
                Dictionary<string, List<Comande>> lst = Comande.getComandeByStatoPerRep(Stato, idTavolata);
                foreach (var rep in lst.Keys)
                {
                    List<Comande> comandeRep;
                    lst.TryGetValue(rep, out comandeRep);
                    StampaComande(rep, comandeRep);
                }
            }
            return Ok();
        }

        private void StampaComande(string idRep, List<Comande> lista, bool isAttesa = false, bool isCorrezione = false)
        {
            logEventi log = new logEventi();
            log.Scrivi("Inizio Stampa comanda per tavolata: " + lista[0].IdTavolata + " la lista contiene " + lista.Count + " elementi", lista[0].Operatore);
            foreach (Comande item in lista)
            {
                if (item.Stato == "ristampa")
                {
                    log.Scrivi($"Richiesta RISTAMPA per la comanda {item.IdComande} tavolata {item.IdTavolata}");
                }
                db db = new db();
                string sql = $@"
                    update comande set stato = 'stampata', ora_stampa=convert(datetime,'{DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()}',103)
                    where id_pietanza = '{item.IdPietanza}'
                        and id_tavolata = {item.IdTavolata} and stato = '{item.Stato}'";
                db.getReader(sql);
                db.CloseReader();
                log.Scrivi("Stato comanda variato per comanda: " + item.IdComande, lista[0].Operatore);
                if (item.Stato != "ristampa")
                {
                    sql = $@"
                        with T1 as (select quantita as q, id_ingrediente as i from ricette where id_pietanza='{item.IdPietanza}')
                        update ingredienti 
                        set giacenza=giacenza-(select q from T1)
                        where id_ingrediente=(select i from T1)";
                    db.getReader(sql);
                }
                db.Dispose();
            }
            try
            {
                Reparti rep = new Reparti(idRep);
                log.Scrivi("Invio alla Stampante " + rep.nomestampante + "  comanda per tavolata: " + lista[0].IdTavolata, lista[0].Operatore);
                ReportDocument doc = new ReportDocument();
                var reportPath = System.IO.Path.Combine(_env.ContentRootPath, "StampaOrdini.rpt");
                doc.Load(reportPath);
                doc.SetDataSource(lista.ToArray());
                doc.SetParameterValue(0, rep.descrizione);
                doc.SetParameterValue(1, isAttesa);
                doc.SetParameterValue(2, isCorrezione);
                doc.PrintOptions.PrinterName = rep.nomestampante;
                doc.PrintToPrinter(1, false, 0, 0);
                Print(rep.nomestampante, GetDocument());
            }
            catch (Exception ex)
            {
                log.Scrivi("Qualcosa non ha funzionato con la stampa comanda per tavolata: " + lista[0].IdTavolata + ". Errore: " + ex.Message + "-->" + ex.InnerException, lista[0].Operatore);
                RestartIis();
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
        public IActionResult StampaOrdine(string idRep, List<Comande> lista)
        {
            Reparti rep = new Reparti(idRep);
            Print(PrinterName(rep.nomestampante), GetDocument());
            return Ok();
        }

        [HttpPost("stampaContoTavolo")]
        public IActionResult StampaContoTavolo(string idTavolo)
        {
            logEventi log = new logEventi();
            try
            {
                db db = new db();
                db.getReader("update tavolata set stato=4 where id_tavolata=" + idTavolo);
                db.Dispose();
                ReportDocument doc = new ReportDocument();
                var reportPath = System.IO.Path.Combine(_env.ContentRootPath, "test.rpt");
                doc.Load(reportPath);
                doc.PrintOptions.PrinterName = "POS-CASSA";
                doc.DataSourceConnections[0].SetConnection(db.DataSource, db.DBName, false);
                doc.SetDatabaseLogon("jmk", "napoli.081");
                doc.SetParameterValue(0, idTavolo);
                doc.SetParameterValue(1, 0);
                doc.PrintToPrinter(1, false, 0, 0);
                doc.Close();
                Print("POS-CASSA", GetDocument());
            }
            catch (Exception ex)
            {
                log.Scrivi("Qualcosa non ha funzionato con la stampa Conto Tavolo: " + idTavolo + ". Errore: " + ex.Message + "-->" + ex.InnerException, "Admin");
                RestartIis();
            }
            return Ok();
        }

        [HttpPost("stampaPreContoTavolo")]
        public IActionResult StampaPreContoTavolo(string idTavolo)
        {
            logEventi log = new logEventi();
            try
            {
                db db = new db();
                db.getReader("update tavolata set stato=5 where id_tavolata=" + idTavolo);
                db.Dispose();
                ReportDocument doc = new ReportDocument();
                var reportPath = System.IO.Path.Combine(_env.ContentRootPath, "test.rpt");
                doc.Load(reportPath);
                doc.PrintOptions.PrinterName = "POS-CASSA";
                doc.DataSourceConnections[0].SetConnection(db.DataSource, db.DBName, false);
                doc.SetDatabaseLogon("jmk", "napoli.081");
                doc.SetParameterValue(0, idTavolo);
                doc.SetParameterValue(1, 1);
                doc.PrintToPrinter(1, false, 0, 0);
                doc.Close();
                Print("POS-CASSA", GetDocument());
            }
            catch (Exception ex)
            {
                log.Scrivi("Qualcosa non ha funzionato con la stampa PreConto Tavolo: " + idTavolo + ". Errore: " + ex.Message + "-->" + ex.InnerException, "Admin");
                RestartIis();
            }
            return Ok();
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
    }
}
