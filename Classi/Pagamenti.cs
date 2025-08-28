using System.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace ComandeRestAPI.Classi
{
    public class Pagamenti
    {


        // TIPO = 0 registrazione da PAGATO
        // TIPO = 1 registrazione da CONTO
        // TIPO = 2 registrazione da ACCONTO
        // TIPO = 5 registrazione vendita diretta alla cassa

        private int _id_pagamento;
        private DateTime _data_ora_registrazione;
        private int _id_tavolata;
        private int _tipo;
        private double _conto_pos;
        private double _conto_contanti;
        private double _conto_altro;
        private string _note;

        public int Id_pagamento { get => _id_pagamento; set => _id_pagamento = value; }
        public DateTime Data_ora_registrazione { get => _data_ora_registrazione; set => _data_ora_registrazione = value; }
        public int Id_tavolata { get => _id_tavolata; set => _id_tavolata = value; }
        public int Tipo { get => _tipo; set => _tipo = value; }
        public double Conto_pos { get => _conto_pos; set => _conto_pos = value; }
        public double Conto_contanti { get => _conto_contanti; set => _conto_contanti = value; }
        public double Conto_altro { get => _conto_altro; set => _conto_altro = value; }
        public string Note { get => _note; set => _note = value; }
        public Pagamenti() { }
        public Pagamenti(int id)
        {
            db db = new db();
            SqlDataReader r = db.getReader($"select * from pagamenti where id_pagamento={id}");
            if (r.HasRows)
            {
                r.Read();
                _id_pagamento = (int)r[0];
                _data_ora_registrazione = r.GetDateTime(1);
                _id_tavolata = (int)r[2];
                _tipo = (int)r[3];
                try { _conto_pos = (double)r.GetDecimal(4); } catch { _conto_pos = 0; }
                try { _conto_contanti = (double)r.GetDecimal(5); } catch { _conto_contanti = 0; }
                try { _conto_altro = (double)r.GetDecimal(6); } catch { _conto_altro = 0; }
                _note = r[7].ToString();
            }
            else
            {
                throw new Exception("Nessun Pagamento con questo ID");
            }
            db.Dispose();
        }
        public static void insert(Pagamenti p)
        {
            // inserimento nuovo Pagamento
            db db = new db();
            string data_ora_reg = $"convert(datetime,'{p.Data_ora_registrazione}',103)";
            string sql = $@"insert into pagamenti 
             values({data_ora_reg},
                    {p.Id_tavolata},
                    {p.Tipo},
                    {p.Conto_pos.ToString().Replace(",", ".")},
                    {p.Conto_contanti.ToString().Replace(",", ".")},
                    {p.Conto_altro.ToString().Replace(",", ".")},
                    '{p.Note.ToString().Replace("'", "''")}'
                    )";
            try
            {
                SqlDataReader r = db.getReader(sql);
                r.Read();
                //int index = int.Parse(r[0].ToString());
                db.Dispose();
                return;

            }
            catch (Exception ex)
            {

                throw new Exception("Errore Salvataggio Pagamento:" + ex.Message);
            }
        }
        public static void update(Pagamenti p)
        {
            // non aggiorno la TAVOLATA

            var db = new db();
            string data_ora_reg = $"convert(datetime,'{p.Data_ora_registrazione}',103)";
            string sql = $@"update pagamenti
                            
                            set data_ora_registrazione={data_ora_reg},
                            tipo={p.Tipo}, 
                            conto_pos={p._conto_pos}, 
                            conto_contanti={p.Conto_contanti}, 
                            conto_altro={p.Conto_altro},
                            note='{p.Note.Replace("'", "''")}'

                            WHERE id_pagamento={p.Id_pagamento} ";
            try
            {
                db.exe(sql);
                db.CloseReader();
                db.Dispose();

            }
            catch (Exception ex)
            {
                throw new Exception("Errore Salvataggio Pagamento:" + ex.Message);
            }
        }
        public static void delete(int id)
        {
            db db = new db();
            string sql = $"delete from pagamenti where id_pagamento={id}";
            try
            {
                db.exe(sql);
                db.CloseReader();
                db.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("Errore Cancellazione Pagamento:" + ex.Message);
            }
        }
        public static List<Pagamenti> getSpeseFiltro(string filtro)
        {
            db db = new db();
            string sql = $@"select id_pagamento from pagamenti where {filtro} ";
            List<Pagamenti> lst = new List<Pagamenti>();
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                int c = (int)r[0];
                Pagamenti p = new Pagamenti(c);
                lst.Add(p);
            }
            db.Dispose();
            return lst;
        }
    }
}
