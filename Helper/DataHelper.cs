using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Dapper;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using MySql.Data.MySqlClient;

namespace OPD.Helper
{
    public class DataHelper
    {
        public static String DataBaseDir
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["dataBaseDir"];
            }
        }

        public static void cleanDataDir(object directory)
        {
            foreach (String file in System.IO.Directory.GetFiles(directory.ToString(), "*.zip"))
            {
                if (File.Exists(file))
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.CreationTime < DateTime.Today.AddDays(-2))
                    {
                        File.Delete(file);
                    }
                }
            }
        }

        public static FileResult Download(List<int> MeasurementIds, bool DownloadStl, bool DownloadVtk, bool DownloadCsv)
        {            
            string fileName = string.Format("dataset_{0}.zip", DateTime.Today.ToString("yyyyMMddHHmmss"));

            var builder = new SqlBuilder();
            string baseSql = "SELECT measurement_id, filename from particles pa /**where**/"; // measurement_id in @measurement_id and tomogram_id in @tomogram_id";
            var selector = builder.AddTemplate(baseSql);


            DynamicParameters parameters = new DynamicParameters();
            foreach(int id in MeasurementIds)
            {
                builder.OrWhere("measurement_id = @measurement_id and tomogram_id = @tomogram_id", new { measurement_id = id });
            }

            IEnumerable<dynamic> filesToDownload;
            using (var connection = new MySqlConnection(DatabaseHelper.getDbConnectionString()))
            {
                filesToDownload = connection.Query(selector.RawSql, selector.Parameters).AsList();    
                
            }

            MemoryStream outputMemStream = new MemoryStream();

            using (ZipOutputStream s = new ZipOutputStream(outputMemStream))
            {
                foreach (string file in filesToDownload)
                {

                    ZipEntry entry = new ZipEntry(String.Format("{0}/{1}/"));
                }
            }

            if (fileName == null) return null;
            byte[] fileBytes = System.IO.File.ReadAllBytes(fileName);
            return FileResult(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        private static FileResult FileResult(byte[] fileBytes, string octet, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}