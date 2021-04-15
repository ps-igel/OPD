using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;

namespace OPD.ViewModels
{
    public class CompleteViewModel
    {
        public FilterViewModel FilterViewModel { get; set; }
        public List<DataViewModel> DataViewModel { get; set; }
        public DownloadViewModel DownloadViewModel { get; set; }
        public List<SearchViewModel> SearchViewModel { get; set; }

        public String FilterSettings { get; set; }

    }
}