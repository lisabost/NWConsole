using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NorthwindConsole.Model
{
    public partial class Categories
    {
        public Categories()
        {
            Products = new HashSet<Products>();
        }

        public int CategoryId { get; set; }
        [Required]
        public string CategoryName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Products> Products { get; set; }
    }
}
