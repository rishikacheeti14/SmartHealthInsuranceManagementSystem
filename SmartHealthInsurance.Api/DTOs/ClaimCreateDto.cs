using System;

namespace SmartHealthInsurance.Api.DTOs
{
    public class ClaimCreateDto
    {
        public int PolicyId { get; set; }
        public int TreatmentId { get; set; }
        public decimal ClaimAmount { get; set; }
    }
}
