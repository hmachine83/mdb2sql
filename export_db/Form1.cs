using System;
using System.Data.OleDb;
using System.Diagnostics;
using System.Windows.Forms;

namespace export_db
{
    public partial class Form1 : Form
    {

        OleDbConnection databaseConnection = null;
        Helper util = new Helper();

        public Form1()
        {
            InitializeComponent();
 
        }
        
        private void readDatabase(string databaseName)
        {


            String f = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=""" + databaseName + "\"";
            try
            {
                Debug.WriteLine(f);

                databaseConnection = new OleDbConnection();
                databaseConnection.ConnectionString = f;
                databaseConnection.Open();

                util.loadData(databaseConnection, this.listBox1);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }

            

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (saveFileDialog1.ShowDialog()== DialogResult.OK)
            {
                util.dumpData(databaseConnection, saveFileDialog1.FileName);
            }
             

        }

        private void button2_Click(object sender, EventArgs e)
        {

  
            openFileDialog1.Title = "Browse mdb File";
            openFileDialog1.Filter = "mdb file (*.mdb)|*.mdb";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string s = openFileDialog1.FileName;
                readDatabase(s);
                btnExport.Enabled= true;
            }

        }

    }
}
