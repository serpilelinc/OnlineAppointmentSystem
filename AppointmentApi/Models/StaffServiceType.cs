namespace AppointmentApi.Models
{
    public class StaffServiceType : ISoftDeletable
    {
        public int Id { get; set; }

        public int StaffId { get; set; }

        public Staff? Staff { get; set; }

        public int ServiceTypeId { get; set; }

        public AppointmentServiceType? ServiceType { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}