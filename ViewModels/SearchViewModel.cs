using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;

namespace OPD.ViewModels
{
    public class SearchViewModel
    {
        private static String baseDir = Helper.DataHelper.DataBaseDir + "{0}/{1}";

        public bool Checked {get; set;}

        [Display(Name = "material")]
        public String Material { get; set; }

        public int MeasurementId { get; set; }

        [Display(Name = "Number filtered particles")]
        public int NumberFilteredParticles { get; set; }

        [Display(Name = "SEM example")]
        public String SEMExample { get; set; }
        public String SEMExampleFullPath
        {
            get 
            {
                return String.Format(baseDir, MeasurementId, SEMExample);
            }
        }

        [Display(Name = "SEM example detail")]
        public String SEMExampleDetail { get; set; }
        public String SEMExampleDetailFullPath
        {
            get
            {
                return String.Format(baseDir, MeasurementId, SEMExampleDetail);
            }
        }

        [Display(Name = "ParticleSizeDistribution")]
        public String ParticleSizeDistribution { get; set; }
        public String ParticleSizeDistributionFullPath
        {
            get
            {
                return String.Format(baseDir, MeasurementId, ParticleSizeDistribution);
            }
        }
    }

}