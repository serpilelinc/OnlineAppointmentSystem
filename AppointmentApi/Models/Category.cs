using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentApi.Models
{
    [Table("Category")]
    public class Category : ISoftDeletable
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<AppointmentServiceType> ServiceTypes { get; set; }
            = new();

        public bool IsDeleted { get; set; } = false;
    }
}