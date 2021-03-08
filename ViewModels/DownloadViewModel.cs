using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;

namespace OPD.ViewModels
{
    public class DownloadViewModel
    {
        private static String baseDir = Helper.DataHelper.DataBaseDir + "{0}/particles/{1}.stl";

        [Display(Name="download")]
        public bool Checked { get; set; }

        public int MeasurementId { get; set; }

        public String FileName { get; set; }

        [Display(Name = "material")]
        public String Material { get; set; }

        [Display(Name = "particle preview")]
        public String FileNameAndPathToStl
        {
            get
            {
                return String.Format(baseDir, MeasurementId, FileName);
            }
        }

        [Display(Name = "volume data - voxel (*.vtk)")]
        public bool DownloadVtk { get; set; }

        [Display(Name = "volume data - voxel (*.stl)")]
        public bool DownloadStl { get; set; }

        [Display(Name = "complete data + metadata (*.csv)")]
        public bool DownloadCsv { get; set; }
    }
}