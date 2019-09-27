using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Models.RepairModels
{
    public class QryRepListData
    {
        public string BMEDqtyDOCID { get; set; }
        public string BMEDqtyASSETNO { get; set; }
        public string BMEDqtyACCDPT { get; set; }
        public string BMEDqtyASSETNAME { get; set; }
        public string BMEDqtyFLOWTYPE { get; set; }
        public string BMEDqtyDPTID { get; set; }
        public string BMEDqtyDealStatus { get; set; }
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public string BMEDqtyApplyDateFrom { get; set; }
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public string BMEDqtyApplyDateTo { get; set; }
        public string BMEDqtyDateType { get; set; }
        public string BMEDqtyIsCharged { get; set; }
        public bool BMEDqtySearchAllDoc { get; set; }
        public string BMEDqtyEngCode { get; set; }
        public string BMEDqtyTicketNo { get; set; }
        public string BMEDqtyVendor { get; set; }
    }
}
