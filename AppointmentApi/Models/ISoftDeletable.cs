namespace AppointmentApi.Models
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
    }
}
