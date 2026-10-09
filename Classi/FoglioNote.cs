using System.Data.SqlClient;
using System.Globalization;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace ComandeRestAPI.Classi
{
    /// <summary>
    /// Riga del Foglio Note (una per tavolata del servizio).
    /// Porting di CarbolandiaClient/ClassiDiServizio/FoglioNoteNew.cs.
    /// </summary>
    public class FoglioNoteRiga
    {
        public int IdTavolata { get; set; }
        public string? Descrizione { get; set; }
        public string? Telefono { get; set; }
        public int? Adulti { get; set; }
        public int? Bambini { get; set; }
        public string? Sala { get; set; }
        public int? Tavolo { get; set; }
        public string? Note { get; set; }
        public decimal? Acconto { get; set; }
        public string? Extra { get; set; }
        public DateTime? DataPrenotazione { get; set; }
    }

    /// <summary>
    /// Foglio Note del servizio (pranzo/cena) in PDF A4 orizzontale.
    /// Porting 1:1 di CarbolandiaClient/ClassiDiServizio/FoglioNotePdfGenerator.cs: stessa query,
    /// stesse colonne, stesse icone (cartella Risorse/foglionote accanto all'eseguibile).
    /// </summary>
    public static class FoglioNote
    {
        /// <param name="dataArrivo">data e ora del servizio nel formato "dd/MM/yyyy HH:mm" (es. 08/10/2026 19:00)</param>
        /// <param name="ordinamento">1 = per nominativo, 2 = per data di prenotazione</param>
        public static List<FoglioNoteRiga> getRighe(string dataArrivo, int ordinamento = 1)
        {
            string order = ordinamento == 2
                ? "ORDER BY data_prenotazione, T.descrizione"
                : "ORDER BY T.descrizione, data_prenotazione";

            string sql = $@"
                SELECT
                        T.id_tavolata,
                        T.descrizione,
                        C.telefono,
                        T.adulti,
                        T.bambini,
                        S.descrizione AS Sala,
                        T.numero_tavolo AS Tavolo,
                        T.Note,
                        T.Acconto,
                        ISNULL(STRING_AGG(E.descrizione, ', '), '') AS Extra,
                        LE.data AS data_prenotazione
                    FROM tavolata T
                    JOIN Sale S ON T.id_sala = S.id_sala
                    JOIN clienti C ON T.id_cliente = C.id_cliente
                    LEFT JOIN prestazioni_extra E ON T.id_tavolata = E.idTavolata
                    OUTER APPLY (
                        SELECT TOP 1 L.data
                        FROM Log_Eventi L
                        WHERE L.evento LIKE 'Inserimento prenotazione%'
                          AND L.evento LIKE '%id_tavolata:' + CAST(T.id_tavolata AS VARCHAR)
                        ORDER BY L.data DESC
                    ) LE
                    WHERE T.data_ora_arrivo = convert(datetime, '{dataArrivo.Replace("'", "''")}', 103)
                    GROUP BY
                        T.id_tavolata, T.descrizione, C.telefono, T.adulti, T.bambini,
                        S.descrizione, T.numero_tavolo, T.Note, T.Acconto, LE.data
                    {order}";

            var result = new List<FoglioNoteRiga>();
            db db = new db();
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                result.Add(new FoglioNoteRiga
                {
                    IdTavolata = r.GetInt32(r.GetOrdinal("id_tavolata")),
                    Descrizione = r["descrizione"]?.ToString(),
                    Telefono = r["telefono"]?.ToString(),
                    Adulti = r["adulti"] as int?,
                    Bambini = r["bambini"] as int?,
                    Sala = r["Sala"]?.ToString(),
                    Tavolo = r["Tavolo"] as int?,
                    Note = r["Note"]?.ToString(),
                    Acconto = r["Acconto"] == DBNull.Value ? null : Convert.ToDecimal(r["Acconto"]),
                    Extra = r["Extra"]?.ToString(),
                    DataPrenotazione = r["data_prenotazione"] as DateTime?
                });
            }
            db.Dispose();
            return result;
        }

        /// <summary>Genera il PDF (A4 orizzontale) e lo ritorna come byte[].</summary>
        public static byte[] generaPdf(List<FoglioNoteRiga> dati, string dataServizio, int ordinamento = 1)
        {
            string ord = ordinamento == 2 ? " - (ordinamento per Data di Prenotazione)" : " - (ordinamento per Commensali)";

            using MemoryStream ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            using (var pdf = new PdfDocument(writer))
            {
                pdf.SetDefaultPageSize(PageSize.A4.Rotate());

                using var document = new Document(pdf);
                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                document.Add(new Paragraph("CARBOLANDIA - FOGLIO NOTE")
                    .SetFont(bold).SetFontSize(22).SetTextAlignment(TextAlignment.LEFT));
                document.Add(new Paragraph($"Elenco tavolate per il servizio del: {dataServizio}{ord}")
                    .SetFont(font).SetFontSize(12).SetMarginBottom(10));

                // icone (facoltative: se il file manca l'intestazione resta solo testuale)
                var icoAdulti = icona("adulti.png");
                var icoBambini = icona("bambini.png");
                var icoAcconto = icona("acconto.png");
                var icoPren = icona("prenotazione.png");
                var icoExtra = icona("extra.png");
                var icoPhone = icona("telefono.png");
                var icoTavolo = icona("tavolo.png");
                var icoSala = icona("sala.png");
                var icoNote = icona("note.png");
                var icoDescrizione = icona("nominativo.png");

                var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2, 1, 1, 2, 1, 3, 1, 3, 2 }))
                    .UseAllAvailableWidth();

                addHeader(table, bold, "Descrizione", icoDescrizione);
                addHeader(table, bold, "Telefono", icoPhone);
                addHeader(table, bold, "Adulti", icoAdulti);
                addHeader(table, bold, "Bambini", icoBambini);
                addHeader(table, bold, "Sala", icoSala);
                addHeader(table, bold, "Tavolo", icoTavolo);
                addHeader(table, bold, "Note", icoNote);
                addHeader(table, bold, "Acconto", icoAcconto);
                addHeader(table, bold, "Extra", icoExtra);
                addHeader(table, bold, "Data Pren.", icoPren);

                int totaleAdulti = dati.Sum(x => x.Adulti ?? 0);
                int totaleBambini = dati.Sum(x => x.Bambini ?? 0);
                decimal totaleAcconto = dati.Sum(x => x.Acconto ?? 0);

                foreach (var r in dati)
                {
                    table.AddCell(cellValue(r.Descrizione));
                    table.AddCell(cellValue(r.Telefono, fontSize: 11));
                    table.AddCell(cellValue(r.Adulti));
                    table.AddCell(cellValue(r.Bambini));
                    table.AddCell(cellValue(r.Sala));
                    table.AddCell(cellValue(r.Tavolo));
                    table.AddCell(cellValue(r.Note));
                    table.AddCell(cellValue(r.Acconto, isMoney: true));
                    table.AddCell(cellValue(r.Extra));
                    table.AddCell(cellValue(r.DataPrenotazione, isDate: true));
                }

                // riga totali
                table.AddCell(new Cell(1, 2).Add(new Paragraph("TOTALI")).SetFont(bold)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.RIGHT));
                table.AddCell(new Cell().Add(new Paragraph(totaleAdulti.ToString())).SetFont(bold)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(totaleBambini.ToString())).SetFont(bold)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell().SetBackgroundColor(ColorConstants.LIGHT_GRAY)); // Sala
                table.AddCell(new Cell().SetBackgroundColor(ColorConstants.LIGHT_GRAY)); // Tavolo
                table.AddCell(new Cell().SetBackgroundColor(ColorConstants.LIGHT_GRAY)); // Note
                table.AddCell(new Cell().Add(new Paragraph(totaleAcconto.ToString("0.00"))).SetFont(bold)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell().SetBackgroundColor(ColorConstants.LIGHT_GRAY)); // Extra
                table.AddCell(new Cell().SetBackgroundColor(ColorConstants.LIGHT_GRAY)); // Data Pren.

                document.Add(table);
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Stampa il PDF direttamente dal server su una stampante Windows.
        /// Le pagine vengono renderizzate in immagini (PDFtoImage / PDFium, in-process) e mandate allo
        /// spooler con System.Drawing.Printing: nessun eseguibile esterno, nessuna finestra, quindi
        /// funziona anche sotto IIS (sessione di servizio senza desktop), come già fa l'ASMX.
        /// Stampante usata, in ordine: parametro esplicito, chiave parametri_wa 'stampante_foglio_note',
        /// stampante predefinita dell'utenza con cui gira l'API (app pool IIS).
        /// Ritorna il nome della stampante usata.
        /// </summary>
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        public static string stampa(byte[] pdf, string? stampante = null)
        {
            if (string.IsNullOrWhiteSpace(stampante))
            {
                try { stampante = commons.recuperoParametri("stampante_foglio_note").Trim(); } catch { stampante = ""; }
            }
            if (string.IsNullOrWhiteSpace(stampante))
            {
                stampante = new System.Drawing.Printing.PrinterSettings().PrinterName; // predefinita
            }
            if (string.IsNullOrWhiteSpace(stampante))
            {
                throw new Exception("Nessuna stampante predefinita per l'utenza dell'API e chiave 'stampante_foglio_note' assente in parametri_wa.");
            }

            var impostazioni = new System.Drawing.Printing.PrinterSettings { PrinterName = stampante };
            if (!impostazioni.IsValid)
            {
                throw new Exception($"Stampante '{stampante}' non trovata o non accessibile per l'utenza con cui gira l'API.");
            }

            // 1) PDF -> una immagine per pagina (150 dpi bastano per testo e icone del Foglio Note)
            var pagine = new List<System.Drawing.Image>();
            foreach (SkiaSharp.SKBitmap bmp in PDFtoImage.Conversion.ToImages(pdf, options: new PDFtoImage.RenderOptions(Dpi: 150)))
            {
                using (bmp)
                using (SkiaSharp.SKImage img = SkiaSharp.SKImage.FromBitmap(bmp))
                using (SkiaSharp.SKData data = img.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
                {
                    pagine.Add(System.Drawing.Image.FromStream(new MemoryStream(data.ToArray())));
                }
            }
            if (pagine.Count == 0) throw new Exception("Il PDF del Foglio Note non contiene pagine.");

            // 2) stampa GDI: una pagina A4 orizzontale per immagine, adattata ai margini
            try
            {
                int indice = 0;
                using var doc = new System.Drawing.Printing.PrintDocument();
                doc.PrinterSettings = impostazioni;
                doc.DocumentName = "Foglio Note Carbolandia";
                doc.PrintController = new System.Drawing.Printing.StandardPrintController(); // niente finestra di stato
                doc.DefaultPageSettings.Landscape = true;
                doc.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(25, 25, 25, 25); // centesimi di pollice
                doc.PrintPage += (s, e) =>
                {
                    System.Drawing.Image img = pagine[indice];
                    System.Drawing.RectangleF area = e.MarginBounds;
                    float rapporto = img.Width / (float)img.Height;
                    float w = area.Width, h = w / rapporto;
                    if (h > area.Height) { h = area.Height; w = h * rapporto; }
                    e.Graphics!.DrawImage(img, area.X + (area.Width - w) / 2, area.Y + (area.Height - h) / 2, w, h);
                    indice++;
                    e.HasMorePages = indice < pagine.Count;
                };
                doc.Print();
            }
            finally
            {
                foreach (var p in pagine) p.Dispose();
            }
            return stampante;
        }

        // ============================ helpers ============================

        private static string cartellaIcone => System.IO.Path.Combine(AppContext.BaseDirectory, "Risorse", "foglionote");

        private static Image? icona(string nomeFile)
        {
            try
            {
                string path = System.IO.Path.Combine(cartellaIcone, nomeFile);
                if (!File.Exists(path)) return null;
                return new Image(ImageDataFactory.Create(path));
            }
            catch { return null; }
        }

        private static void addHeader(Table table, PdfFont font, string text, Image? icon)
        {
            var paragraph = new Paragraph().SetFont(font).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER);
            if (icon != null)
            {
                icon.ScaleToFit(25, 25);
                icon.SetAutoScale(false);
                icon.SetMarginRight(4);
                paragraph.Add(icon);
            }
            paragraph.Add(text);
            table.AddHeaderCell(new Cell().Add(paragraph)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPaddingTop(5).SetPaddingBottom(5).SetMinHeight(30));
        }

        private static Cell cellValue(object? val, bool isMoney = false, bool isDate = false, int fontSize = 8)
        {
            string text = "-";
            if (val != null)
            {
                if (isMoney && val is decimal d) text = d.ToString("0.00", CultureInfo.InvariantCulture);
                else if (isDate && val is DateTime dt) text = dt.ToString("dd/MM HH:mm");
                else text = val.ToString() ?? "-";
                if (text == "") text = "-";
            }
            return new Cell().Add(new Paragraph(text)).SetFontSize(fontSize)
                .SetPaddingTop(4).SetPaddingBottom(4).SetMinHeight(20);
        }
    }
}
