using System;
using System.ComponentModel.DataAnnotations;

public class HousePriceChangeLog
{
    [Key]  // <-- BU SATIR GEREKLİ
    public int LogId { get; set; }
    public int HouseId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangedAt { get; set; }
}
