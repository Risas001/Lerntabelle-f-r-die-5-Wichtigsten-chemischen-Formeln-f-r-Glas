using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private DataGridView dgvTabelle;
        private Button btnPruefen, btnNeustart;
        private ComboBox cbLevel;
        private ListBox begriffeListe;
        private Random rand = new Random();
        private Dictionary<(int, int), string> loesungen = new Dictionary<(int, int), string>();
        private List<string> fehlendeBegriffe = new List<string>();

        public Form1()
        {
            InitializeComponent();
            InitializeUI();
            ErstelleTabelle(1); // Start mit Level 1
        }

        private void InitializeUI()
        {
            this.Text = "Lern-Tabelle";
            this.ClientSize = new Size(900, 560);
            this.MinimumSize = new Size(640, 480);

            // Alles sitzt in einem Layout, damit beim Vergrößern keine
            // leeren (weißen) Ränder am Rand oder unter der Tabelle stehen bleiben.
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 1,
                RowCount = 3
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));   // Tabelle
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Bedienleiste
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));   // Begriffe

            dgvTabelle = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8),
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                // Spalten füllen die Breite komplett aus -> kein weißer Streifen rechts.
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                // Fläche unter der letzten Zeile in Fensterfarbe statt Weiß.
                BackgroundColor = SystemColors.Control,
                BorderStyle = BorderStyle.FixedSingle,
                ColumnCount = 5
            };

            dgvTabelle.Columns[0].Name = "Rohstoff";
            dgvTabelle.Columns[1].Name = "Chem. Name";
            dgvTabelle.Columns[2].Name = "Chem. Formel";
            dgvTabelle.Columns[3].Name = "Glasoxid";
            dgvTabelle.Columns[4].Name = "Funktion";

            dgvTabelle.DragEnter += DgvTabelle_DragEnter;
            dgvTabelle.DragDrop += DgvTabelle_DragDrop;

            var bedienleiste = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(0),
                WrapContents = false
            };

            btnPruefen = new Button
            {
                Text = "Überprüfen",
                Size = new Size(100, 30),
                Margin = new Padding(0, 0, 10, 0)
            };
            btnPruefen.Click += BtnPruefen_Click;

            btnNeustart = new Button
            {
                Text = "Neustart",
                Size = new Size(100, 30),
                Margin = new Padding(0, 0, 10, 0),
                Visible = false
            };
            btnNeustart.Click += BtnNeustart_Click;

            cbLevel = new ComboBox
            {
                Size = new Size(100, 30),
                Margin = new Padding(0),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbLevel.Items.AddRange(new string[] { "Level 1", "Level 2", "Level 3", "Level 4" });
            cbLevel.SelectedIndex = 0;
            cbLevel.SelectedIndexChanged += (s, e) => ErstelleTabelle(cbLevel.SelectedIndex + 1);

            bedienleiste.Controls.Add(btnPruefen);
            bedienleiste.Controls.Add(btnNeustart);
            bedienleiste.Controls.Add(cbLevel);

            begriffeListe = new ListBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                BorderStyle = BorderStyle.FixedSingle,
                AllowDrop = true
            };
            begriffeListe.MouseDown += BegriffeListe_MouseDown;

            layout.Controls.Add(dgvTabelle, 0, 0);
            layout.Controls.Add(bedienleiste, 0, 1);
            layout.Controls.Add(begriffeListe, 0, 2);

            this.Controls.Add(layout);
        }

        private void ErstelleTabelle(int level)
        {
            dgvTabelle.Rows.Clear();
            loesungen.Clear();
            fehlendeBegriffe.Clear();
            begriffeListe.Items.Clear();
            btnNeustart.Visible = false;
            btnPruefen.Enabled = true;

            string[,] tabelle = {
                { "Quarzsand", "Siliziumdioxid", "SiO2", "SiO2", "Hauptbestandteil" },
                { "Calk", "Calciumcarbonat", "CaCO3", "CaO", "Stabilisator" },
                { "Borsäure", "Boroxid", "H3BO3", "B2O3", "Netzwerkbinder" },
                { "Soda", "Natriumcarbonat", "Na2CO3", "Na2O", "Flussmittel" },
                { "Pottasche", "Kaliumcarbonat", "K2CO3", "K2O", "Flussmittel" },
                { "Dolomit", "Calciumcarbonat + Magnesiumcarbonat", "CaCO3 * MgCO3", "CaO + MgO", "Stabilisator" }
            };

            int spaltenAnzahl = tabelle.GetLength(1);
            int zeilenAnzahl = tabelle.GetLength(0);

            for (int i = 0; i < zeilenAnzahl; i++)
            {
                List<int> leereSpalten = new List<int>();

                // Level bestimmt, wie viele Begriffe entfernt werden
                int anzahlLuecken = Math.Min(level, spaltenAnzahl);

                while (leereSpalten.Count < anzahlLuecken)
                {
                    int zufallSpalte = rand.Next(0, spaltenAnzahl);
                    if (!leereSpalten.Contains(zufallSpalte))
                    {
                        leereSpalten.Add(zufallSpalte);
                    }
                }

                string[] row = new string[spaltenAnzahl];

                for (int j = 0; j < spaltenAnzahl; j++)
                {
                    if (leereSpalten.Contains(j))
                    {
                        row[j] = "";
                        loesungen[(i, j)] = tabelle[i, j];
                        fehlendeBegriffe.Add(tabelle[i, j]);
                    }
                    else
                    {
                        row[j] = tabelle[i, j];
                    }
                }
                dgvTabelle.Rows.Add(row);
            }

            foreach (var begriff in fehlendeBegriffe.OrderBy(x => rand.Next()))
            {
                begriffeListe.Items.Add(begriff);
            }
        }

        private void BtnPruefen_Click(object sender, EventArgs e)
        {
            List<string> zuEntfernendeBegriffe = new List<string>();
            bool allesRichtig = true;

            foreach (var (pos, loesung) in loesungen)
            {
                int row = pos.Item1;
                int col = pos.Item2;
                string userEingabe = dgvTabelle.Rows[row].Cells[col].Value?.ToString().Trim() ?? "";

                if (string.Equals(userEingabe, loesung, StringComparison.OrdinalIgnoreCase))
                {
                    dgvTabelle.Rows[row].Cells[col].Style.BackColor = Color.LightGreen;
                    dgvTabelle.Rows[row].Cells[col].ReadOnly = true;
                    zuEntfernendeBegriffe.Add(userEingabe);
                }
                else
                {
                    dgvTabelle.Rows[row].Cells[col].Style.BackColor = Color.LightCoral;
                    allesRichtig = false;
                }
            }

            foreach (string begriff in zuEntfernendeBegriffe)
            {
                begriffeListe.Items.Remove(begriff);
            }

            if (allesRichtig && begriffeListe.Items.Count == 0)
            {
                btnNeustart.Visible = true;
                btnPruefen.Enabled = false;
                MessageBox.Show("Glückwunsch! Du hast alle Begriffe richtig!", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnNeustart_Click(object sender, EventArgs e)
        {
            ErstelleTabelle(cbLevel.SelectedIndex + 1);
            btnNeustart.Visible = false;
            btnPruefen.Enabled = true;
        }

        private void BegriffeListe_MouseDown(object sender, MouseEventArgs e)
        {
            if (begriffeListe.SelectedItem != null)
                begriffeListe.DoDragDrop(begriffeListe.SelectedItem.ToString(), DragDropEffects.Move);
        }

        private void DgvTabelle_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void DgvTabelle_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)))
            {
                string draggedText = (string)e.Data.GetData(typeof(string));
                if (dgvTabelle.CurrentCell != null && dgvTabelle.CurrentCell.Value.ToString() == "")
                {
                    dgvTabelle.CurrentCell.Value = draggedText;
                    begriffeListe.Items.Remove(draggedText);
                }
            }
        }
    }
}