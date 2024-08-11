namespace ComandeRestAPI.Classi
{
    public class Spesa : DBOperations<Spesa>
    {
        private int id_spesa;
        private string descrizione;
        private string tipo_spesa;
        private decimal importo;
        private DateTime data_registrazione;
        private int moneta;
        private string note;
        private int mese_contabile;
        private int anno_contabile;
        private string utente;

        [DB_Index]
        public int Id_spesa { get => id_spesa; set => id_spesa = value; }
        public string Descrizione { get => descrizione; set => descrizione = value; }
        public string Tipo_spesa { get => tipo_spesa; set => tipo_spesa = value; }
        public decimal Importo { get => importo; set => importo = value; }
        public DateTime Data_registrazione { get => data_registrazione; set => data_registrazione = value; }
        public int Moneta { get => moneta; set => moneta = value; }
        public string Note { get => note; set => note = value; }
        public int Mese_contabile { get => mese_contabile; set => mese_contabile = value; }
        public int Anno_contabile { get => anno_contabile; set => anno_contabile = value; }
        public string Utente { get => utente; set => utente = value; }

        public Spesa() { }
        public Spesa(int id) : base(id) { }
    }
}
