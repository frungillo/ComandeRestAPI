using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using CrystalDecisions.ReportAppServer.DataDefModel;



namespace ComandeRestAPI.Classi
{
    public class Scontrino
    {
        private string item;
        private decimal prezzo_unitario;
        private int quantita;
        private decimal totale;

        public string Item { get => item; set => item = value; }
        public decimal Prezzo_unitario { get => prezzo_unitario; set => prezzo_unitario = value; }
        public int Quantita { get => quantita; set => quantita = value; }
        public decimal Totale { get => totale; set => totale = value; }

        public Scontrino() { }
        public static List<Scontrino> getScontrinoTavolo(int id_tavolata) 
        {
            List <Scontrino> list = new List<Scontrino>();
            string sql = $@"select   pietanze.descrizione,  pietanze.prezzo, quantita, quantita*pietanze.prezzo 
                            from ordini join pietanze 
                            on ordini.id_pietanza=pietanze.id_pietanza 
                            where id_tavolata={id_tavolata}
                            union 
                            select  menu.descrizione,   menu.prezzo,Q=quantita, quantita*menu.prezzo
                            from ordini join menu 
                            on menu.id_menu=ordini.id_menu
                            where id_tavolata={id_tavolata}
                            union 
                            select prestazioni_extra.descrizione,   prestazioni_extra.prezzo,quantita=1, prestazioni_extra.prezzo 
                            from prestazioni_extra join tavolata 
                            on prestazioni_extra.idTavolata=tavolata.id_tavolata 
                            where id_tavolata={id_tavolata}
                            union 
							select 'SCONTO', tavolata.sconto,quantita=1,-tavolata.sconto from tavolata where id_tavolata={id_tavolata} and tavolata.sconto <> 0
							union 
							select 'ACCONTO', tavolata.acconto,quantita=1,-tavolata.acconto from tavolata where id_tavolata={id_tavolata} and tavolata.acconto <> 0;";
            db db = new db();
           
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                Scontrino s = new Scontrino();
                s.Item = r.GetString(0);
                s.prezzo_unitario=r.GetDecimal(1);
                s.quantita=r.GetInt32(2);
                s.totale=r.GetDecimal(3);
                list.Add(s);
            }
            db.Dispose();
            return list;
        }
    }
}
