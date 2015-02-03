using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Uri
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Id { get; set; }

        public string UrlPart { get; set; }

        // Foreign key to customer
        [ForeignKey("CurrentUrl")]
        public int UrlIdentity { get; set; }
        public virtual CurrUrl CurrentUrl { get; set; }
    }
}

