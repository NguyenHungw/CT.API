﻿using CT.MOD;
using CT.MOD.TrangChuMOD;
using CT.ULT;
using Microsoft.Data.SqlClient;
using OfficeOpenXml.Drawing.Chart;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace CT.DAL
{
	public class TrangChu_DSSPDAL
	{
		//private string SQLHelper.appConnectionStrings = "Data Source=DESKTOP-PMRM3DP\\SQLEXPRESS;Initial Catalog=CT;Persist Security Info=True;User ID=Hungw;Password=123456;Trusted_Connection=True;Max Pool Size=100";
		SqlConnection SQLCon = null;
		public BaseResultMOD getdssp(int page)
		{
			const int productperpage = 20;
			int startpage = productperpage * (page - 1);
			var result = new BaseResultMOD();
			try
			{
				List<TrangChu_DSSPMOD> dssp = new List<TrangChu_DSSPMOD>();
				using (SqlConnection SQLCon = new SqlConnection(SQLHelper.appConnectionStrings))
				{
					SQLCon.Open();
					SqlCommand cmd = new SqlCommand();
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = @"
select sp.id,sp.MSanPham,sp.Picture , sp.TenSanPham,sp.ID_LoaiSanPham,lsp.TenLoaiSP , gbsp.GiaBan
										from SanPham sp
										left join GiaBanSanPham gbsp on sp.MSanPham = gbsp.MSanPham
									
										left join LoaiSanPham lsp on sp.ID_LoaiSanPham =lsp.ID_LoaiSanPham
										ORDER BY id
										OFFSET @StartPage ROWS
                                        FETCH NEXT @ProductPerPage ROWS ONLY;";

					cmd.Parameters.AddWithValue("@StartPage", startpage);
					cmd.Parameters.AddWithValue("@ProductPerPage", productperpage);
					cmd.Connection = SQLCon;
					cmd.ExecuteNonQuery();
					SqlDataReader read = cmd.ExecuteReader();
					while (read.Read())
					{
                        TrangChu_DSSPMOD item = new TrangChu_DSSPMOD();
						item.id = read.GetInt32(0);
						item.MaSanPham = read.GetString(1);
                        string picture = read.GetString(2);
                        if (picture.EndsWith(".jpg") || picture.EndsWith(".png") || picture.EndsWith(".gif"))
                        {
                            item.Picture = "https://localhost:7177/" + read.GetString(2);
                        }
                        else
                        {
                            // nếu ko phải là kiểu ảnh thì là base64
                            item.Picture = read.GetString(2);
                        }
                        item.TenSanPham = read.GetString(3);
                        item.ID_LoaiSanPham = read.GetInt32(4);
                        item.LoaiSanPham = read.GetString(5);
                  

                        if (!read.IsDBNull(6))
                        {
                            item.Giaban = Convert.ToDecimal(read.GetValue(6));
                        }
                        else
                        {
                            item.Giaban = null;
                        }

                        dssp.Add(item);
					}
					read.Close();
					result.Status = 1;
					result.Data = dssp;


				}
			}
			catch (Exception ex)
			{
				result.Status = -1;
				result.Message = ULT.Constant.API_Error_System;
			}
			return result;
		}
	

		public DanhGiaSanPhamMOD ThongTinDanhGia(int id)
		{
            DanhGiaSanPhamMOD item = null;

			try
			{
				if (SQLCon == null)
				{
					SQLCon = new SqlConnection(SQLHelper.appConnectionStrings);

				}
				if (SQLCon.State == ConnectionState.Closed)
				{
					SQLCon.Open();
				}
				SqlCommand cmd = new SqlCommand();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "SELECT id from DanhGiaSanPham where id ='" + id + "'";
				cmd.Connection = SQLCon;
				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					item = new DanhGiaSanPhamMOD();
					item.id = reader.GetInt32(0);
				}
				reader.Close();
			}
			catch (Exception)
			{
				throw;
			}
			return item;
		}

        public TrangChu_CTSPMOD CTSP(string msp)
        {
            TrangChu_CTSPMOD item = null;
            try
            {
                if (SQLCon == null)
                {
                    SQLCon = new SqlConnection(SQLHelper.appConnectionStrings);
                }
                if (SQLCon.State == ConnectionState.Closed)
                {
                    SQLCon.Open();
                }
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"select sp.id,sp.MSanPham,sp.Picture , sp.TenSanPham,SUM(ctn.SoLuong) as SoLuong,SUM(ctn.TongSoLuong) as TongSoLuong, gbsp.GiaBan
									from SanPham sp
									left join GiaBanSanPham gbsp on sp.MSanPham = gbsp.MSanPham
									left join ChiTietNhap ctn on sp.MSanPham = ctn.MSanPham
                                    where sp.MSanPham = @MSanPham
                                    GROUP BY 
                                        sp.id,
                                        sp.MSanPham,
                                        sp.Picture,
                                        sp.TenSanPham,
                                        gbsp.GiaBan;";
                cmd.Parameters.AddWithValue("@MSanPham", msp);
                cmd.Connection = SQLCon;
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    item = new TrangChu_CTSPMOD();
					item.id = reader.GetInt32(0);
                    item.MaSanPham = msp;
                    string picture = reader.GetString(2);
                    if (picture.EndsWith(".jpg") || picture.EndsWith(".png") || picture.EndsWith(".gif"))
                    {
                        item.Picture = "https://localhost:7177/" + reader.GetString(2);
                    }
                    else
                    {
                        // nếu ko phải là kiểu ảnh thì là base64
                        item.Picture = reader.GetString(2);
                    }
                    item.TenSanPham = reader.GetString(3);

                    if (!reader.IsDBNull(4))
                    {
                        item.SoLuong = Convert.ToInt32(reader.GetValue(4));
                    }
                    else
                    {
                        item.SoLuong = null;
                    }
                    if (!reader.IsDBNull(5))
                    {
                        item.TongSoLuong = Convert.ToInt32(reader.GetValue(5));
                    }
                    else
                    {
                        item.TongSoLuong = null;
                    }
                    if (!reader.IsDBNull(6))
                    {
                        item.GiaBan = Convert.ToDecimal(reader.GetValue(6));
                    }
                    else
                    {
                        item.GiaBan = null;
                    }


                }
                reader.Close();

            }
            catch (Exception)
            {
                throw;
            }
            return item;
        }
     
    }
}
