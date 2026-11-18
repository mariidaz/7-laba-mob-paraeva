using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab7_MAUI.Models;

[Table("apartments")]
public class Apartment
{
    [Key]
    [Column("apartment_id")]
    public int ApartmentId { get; set; }

    [Column("tenant_id")]
    public int TenantId { get; set; }

    [Column("address")]
    [Required]
    public string Address { get; set; } = string.Empty;

    [Column("account_number")]
    [Required]
    public string AccountNumber { get; set; } = string.Empty;

    [Column("residents_count")]
    public int ResidentsCount { get; set; }

    [Column("area")]
    public decimal Area { get; set; }

    [NotMapped]
    public string TenantName { get; set; } = string.Empty;
}
