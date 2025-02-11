using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace Shop.Models
{
    public class Product
    {


        [Key]
        public int id { get; set; }
        [Required]
        [DisplayName("Game Name")]
        public string GameName { get; set; }
        [Required]
        public string Description {  get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Price must have up to two decimal places.")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public float Price { get; set; }

        [Required]
        public int ReleaseYear { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }
        //[Required(ErrorMessage = "Please select an Image.")]
        //[DataType(DataType.Upload)]
        [ValidateNever]

        public string ImageUrl {  get; set; }
        [ValidateNever]
        public String Platform { get; set; }
        [ValidateNever]
        public int Stock {  get; set; }
        [ValidateNever]
        public int PEGI { get; set; }
        [ValidateNever]
        public int SoldSoFar {  get; set; }
        [ValidateNever]
        public bool BestSeller { get; set; }


        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100.")]
        public int? Discount { get; set; } = 0;
        [ValidateNever]

        public string SKU {  get; set; }
    }
}

