using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using OPD.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using SIO=System.IO;
using System.Threading;
using OPD.Helper;
using MySql.Data.MySqlClient;
using Dapper;
using System.Web;
using System.Web.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OPD.Controllers
{
    public class DataController : Controller
    {

        public ActionResult Index()
        {
            String baseSql = @" SELECT min(nr_object_voxels) as min_object_voxels, max(nr_object_voxels) as max_object_voxels, min(nr_surface_voxels) as min_surface_voxels, max(nr_surface_voxels) as max_surface_voxels from particles;
                                SELECT min(voxel_size) as min_voxel_size, max(voxel_size) as max_voxel_size from measurement;
                                SELECT
                                truncate(min(equivalent_spherical_diameter),2) as min_equivalent_spherical_diameter,
                                round(max(equivalent_spherical_diameter),2) as max_equivalent_spherical_diameter,
                                truncate(min(sphericity),2) as min_sphericity,
                                round(max(sphericity),2) as max_sphericity,
                                truncate(min(feret_box_filling_ratio),2) as min_feret_box_filling_ratio,
                                round(max(feret_box_filling_ratio),2) as max_feret_box_filling_ratio
                                from
                                v_calculatedparameters;
                            ";

            FilterViewModel vm = new FilterViewModel();

            using (var connection = new MySqlConnection(DatabaseHelper.getDbConnectionString()))
            {
                vm.MaterialList = connection.Query<SelectListItem>("SELECT DISTINCT material as Text, material as Value FROM sample");
                vm.ProcessList = connection.Query<SelectListItem>("SELECT DISTINCT production_process as Text, production_process as Value FROM sample");                

                using (var multi = connection.QueryMultiple(baseSql))
                {
                    var particles = multi.Read<dynamic>().Single();
                    vm.MinNumberSurfaceVoxels = particles.min_surface_voxels;
                    vm.MaxNumberSurfaceVoxels = particles.max_surface_voxels;
                    vm.MinNumberObjectVoxels = particles.min_object_voxels;
                    vm.MaxNumberObjectVoxels = particles.max_object_voxels;

                    var measurement = multi.Read<dynamic>().Single();
                    vm.MinVoxelSize = measurement.min_voxel_size;
                    vm.MaxVoxelSize = measurement.max_voxel_size;

                    var calcParameters = multi.Read<dynamic>().Single();
                    vm.MinEquivalentSphericalDiameter = calcParameters.min_equivalent_spherical_diameter;
                    vm.MaxEquivalentSphericalDiameter = calcParameters.max_equivalent_spherical_diameter;
                    vm.MinSphericity = calcParameters.min_sphericity;
                    vm.MaxSphericity = calcParameters.max_sphericity;
                    vm.MinFeretBoxFillingRatio = calcParameters.min_feret_box_filling_ratio;
                    vm.MaxFeretBoxFillingRatio = calcParameters.max_feret_box_filling_ratio;
                }
            }

            return View(new CompleteViewModel()
            {
                FilterViewModel = vm
            });
        }

        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Search(CompleteViewModel vm)
        {
            String baseSql = @"
                                SELECT
                                sa.material as Material
                                ,meas.measurement_id as MeasurementId
                                ,count(*) as NumberFilteredParticles
                                ,sa.sem_example_image as SEMExample
                                ,sa.sem_example_detail_image as SEMExampleDetail
                                ,sa.particle_size_distribution_image as ParticleSizeDistribution
                                from 
                                particles pa
                                inner join sample sa on pa.sample_id = sa.sample_id
                                inner join measurement meas on pa.measurement_id = meas.measurement_id
                                inner join v_calculatedparameters v_calc on v_calc.particle_id = pa.particle_id
                                /**where**/
                                group by sa.material,meas.measurement_id,sa.sem_example_image,sa.sem_example_detail_image,sa.particle_size_distribution_image
                             ";

            DynamicParameters parameters = new DynamicParameters();
            SqlBuilder sqlBuilder = new SqlBuilder();
            var sql = sqlBuilder.AddTemplate(baseSql);

            sqlBuilder.addBaseFilters(vm.FilterViewModel);

            List <SearchViewModel> search;
            using (var connection = new MySqlConnection(DatabaseHelper.getDbConnectionString()))
            {
                search = connection.Query<SearchViewModel>(sql.RawSql, sql.Parameters).ToList();
            }
            
            return PartialView(new CompleteViewModel()
            {
                SearchViewModel = search,
                FilterSettings = JsonConvert.SerializeObject(vm.FilterViewModel, Formatting.Indented)
        });
        }

        public ActionResult Data()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Data(CompleteViewModel vm)
        {
            vm.FilterViewModel = JsonConvert.DeserializeObject<FilterViewModel>(vm.FilterSettings);

            String baseSql = @"
                                SELECT
                                pa.measurement_id as MeasurementId
                                ,sa.material as Material
                                ,sa.production_process as Process
                                ,count(*) as NumberFilteredParticles
                                ,max(meas.voxel_size) as VoxelSize
                                ,meas.example_stack_image as ExampleImageFromStack
                                ,meas.doi_url  as RawImageDownloadURL
                                from particles pa
                                inner join sample sa on pa.sample_id = sa.sample_id
                                inner join measurement meas on pa.measurement_id = meas.measurement_id
                                inner join v_calculatedparameters v_calc on v_calc.particle_id = pa.particle_id
                                /**where**/
                                group by pa.measurement_id,sa.material,sa.production_process;
                             ";

            DynamicParameters parameters = new DynamicParameters();
            SqlBuilder sqlBuilder = new SqlBuilder();
            var sql = sqlBuilder.AddTemplate(baseSql);

            sqlBuilder.addBaseFilters(vm.FilterViewModel);

            sqlBuilder.Where("sa.material in @material", new { material = vm.SearchViewModel.Where(i => i.Checked).Select(i => i.Material).ToList() });

            List<DataViewModel> data;
            using (var connection = new MySqlConnection(DatabaseHelper.getDbConnectionString()))
            {
               data = connection.Query<DataViewModel>(sql.RawSql, sql.Parameters).ToList();
            }

            return PartialView(new CompleteViewModel()
            {
                DataViewModel = data,
                FilterSettings = JsonConvert.SerializeObject(vm.FilterViewModel, Formatting.Indented)
            });
        }

        public ActionResult Download()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Download(CompleteViewModel vm)
        {
            vm.FilterViewModel = JsonConvert.DeserializeObject<FilterViewModel>(vm.FilterSettings);

            String baseSql = @"
                                SELECT 
                                meas.measurement_id as MeasurementId
                                ,max(pa.filename) as FileName
                                ,max(sa.material) as Material
                                from particles pa
                                inner join sample sa on pa.sample_id = sa.sample_id
                                inner join measurement meas on pa.measurement_id = meas.measurement_id
                                inner join v_calculatedparameters v_calc on v_calc.particle_id = pa.particle_id
                                /**where**/
                                group by meas.measurement_id
                            ";

            DynamicParameters parameters = new DynamicParameters();
            SqlBuilder sqlBuilder = new SqlBuilder();
            var sql = sqlBuilder.AddTemplate(baseSql);

            sqlBuilder.Where("meas.measurement_id in @measurementId", new { measurementId = vm.DataViewModel.Where(i => i.Checked).Select(i => i.MeasurementId).ToList() });
            sqlBuilder.addBaseFilters(vm.FilterViewModel);

            List<DownloadViewModelList> download;
            using (var connection = new MySqlConnection(DatabaseHelper.getDbConnectionString()))
            {
                download = connection.Query<DownloadViewModelList>(sql.RawSql, sql.Parameters).ToList();
            }
                        
            return PartialView(new CompleteViewModel()
            {
                DownloadViewModel = new DownloadViewModel() { DownloadViewModelList = download },
                FilterSettings = JsonConvert.SerializeObject(vm.FilterViewModel, Formatting.Indented)
            });
        }

        public ActionResult DownloadFile()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DownloadFile(CompleteViewModel vm)
        {
            List<int> measurementIds = vm.DownloadViewModel.DownloadViewModelList.Where(x => x.Checked).Select(x => x.MeasurementId).ToList<int>();

            string fs = DataHelper.Download(measurementIds,vm.FilterSettings, vm.DownloadViewModel.DownloadStl, vm.DownloadViewModel.DownloadVtk, vm.DownloadViewModel.DownloadCsv);

            Response.Cookies.Add(new HttpCookie("OPD_download_done", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss:ff")));
            if (fs == null) return null;                       
            byte[] fileBytes = System.IO.File.ReadAllBytes(fs);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, System.IO.Path.GetFileName(fs));
        }

        public ActionResult Engineer()
        {
            return View();
        }

        public ActionResult Modeler()
        {
            return View();
        }
    }
}