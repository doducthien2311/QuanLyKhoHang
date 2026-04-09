using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoLogistics.Models.Entities
{
    [Table("KhuVuc")]
    public class KhuVuc
    {
    [Key, Column(Order = 0)]
    public int KhoId { get; set; }
    public int KhuId { get; set; }

    public string TenKhu { get; set; }
        public string DienTich { get; set; } 
    }
}