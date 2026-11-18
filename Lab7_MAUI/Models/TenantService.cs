using System.ComponentModel.DataAnnotations.Schema;

namespace Lab7_MAUI.Models;

[Table("tenant_services")]
public class TenantService
{
    [Column("tenant_id")]
    public int TenantId { get; set; }

    [Column("service_id")]
    public int ServiceId { get; set; }
}
