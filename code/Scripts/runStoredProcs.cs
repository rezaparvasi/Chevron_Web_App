using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace DataScheduling
{
    class Program
    {
        public void execStoredProc()
        {
            using (SqlConnection conn = new SqlConnection("Server=tcp:chevrondata.database.windows.net,1433;Initial Catalog=DataSimulator;Persist Security Info=False;User ID=chaincoders;Password=EveryDayImCoding0;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=240;"))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("create_data", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Minutes", SqlDbType.Int).Value = 5;
                        cmd.ExecuteNonQuery();
                    }

                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                    return;
                }

                //Query the Rules and measurements tables
                //Run the UpdateEvents Stored Procedure for each rule
                using (var rulesCommand = conn.CreateCommand())
                {   
                    try
                    {
                        rulesCommand.CommandType = CommandType.Text;
                        rulesCommand.CommandText = @"SELECT Rules.Value, Rules.ID, Measurements.tagName FROM dbo.Rules INNER JOIN dbo.Measurements ON (Rules.FK_MeasurementsID = Measurements.ID)";
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(rulesCommand);
                        DataTable queryResults = new DataTable();
                        dataAdapter.Fill(queryResults);
                        foreach(DataRow row in queryResults.Rows)
                        {
                            var compareVal = row[0];
                            var ruleId = row[1];
                            var tag = row[2];
                            using (SqlCommand cmd = new SqlCommand("UpdateEvents", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@CompareValue", SqlDbType.Int).Value = compareVal;
                                cmd.Parameters.Add("@ruleid", SqlDbType.Int).Value = ruleId;
                                cmd.Parameters.Add("@tag", SqlDbType.NVarChar).Value = tag;
                                cmd.ExecuteNonQuery();
                            }
                        }
                      
       
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                        return;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

        }

        static void Main(string[] args)
        {
            Program p = new Program();
            p.execStoredProc();
        }
    }
}
