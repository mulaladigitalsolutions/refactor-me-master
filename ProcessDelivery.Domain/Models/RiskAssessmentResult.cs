using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcessDelivery.Domain.Enums;

namespace ProcessDelivery.Domain.Models
{
    public class RiskAssessmentResult
    {
        public RiskLevel Level { get; set; }
        public required string Reason { get; set; }
        public override string ToString() => $"{Level}Risk: {Reason}";
    }

}
