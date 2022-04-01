using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace SIMTech.APS.Integration.API.Models
{
    using SIMTech.APS.Models;

    [Table("PPOrder")]
    public partial class Pporder
    {
        public Pporder()
        {
            PporderDetails = new HashSet<PporderDetail>();
            PporderRoutes = new HashSet<PporderRoute>();
        }

        [Key]
        public int Id { get; set; }
        [Column("MeltingOrderID")]
        public int? MeltingOrderId { get; set; }
        [StringLength(50)]
        public string GoldContent { get; set; }
        [StringLength(30)]
        public string Size { get; set; }
        [StringLength(50)]
        public string BasicChainType { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AllocWeight { get; set; }
        [StringLength(50)]
        public string Description { get; set; }
        [StringLength(1)]
        public string Type { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? IssueDate { get; set; }
        [Column("status")]
        [StringLength(10)]
        public string Status { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? GenDate { get; set; }

        [InverseProperty(nameof(PporderDetail.Pp))]
        public virtual ICollection<PporderDetail> PporderDetails { get; set; }
        [InverseProperty(nameof(PporderRoute.Pp))]
        public virtual ICollection<PporderRoute> PporderRoutes { get; set; }
    }
}
