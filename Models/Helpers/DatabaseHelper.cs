using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration; // Thêm thư viện này
using System.IO;

namespace QuanLyKhoLogistics.Models.Helpers
{
    public static class DatabaseHelper
    {
       
      
        private static string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        return config.GetConnectionString("DefaultConnection");
    }
        // Thêm chữ static vào trước void
public static void XuatKho(int phieuId, int spId, int viTriId, int soLuong, decimal gia)
{
    /// Sửa connectionString thành GetConnectionString()
    using (SqlConnection conn = new SqlConnection(GetConnectionString())) 
    {
        SqlCommand cmd = new SqlCommand("sp_XuatKho", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@PhieuXuatID", phieuId);
        cmd.Parameters.AddWithValue("@SanPhamID", spId);
        cmd.Parameters.AddWithValue("@ViTriID", viTriId);
        cmd.Parameters.AddWithValue("@SoLuongXuat", soLuong);
        cmd.Parameters.AddWithValue("@DonGia", gia);

        conn.Open();
        cmd.ExecuteNonQuery();
    }
}

        public static DataTable LayDuLieu(string query, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            // Gọi hàm GetConnectionString() thay vì dùng biến tĩnh
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null) cmd.Parameters.AddRange(parameters);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("--- LỖI SELECT SQL ---");
                    Console.WriteLine(ex.Message);
                }
            }
            return dt;
        }

        public static bool ThucThiLenh(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null) cmd.Parameters.AddRange(parameters);
                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("--- LỖI INSERT/UPDATE SQL ---");
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
       public static SanPhamTonKho GetChiTietO(int viTriId)
    {
        // 2. SỬA: Đổi dbo.ViTriO thành dbo.OKho để khớp với 1600 ô bạn đã tạo
        string query = @"SELECT sp.MaSKU, sp.TenSP, dm.TenDanhMuc, tk.SoLuong, sp.DVT, vt.MaViTri
                         FROM dbo.TonKho tk
                         JOIN dbo.SanPham sp ON tk.SanPhamID = sp.SanPhamID
                         JOIN dbo.DanhMuc dm ON sp.DanhMucID = dm.DanhMucID
                         JOIN dbo.OKho vt ON tk.ViTriID = vt.ViTriID 
                         WHERE tk.ViTriID = @ViTriID";

        SqlParameter[] param = { new SqlParameter("@ViTriID", viTriId) };
        DataTable dt = LayDuLieu(query, param);

        if (dt != null && dt.Rows.Count > 0)
        {
            DataRow dr = dt.Rows[0];
            return new SanPhamTonKho 
            {
                MaSKU = dr["MaSKU"].ToString(),
                TenSP = dr["TenSP"].ToString(),
                TenDanhMuc = dr["TenDanhMuc"].ToString(),
                SoLuong = dr["SoLuong"] != DBNull.Value ? Convert.ToInt32(dr["SoLuong"]) : 0,
                DVT = dr["DVT"].ToString(),
                MaViTri = dr["MaViTri"].ToString()
            };
        }
        return null; 
    }
    }
}