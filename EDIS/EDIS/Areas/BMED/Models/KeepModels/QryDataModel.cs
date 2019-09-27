using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Models.KeepModels
{
    public class QryKeepListData
    {
        public string BMEDKqtyDOCID { get; set; }
        public string BMEDKqtyASSETNO { get; set; }
        public string BMEDKqtyACCDPT { get; set; }
        public string BMEDKqtyASSETNAME { get; set; }
        public string BMEDKqtyFLOWTYPE { get; set; }
        public string BMEDKqtyDPTID { get; set; }
        public string BMEDKqtyKeepResult { get; set; }
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public string BMEDKqtyApplyDateFrom { get; set; }
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public string BMEDKqtyApplyDateTo { get; set; }
        public string BMEDKqtyDateType { get; set; }
        public string BMEDKqtyIsCharged { get; set; }
        public bool BMEDKqtySearchAllDoc { get; set; }
        public string BMEDKqtyEngCode { get; set; }
        public string BMEDKqtyTicketNo { get; set; }
        public string BMEDKqtyVendor { get; set; }
    }
}
