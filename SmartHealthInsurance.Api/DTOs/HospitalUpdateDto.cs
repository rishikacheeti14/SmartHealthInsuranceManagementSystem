namespace SmartHealthInsurance.Api.DTOs
{
    public class HospitalUpdateDto
    {
        public string HospitalName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string ZipCode { get; set; } = null!;
        public bool IsNetworkProvider { get; set; }
        public int UserId { get; set; } 
    }
}
