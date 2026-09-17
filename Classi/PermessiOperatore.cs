using System.Globalization;

namespace ComandeRestAPI.Classi
{
    /// <summary>
    /// Permessi / vincoli di un operatore (socio) letti dalla tabella parametri_wa.
    /// Chiavi riconosciute (XXXX = id_operatore della tabella operatori):
    ///   socio_XXXX_data             valore "dd/MM/yyyy" -> l'operatore vede i dati contabili
    ///                               (incassi, spese, pagamenti, conti tavoli, PDF contabile)
    ///                               solo a partire da questa data inclusa.
    ///   socio_XXXX_cancella_chiusi  valore "1" -> l'operatore può cancellare tavolate già
    ///                               contabilizzate (stato CHIUSO=3 o STAMPATO=4).
    /// In assenza del record il vincolo non si applica (nessun limite di data, nessuna
    /// abilitazione alla cancellazione dei tavoli chiusi).
    /// </summary>
    public class PermessiOperatore
    {
        public int Id_operatore { get; set; }
        public DateTime? Data_inizio_contabile { get; set; }
        public bool Cancella_tavoli_chiusi { get; set; }

        private static readonly string[] FormatiData = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" };

        public static PermessiOperatore get(int id_operatore)
        {
            var p = new PermessiOperatore { Id_operatore = id_operatore };
            if (id_operatore <= 0) return p;

            string data = commons.recuperoParametri($"socio_{id_operatore}_data").Trim();
            if (data != "" &&
                DateTime.TryParseExact(data, FormatiData, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d))
            {
                p.Data_inizio_contabile = d.Date;
            }

            string canc = commons.recuperoParametri($"socio_{id_operatore}_cancella_chiusi").Trim().ToLower();
            p.Cancella_tavoli_chiusi = canc == "1" || canc == "true" || canc == "si";

            return p;
        }

        /// <summary>True se la data è visibile all'operatore (nessun vincolo oppure data >= inizio contabile).</summary>
        public bool dataConsentita(DateTime data)
        {
            return Data_inizio_contabile == null || data.Date >= Data_inizio_contabile.Value.Date;
        }

        /// <summary>
        /// Verifica il vincolo di data partendo dall'id operatore e da una data in formato stringa
        /// (accetta sia "dd/MM/yyyy" che ISO). Se la data non è interpretabile il vincolo non blocca.
        /// </summary>
        public static bool dataConsentita(int id_operatore, string data)
        {
            if (id_operatore <= 0 || string.IsNullOrWhiteSpace(data)) return true;
            var p = get(id_operatore);
            if (p.Data_inizio_contabile == null) return true;
            if (!DateTime.TryParse(data, new CultureInfo("it-IT"), DateTimeStyles.None, out DateTime d)) return true;
            return p.dataConsentita(d);
        }
    }
}
