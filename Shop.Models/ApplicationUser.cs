using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Models
{
    public class ApplicationUser:IdentityUser
    {
        [Required]
        public string name {  get; set; }
        public String? StreetAddress {  get; set; }
        public String? City { get; set; }
        public String? State { get; set; }
        public String? ZipCode { get; set;}

    }
}
