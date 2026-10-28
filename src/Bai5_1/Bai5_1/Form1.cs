using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bai5_1
{
    public partial class Form1 : Form
    {
        // Chuỗi kết nối đúng với SQL Server trên máy của bạn
        private readonly string connectionString = @"Data Source=DESKTOP-VMU8SL9\SQLEXPRESS;Initial Catalog=QuanLyHeThong;Integrated Security=True";

        public Form1()
        {
            InitializeComponent();
            SetupFormFeatures();
        }

        /// <summary>
        /// Cấu hình phím tắt và chế độ ban đầu cho Form
        /// </summary>
        private void SetupFormFeatures()
        {
            // Yêu cầu: Phím Enter kích hoạt btnLogin, phím Esc kích hoạt btnExit
            this.AcceptButton = btnLogin;
            this.CancelButton = btnExit;

            // Yêu cầu: Mặc định ẩn mật khẩu
            txtPassword.UseSystemPasswordChar = true;
            chkShowPassword.Checked = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {

        }

        #region Yêu cầu 1: Ẩn / Hiện mật khẩu (chkShowPassword)
        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            // Tick chọn (Checked = true)  -> Hiện mật khẩu (UseSystemPasswordChar = false)
            // Bỏ chọn (Checked = false) -> Ẩn mật khẩu (UseSystemPasswordChar = true)
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }
        #endregion

        #region Yêu cầu 2: Validation ErrorProvider không để trống
        private bool ValidateLoginInput()
        {
            bool isValid = true;
            errorProvider1.Clear(); // Xóa sạch các dấu đỏ cũ trước khi kiểm tra

            // Kiểm tra Tên đăng nhập
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                errorProvider1.SetError(txtUsername, "Tên đăng nhập không được để trống!");
                isValid = false;
            }

            // Kiểm tra Mật khẩu
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                errorProvider1.SetError(txtPassword, "Mật khẩu không được để trống!");
                isValid = false;
            }

            return isValid;
        }
        #endregion

        #region Yêu cầu 3: Kiểm tra Đăng nhập từ CSDL SQL Server (QuanLyHeThong)
        private void btnLogin_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra validation trước khi kết nối CSDL
            if (!ValidateLoginInput())
            {
                return;
            }

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            // 2. Sử dụng SqlParameter để bảo mật chống SQL Injection
            string query = "SELECT FullName, Role FROM Users WHERE Username = @Username AND Password = @Password";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                        cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 100).Value = password;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string fullName = reader["FullName"].ToString();
                                string role = reader["Role"].ToString();

                                MessageBox.Show($"Đăng nhập thành công!\nXin chào: {fullName} ({role})",
                                                "Thông báo",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Information);

                                this.DialogResult = DialogResult.OK;
                            }
                            else
                            {
                                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác!",
                                                "Đăng nhập thất bại",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);

                                txtPassword.Clear();
                                txtPassword.Focus();
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL SQL Server: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Yêu cầu 4: Nút Thoát (btnExit)
        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có muốn thoát chương trình không?",
                                                  "Xác nhận thoát",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        #endregion
    }
}