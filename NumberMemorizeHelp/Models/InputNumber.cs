using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NumberMemorizeHelp.Models
{
    public class InputNumber
    {
        [Required]
        [MaxLength(100)]
        //[RegularExpression(@"^\d$", ErrorMessage = "Input must be a whole number")]
        public string Number { get; set; }


    }
}