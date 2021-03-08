using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;

namespace OPD.ViewModels
{
    public class FilterViewModel
    {
        [Display(Name = "material")]
        public String Material { get; set; }

        [Display(Name = "process")]
        public String Process { get; set; }

        [Display(Name = "voxel size in µm")]
        public decimal? MinVoxelSize { get; set; }
        public decimal? MaxVoxelSize { get; set; }

        [Display(Name = "number object voxels")]
        public int MinNumberObjectVoxels { get; set; }
        public int MaxNumberObjectVoxels { get; set; }

        [Display(Name = "number surface voxels")]
        public int MinNumberSurfaceVoxels { get; set; }
        public int MaxNumberSurfaceVoxels { get; set; }

        [Display(Name = "equivalent spherical diameter")]
        public double? MinEquivalentSphericalDiameter { get; set; }
        public double? MaxEquivalentSphericalDiameter { get; set; }

        [Display(Name = "sphericity")]
        public double? MinSphericity { get; set; }
        public double? MaxSphericity { get; set; }

        [Display(Name = "FERET Box Filling Ratio")]
        public decimal? MinFeretBoxFillingRatio { get; set; }
        public decimal? MaxFeretBoxFillingRatio { get; set; }

        public IEnumerable<SelectListItem> MaterialList;
        public IEnumerable<SelectListItem> ProcessList;
    }
}