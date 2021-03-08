using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;

namespace OPD.ViewModels
{
    public class DataViewModel
    {
        private static String baseDir = Helper.DataHelper.DataBaseDir + "{0}/{1}";

        public bool Checked { get; set; }

        public int MeasurementId { get; set; }

        [Display(Name = "material")]
        public String Material { get; set; }

        [Display(Name = "process")]
        public String Process { get; set; }

        [Display(Name = "number of filtered particles")]
        public int NumberFilteredParticles { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.0000}")]
        [Display(Name = "voxel size in µm")]
        public decimal? VoxelSize { get; set; }

        [Display(Name = "Example Image From Stack")]
        public String ExampleImageFromStack { get; set; }

        public String ExampleImageFromStackFullPath
        {
            get
            {
                return String.Format(baseDir, MeasurementId, ExampleImageFromStack);
            }
        }

        [Display(Name = "Download RAW Image Stack (OpARA)")]
        public String RawImageDownloadURL { get; set; }
    }
}