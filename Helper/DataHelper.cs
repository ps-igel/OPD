using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Dapper;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using OPD.ViewModels;
using System.Linq;

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

        public static string Download(List<int> measurementIds, String filterSettings, bool DownloadStl, bool DownloadVtk, bool DownloadCsv)
        {            
            string fileName = string.Format("dataset_{0}.zip", DateTime.Now.ToString("yyyyMMddHHmmss"));

            //get particle filenames + path from measurement id
            //check csv/vtk/stl bools 
            //include into separate folders?
            //create in memory text with complete query data
            //create in memory text with filter settings

            FilterViewModel vm = JsonConvert.DeserializeObject<FilterViewModel>(filterSettings);

            String baseSql = @"
                                SELECT 
                                pa.filename
                                ,pa.measurement_id
                                ,pa.volume
                                ,pa.surface
                                ,pa.nr_object_voxels
                                ,pa.nr_surface_voxels
                                ,pa.x
                                ,pa.y
                                ,pa.z
                                ,pa.mean_distance_to_surface
                                ,pa.std_distance_to_surface
                                ,pa.median_distance_to_surface
                                ,pa.xm
                                ,pa.ym
                                ,pa.zm
                                ,pa.bx
                                ,pa.`by`
                                ,pa.bz
                                ,pa.b_width
                                ,pa.b_height
                                ,pa.b_depth
                                ,meas.number_stacks
                                ,meas.field_of_view
                                ,meas.acceleration_voltage
                                ,meas.electrical_power
                                ,meas.source_filter
                                ,meas.exposure_time
                                ,meas.optical_magnification
                                ,meas.number_projections
                                ,meas.angular_range
                                ,meas.voxel_size
                                ,meas.binning
                                ,meas.example_stack_image
                                ,meas.doi_url
                                ,sa.diameter
                                ,sa.height
                                ,sa.material
                                ,sa.production_process
                                ,sa.official_project_number
                                ,sa.sem_example_image
                                ,sa.sem_example_detail_image
                                ,sa.particle_size_distribution_image
                                ,img.software_part1
                                ,img.software_part2
                                ,img.image_processing_workflow_part1
                                ,img.image_processing_workflow_part2
                                ,calc.equivalent_spherical_diameter
                                ,calc.sphericity
                                ,calc.convexity
                                ,calc.surface_area
                                ,calc.principal_axis1_length
                                ,calc.principal_axis2_length
                                ,calc.principal_axis3_length
                                ,recon.algorithm
                                ,recon.beam_hardening_correction
                                ,recon.smoothing
                                ,recon.byte_scaling_min
                                ,recon.byte_scaling_max
                                from
                                particles pa
                                left join measurement meas on meas.measurement_id = pa.measurement_id
                                left join sample sa on pa.sample_id = sa.sample_id
                                left join imageprocessing img on img.img_process_id = pa.img_process_id
                                left join calculatedparameters calc on calc.particle_id = pa.particle_id
                                left join reconstruction recon on recon.measurement_id = meas.measurement_id
                                left join v_calculatedparameters v_calc on v_calc.particle_id = pa.particle_id
                                /**where**/                                
                            ";

            DynamicParameters parameters = new DynamicParameters();
            SqlBuilder sqlBuilder = new SqlBuilder();
            var sql = sqlBuilder.AddTemplate(baseSql);

            sqlBuilder.Where("meas.measurement_id in @measurementId", new { measurementId = measurementIds });
            sqlBuilder.addBaseFilters(vm);

            string csvData = "";
            dynamic files;
            using (var connection = new MySqlConnection(DatabaseHelper.getDbConnectionString()))
            {
                var data = connection.Query(sql.RawSql, sql.Parameters).AsList();

                csvData = ServiceStack.Text.CsvSerializer.SerializeToCsv(data);

                //files = data.Select(x => new Tuple<string, string>(x.filename, x.measurement_id.ToString())).ToList();
                files = data.Select(x => new { Filename = x.filename, MeasurementId = x.measurement_id.ToString(), Material = x.material }).ToList();
            }
            
            String tempFolder = Guid.NewGuid().ToString();

            Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("~/data/temp/" + tempFolder));
            String fileNameAndPath = System.Web.HttpContext.Current.Server.MapPath("~/data/temp/" + tempFolder + "/" + fileName);
            FileStream fs = File.Create(fileNameAndPath);
            using (ZipOutputStream s = new ZipOutputStream(fs))
            {
                s.SetLevel(2); // 0 - store only

                foreach (var info in files)
                {
                    if (DownloadVtk) ArchiveFile(info.MeasurementId, info.Filename, info.Material, "vtk", s);
                    if (DownloadStl) ArchiveFile(info.MeasurementId, info.Filename, info.Material, "stl", s);
                }

                if (DownloadCsv) ArchiveString(csvData, "RawData.csv", s);
                ArchiveString(filterSettings, "FilterSettings.txt", s);
            }

            return fileNameAndPath;
        }

        private static void ArchiveFile(string measurementId, string filename, string material, string extension, ZipOutputStream outputStream)
        {
            String file = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/data/measurement/{0}/particles/{1}.{2}", measurementId, filename, extension));

            if (System.IO.File.Exists(file))
            {
                ZipEntry entry = new ZipEntry(string.Format("{0}/{1}/{2}.{3}", material, measurementId, filename, extension));
                outputStream.PutNextEntry(entry);
                byte[] buffer = new byte[4096];
                using (FileStream fs = File.OpenRead(file))
                {
                    StreamUtils.Copy(fs, outputStream, buffer);
                }
            }
        }

        private static void ArchiveString(String content, String fileName, ZipOutputStream outputStream)
        {
            byte[] buffer = new byte[4096];
            ZipEntry entry = new ZipEntry(fileName);
            outputStream.PutNextEntry(entry);
            using (var stream = GenerateStreamFromString(content))
            {
                StreamUtils.Copy(stream, outputStream, buffer);
            }            
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}