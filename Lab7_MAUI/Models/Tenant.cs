using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab7_MAUI.Models;

[Table("tenants")]
public class Tenant
{
    [Key]
    [Column("tenant_id")]
    public int TenantId { get; set; }

    [Column("last_name")]
    [Required]
    public string LastName { get; set; } = string.Empty;

    [Column("first_name")]
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Column("middle_name")]
    public string MiddleName { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    [NotMapped]
    public string ShortName => $"{LastName} {(!string.IsNullOrEmpty(FirstName) ? FirstName[0] + "." : "")} {(!string.IsNullOrEmpty(MiddleName) ? MiddleName[0] + "." : "")}".Trim();

    [NotMapped]
    public string Initials => $"{(!string.IsNullOrEmpty(LastName) ? LastName[0].ToString().ToUpper() : "")}{(!string.IsNullOrEmpty(FirstName) ? FirstName[0].ToString().ToUpper() : "")}";
}
