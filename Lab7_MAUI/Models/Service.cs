using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab7_MAUI.Models;

[Table("services")]
public class Service
{
    [Key]
    [Column("service_id")]
    public int ServiceId { get; set; }

    [Column("service_name")]
    [Required]
    public string ServiceName { get; set; } = string.Empty;

    [Column("price_per_square_meter")]
    public decimal? PricePerSquareMeter { get; set; }

    [Column("price_per_person")]
    public decimal? PricePerPerson { get; set; }

    [Column("calculation_type")]
    public string CalculationType { get; set; } = "за площею";

    [NotMapped]
    public string TariffDisplay => PricePerSquareMeter.HasValue && PricePerSquareMeter > 0
        ? $"{PricePerSquareMeter:F2} грн / кв. м"
        : (PricePerPerson.HasValue && PricePerPerson > 0 ? $"{PricePerPerson:F2} грн / особу" : "Не встановлено");
}
