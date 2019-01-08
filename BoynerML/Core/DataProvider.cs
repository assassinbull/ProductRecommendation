using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BoynerML.Core
{
    public class DataProvider
    {
        #region | Constants |
        private const string Connectionstring = "";

        private const string GetProductTagsQuery = @"
SELECT ProductId, STUFF((
SELECT ',' + JSON_VALUE(value, '$.description')
FROM[dbo].[ProductImageProcessResponse] pips
CROSS APPLY OPENJSON(JSON_QUERY(Response, '$.responses[0].labelAnnotations'))
WHERE CloudApi = 'Google' AND pips.ProductId = p.ProductId
   AND CAST(JSON_VALUE(value, '$.score') AS DECIMAL(3, 2)) > 0.2
   AND JSON_VALUE(value, '$.description') NOT IN('person', 'standing', 'sitting', 'wearing', 'posing', 'wall', 'fashion model')
FOR XML PATH('')
), 1, 1, '') ProductTags
FROM[dbo].[Product] p
";

        private const string GetRecommendationDatasetQuery = @"
SELECT *
FROM {0}
";

        private const string GetRecommendationDatasetStatusQuery = @"
SELECT
	RecommendationDatasetTableName
    , ActiveFrom
FROM RecommendationDatasetStatus
WHERE IsActive = 1
";
        #endregion
        
        public static DataTable GetRecommendationDatasetFromCsv()
        {
            return GetDatasetFromCsv("Boyner Product Recommendation Dataset.csv");
        }

        public static DataTable GetProductTypeRecommendationDatasetFromCsv()
        {
            return GetDatasetFromCsv("Boyner Product-Type Recommendation.csv");
        }

        public static DataTable GetColorGroupRecommendationDatasetFromCsv()
        {
            return GetDatasetFromCsv("Boyner Color-Group Recommendation.csv");
        }

        public static DataTable GetGenderRecommendationDatasetFromCsv()
        {
            return GetDatasetFromCsv("Boyner Gender Recommendation.csv");
        }

        public static DataTable GetProductDatasetFromCsv()
        {
            return GetDatasetFromCsv("Boyner Product Dataset.csv");
        }

        private static DataTable GetDatasetFromCsv(string csvFileName)
        {
            DataTable dtCsv = new DataTable();
            string Fulltext;
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            string FileSaveWithPath = path.Replace(@"file:\", "") + @"\Datasets\" + csvFileName;
            using (StreamReader sr = new StreamReader(FileSaveWithPath))
            {
                while (!sr.EndOfStream)
                {
                    Fulltext = sr.ReadToEnd().ToString(); //read full file text
                    Fulltext = Fulltext.Replace("\r", "");
                    string[] rows = Fulltext.Split('\n'); //split full file text into rows
                    for (int i = 0; i < rows.Count() - 1; i++)
                    {
                        string[] rowValues = rows[i].Split(','); //split each row with comma to get individual values
                        {
                            if (i == 0)
                            {
                                for (int j = 0; j < rowValues.Count(); j++)
                                {
                                    dtCsv.Columns.Add(rowValues[j]); //add headers
                                }
                            }
                            else
                            {
                                DataRow dr = dtCsv.NewRow();
                                for (int k = 0; k < rowValues.Count(); k++)
                                {
                                    dr[k] = rowValues[k].ToString();
                                }
                                dtCsv.Rows.Add(dr); //add other rows
                            }
                        }
                    }
                }
            }
            return dtCsv;
        }

        public static DataTable GetRecommendationDatasetStatus()
        {
            var result = new DataTable();

            using (SqlConnection connection = new SqlConnection(Connectionstring))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(GetRecommendationDatasetStatusQuery, connection))
                {
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                    {
                        dataAdapter.Fill(result);
                    }
                }
            }

            return result;
        }

        public static DataTable GenerateProductRecommendationDataTable()
        {
            var dt = new DataTable();

            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("InputTags", typeof(string));
            dt.Columns.Add("ConfidenceRank", typeof(int));
            dt.Columns.Add("OutputClass", typeof(string));
            dt.Columns.Add("RecommendationDate", typeof(DateTime));

            return dt;
        }

        public static void WriteProductRecommendationToDatabase(DataTable dt)
        {
            WriteToDatabase(dt, "ProductRecommendation");
        }

        private static void WriteToDatabase(DataTable dataTable, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(Connectionstring))
            {
                SqlBulkCopy bulkCopy =
                    new SqlBulkCopy
                    (
                    connection,
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.FireTriggers |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                    );
                bulkCopy.BulkCopyTimeout = 3600;
                bulkCopy.DestinationTableName = tableName;
                connection.Open();

                bulkCopy.WriteToServer(dataTable);

                connection.Close();
            }
        }
    }
}