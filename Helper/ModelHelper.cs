using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using Dapper;

namespace OPD.Helper
{
    public static class ModelHelper
    {
        public static IEnumerable<SelectListItem> getMaterialList()
        {
            using (var connection = new MySqlConnection(DatabaseHelper.getDbConnectionString()))
            {
                return connection.Query<SelectListItem>("SELECT DISTINCT material as Text, material as Value FROM sample");
                //.Select(r => new
                //SelectListItem
                //{
                //    Text = r,
                //    Value = r
                //});
            }
        }
        public static IEnumerable<SelectListItem> getProcessList()
        {
            using (var connection = new MySqlConnection(DatabaseHelper.getDbConnectionString()))
            {
                return connection.Query<SelectListItem>("SELECT DISTINCT production_process as Text, production_process as Value FROM sample");
                //.Select(r => new
                //SelectListItem
                //{
                //    Text = r,
                //    Value = r
                //});
            }
        }

        public static bool IsSet(String parameter)
        {
            return parameter != null && parameter != "--all--";
        }

        public static void addMinMaxWhere<T>(this SqlBuilder sqlBuilder, String parameterName, T minValue, T maxValue)
        {
            String paramNameMin = parameterName + "_min";
            String paramNameMax = parameterName + "_max";

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(paramNameMin, minValue);
            parameters.Add(paramNameMax, maxValue);

            sqlBuilder.Where(parameterName + " between @" + paramNameMin + " and @" + paramNameMax, parameters);
        }

        public static void addBaseFilters(this SqlBuilder sqlBuilder, ViewModels.FilterViewModel filterViewModel)
        {
            if (ModelHelper.IsSet(filterViewModel.Material)) sqlBuilder.Where("sa.material = @material", new { filterViewModel.Material });
            if (ModelHelper.IsSet(filterViewModel.Process)) sqlBuilder.Where("sa.production_process = @process", new { filterViewModel.Process });

            sqlBuilder.addMinMaxWhere("meas.voxel_size", filterViewModel.MinVoxelSize, filterViewModel.MaxVoxelSize);
            sqlBuilder.addMinMaxWhere("pa.nr_surface_voxels", filterViewModel.MinNumberSurfaceVoxels, filterViewModel.MaxNumberSurfaceVoxels);
            sqlBuilder.addMinMaxWhere("pa.nr_object_voxels", filterViewModel.MinNumberObjectVoxels, filterViewModel.MaxNumberObjectVoxels);
            sqlBuilder.addMinMaxWhere("v_calc.equivalent_spherical_diameter", filterViewModel.MinEquivalentSphericalDiameter, filterViewModel.MaxEquivalentSphericalDiameter);
            sqlBuilder.addMinMaxWhere("v_calc.sphericity", filterViewModel.MinSphericity, filterViewModel.MaxSphericity);
            sqlBuilder.addMinMaxWhere("v_calc.feret_box_filling_ratio", filterViewModel.MinFeretBoxFillingRatio, filterViewModel.MaxFeretBoxFillingRatio);
        }

    }
}