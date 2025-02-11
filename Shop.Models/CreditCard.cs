using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Models
{


    public class CreditCard
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Card number must be exactly 16 digit long.")]
        public string cardNumber {  get; set; }

        public string month {  get; set; }
        public string year { get; set; }
        public string CVC {  get; set; }

        // בחירה של מפתח אמור להיות פעם אחת, כי זה מפתח קבוע
        public byte[] aesKey {  get; set; }
        // בחירה של וקטור גם אמור להיות פעם אחת!
        public byte[] aesIV { get; set; }

    }

    

}
