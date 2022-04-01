using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SIMTech.APS.Integration.RabbitMQ.Models
{
    public class MachineBlockOut
    {
        public int machine_id { get; set; }
        public string machine_name { get; set; }

        public DateTime? time_of_occurs { get; set; }
        
        public DateTime? start_time { get; set; }

        public DateTime? end_time { get; set; }
        public string reasons_for_failure { get; set; }

        public List<string> work_order { get; set; }

    }

    public class MachineBlockOutPM
    {
      

        [Key]
        public int EquipmentBlockOutID { get; set; }

        public int EquipmentID { get; set; }

        public string BlockOutType { get; set; }
        public string EquipmentName { get; set; }
        public string Operator { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Remarks { get; set; }
        public int Value { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }


    }


}
