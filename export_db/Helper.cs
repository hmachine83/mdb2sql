using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace export_db
{
    internal  class Helper
    {
        Dictionary<string, Dictionary<string, string>> tableMetadata = new Dictionary<string, Dictionary<string, string>>();
        string tablesShema = "";
        List<string> tables = new List<string>();


        private string escapePriv(string s)
        {
            string ss = s;
            if (s.ToLower() == "order")
                ss = "Order1";
            if (s.ToLower() == "show")
                ss = "Show1";

            return ss;
        }

        public void loadData(OleDbConnection databaseConnection, ListBox listBox1)
        {


            try
            {
                DataTable dataTable = databaseConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                int numTables = dataTable.Rows.Count;
                string table = "";
                string z = "";

                Debug.WriteLine($"Broj tabli {numTables}");

                for (int tableIndex = 0; tableIndex < numTables; ++tableIndex)
                {
                    table = dataTable.Rows[tableIndex]["TABLE_NAME"].ToString();
                    string datatype = "";
                    if (tableIndex > 0)
                    {
                        listBox1.Items.Add("");
                        listBox1.Items.Add("");
                    }

                    z += "create table if not exists " + table + "(";
                    tables.Add(table);

                    listBox1.Items.Add(table);
                    listBox1.Items.Add("");
                    Dictionary<string, string> type = new Dictionary<string, string>();
                    DataTable schemaTable = databaseConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, table, null });
                    DataRow[] dr = schemaTable.Select(null, "ORDINAL_POSITION", DataViewRowState.CurrentRows);

                    for (int k = 0; k < dr.Length; k++)// DataRow row in schemaTable.Rows)
                    {


                        DataRow row = dr[k];
                        String fieldName = row["COLUMN_NAME"].ToString(); //3
                        String fieldDescription = "";
                        OleDbType myDT = (OleDbType)row["DATA_TYPE"];
                        datatype = myDT.GetType().Name;

                        switch (myDT)
                        {
                            case OleDbType.SmallInt:

                                datatype = "smallint";
                                type.Add(fieldName, datatype);
                                break;
                            case OleDbType.Boolean:
                                datatype = "bool";
                                type.Add(fieldName, datatype);
                                break;
                            case OleDbType.Integer:
                                datatype = "int";
                                type.Add(fieldName, datatype);
                                break;
                            case OleDbType.WChar:
                                datatype = "text";
                                type.Add(fieldName, datatype);
                                break;
                            case OleDbType.Date:
                                datatype = "date";
                                type.Add(fieldName, datatype);
                                break;
                            case OleDbType.Double:
                                datatype = "double";
                                type.Add(fieldName, datatype);
                                break;
                            case OleDbType.Single:
                                datatype = "float";
                                type.Add(fieldName, datatype);
                                break;
                            default:
                                break;
                        }

                        fieldDescription = row["DESCRIPTION"].ToString(); //27
                        if (fieldDescription.Length == 0)
                            fieldDescription = "no description";
                        listBox1.Items.Add("    " + datatype + "\t\t\t\t" + fieldName + "\t\t\t\t(" + fieldDescription + ")");
                        z += escapePriv(fieldName) + " " + datatype + ",";


                    }
                    tableMetadata.Add(table, type);
                    z = z.Substring(0, z.Length - 1);
                    z += ");\n";

                }
                tablesShema = z;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        public void dumpData(OleDbConnection databaseConnection,string fileName)
        {

            File.WriteAllText(fileName, tablesShema);

            for (int i = 0; i < tables.Count; i++)
            {

                OleDbCommand dbCommand = new OleDbCommand();
                dbCommand.Connection = databaseConnection;
                string table_name = tables[i];
                string sSQL = "SELECT * FROM " + tables[i] + ";";
                var dict = tableMetadata[tables[0]];

                //sSQL += "WHERE TypePropertyID = @id AND TypeCounter = @counter;";
                dbCommand.CommandText = sSQL;
                //dbCommand.Parameters.AddWithValue("@counter", nTypeCounter);
                //dbCommand.Parameters.AddWithValue("@id", nTypePropertyId);

                OleDbDataReader reader;

                try
                {
                    reader = dbCommand.ExecuteReader();
                }
                catch (Exception ex)
                {
                    continue;
                }


                List<string> rows = new List<string>();


                string insert = "insert into " + table_name + " (";

                foreach (var entry in dict)
                {
                    insert += entry.Key + ",";
                }

                insert = insert.Substring(0, insert.Length - 1);
                insert += ") values(";



                while (reader.Read())
                {

                    string ins = insert;
                    for (int k = 0; k < reader.FieldCount; k++)
                    {
                        if (reader.GetValue(k) is bool)
                        {
                            var v = reader.GetValue(k).ToString();
                            ins += "" + v + ",";
                        }
                        else
                        {
                            if (reader.GetValue(k) is DateTime)
                            {
                                var v = (DateTime)reader.GetValue(k);
                                var d = v.Date.ToString("yyyy-MM-dd");
                                ins += "'" + d + "',";
                            }
                            else
                            {
                                var v = reader.GetValue(k).ToString();
                                ins += "'" + v + "',";
                            }
                        }
                    }

                    ins = ins.Substring(0, ins.Length - 1);
                    ins += ");\n";

                    File.AppendAllText(fileName, ins);
                }

            }


        }
    }
}
