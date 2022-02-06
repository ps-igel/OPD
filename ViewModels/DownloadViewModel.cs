using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;

namespace OPD.ViewModels
{

    public class DownloadViewModel
    {
        public List<DownloadViewModelList> DownloadViewModelList { get; set; }

        [Display(Name = "Surface representation (*.vtk)")]
        public bool DownloadVtk { get; set; }

        [Display(Name = "Surface representation (*.stl)")]
        public bool DownloadStl { get; set; }

        [Display(Name = "parameters + metadata (*.csv)")]
        public bool DownloadCsv { get; set; }
    }

    public class DownloadViewModelList
    {
        private static String baseDir = Helper.DataHelper.DataBaseDir + "{0}/particles/{1}.stl";

        [Display(Name = "download")]
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
    }
}