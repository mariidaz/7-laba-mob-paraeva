namespace Lab7_MAUI.Models;

public class BillItemViewModel
{
    public string ServiceName { get; set; } = string.Empty;
    public string CalculationType { get; set; } = string.Empty;
    public decimal Tariff { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal ConsumptionVolume { get; set; }
    public decimal TotalAmount { get; set; }

    public string FormattedTariff => $"{Tariff:F2} грн/{Unit}";
    public string FormattedVolume => $"{ConsumptionVolume:F2} {Unit}";
    public string FormattedTotal => $"{TotalAmount:F2} грн";
}

public class BillViewModel
{
    public int ApartmentId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string TenantFullName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int ResidentsCount { get; set; }
    public decimal Area { get; set; }
    public DateTime BillDate { get; set; } = DateTime.Now;
    public List<BillItemViewModel> Items { get; set; } = new();
    public decimal GrandTotal => Items.Sum(i => i.TotalAmount);

    public string FormattedDate => BillDate.ToString("dd.MM.yyyy HH:mm");
    public string FormattedPeriod => BillDate.ToString("MMMM yyyy");
    public string FormattedGrandTotal => $"{GrandTotal:F2} грн";
}
