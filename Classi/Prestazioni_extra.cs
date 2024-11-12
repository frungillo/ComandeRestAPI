namespace ComandeRestAPI.Classi
{
    public class Prestazioni_extra:DBOperations<Prestazioni_extra>
    {
        private int id;
        private int idtavolata;
        private string descrizione;
        private decimal prezzo;
        private string note;

        [DB_Index]
        public int Id { get => id; set => id = value; }
        public int Idtavolata { get => idtavolata; set => idtavolata = value; }
        public string Descrizione { get => descrizione; set => descrizione = value; }
        public decimal Prezzo { get => prezzo; set => prezzo = value; }
        public string Note { get => note; set => note = value; }

        public Prestazioni_extra() { }
        public Prestazioni_extra(int id):base(id) { }

    }
}
