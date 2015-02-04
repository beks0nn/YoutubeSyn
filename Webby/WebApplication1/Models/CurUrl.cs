using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class CurrUrl
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Id { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public int UrlIdentity { get; set; }
    }
}