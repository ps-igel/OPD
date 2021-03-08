using System.ComponentModel.DataAnnotations;

namespace OPD.Models
{
    public class ParticleMetadata
    {
        [Display(Name = "Volume")]
        public int volume { get; set; }

        [Display(Name = "Surface")]
        public int surface { get; set; }

        [Display(Name = "Object voxels")]
        public int nr_object_voxels { get; set; }

        [Display(Name = "Surface Voxels")]
        public int nr_surface_voxels { get; set; }

        [Display(Name ="X")]
        public decimal x { get; set; }

        [Display(Name = "Y")]
        public decimal y { get; set; }

        [Display(Name = "Z")]
        public decimal z { get; set; }

        [Display(Name = "Mean dist. to surface")]
        public decimal mean_distance_to_surface { get; set; }

        [Display(Name = "Mean stddev. to surface")]
        public decimal std_distance_to_surface { get; set; }

        [Display(Name = "Median dist. to surface")]
        public decimal median_distance_to_surface { get; set; }

        [Display(Name = "XM")]
        public decimal xm { get; set; }

        [Display(Name = "YM")]
        public decimal ym { get; set; }

        [Display(Name = "ZM")]
        public decimal zm { get; set; }

        [Display(Name = "BX")]
        public int bx { get; set; }

        [Display(Name = "BY")]
        public int by { get; set; }

        [Display(Name = "BZ")]
        public int bz { get; set; }

        [Display(Name = "B-Width")]
        public int b_width { get; set; }

        [Display(Name = "B-Height")]
        public int b_height { get; set; }

        [Display(Name = "B-Depth")]
        public int b_depth { get; set; }
    }
}