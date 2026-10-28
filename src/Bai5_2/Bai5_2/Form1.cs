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

namespace Bai5_2
{
    public partial class Form1 : Form
    {
        // Chuỗi kết nối đúng với SQL Server trên máy của bạn
        private readonly string connectionString = @"Data Source=DESKTOP-VMU8SL9\SQLEXPRESS;Initial Catalog=QuanLyHeThong;Integrated Security=True;TrustServerCertificate=True";

        public Form1()
        {
            InitializeComponent();

            // 1. Cài đặt định dạng Mask và Ngày sinh ngay khi Form khởi tạo
            mtxtPhone.Mask = "(000) 000-0000";
            dtpBirthDate.Format = DateTimePickerFormat.Custom;
            dtpBirthDate.CustomFormat = "dd/MM/yyyy";

            // 2. Tự động nạp dữ liệu Khóa học vào ComboBox ngay khi mở Form
            LoadCoursesToComboBox();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        #region 1. Nạp danh mục Khóa học vào ComboBox (DisplayMember & ValueMember)
        private void LoadCoursesToComboBox()
        {
            string query = "SELECT CourseID, CourseName FROM Courses";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Gán nguồn dữ liệu cho ComboBox
                        cboCourse.DataSource = dt;
                        // Tên khóa học hiển thị cho người dùng nhìn thấy
                        cboCourse.DisplayMember = "CourseName";
                        // Giá trị thực tế phía sau là Mã khóa học (CourseID)
                        cboCourse.ValueMember = "CourseID";

                        if (cboCourse.Items.Count > 0)
                        {
                            cboCourse.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL khi tải khóa học: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 2. Kiểm tra tính hợp lệ dữ liệu (Validation)
        private bool ValidateFormInput()
        {
            bool isValid = true;
            errorProvider1.Clear(); // Xóa sạch các dấu cảnh báo cũ

            // Kiểm tra Họ tên
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                errorProvider1.SetError(txtFullName, "Vui lòng nhập họ và tên!");
                isValid = false;
            }

            // Kiểm tra MaskedTextBox SĐT đã nhập đủ 10 số hay chưa
            if (!mtxtPhone.MaskFull)
            {
                errorProvider1.SetError(mtxtPhone, "Vui lòng nhập đầy đủ 10 số điện thoại theo định dạng (000) 000-0000!");
                isValid = false;
            }

            // Kiểm tra Ngày sinh (phải nhỏ hơn ngày hiện tại)
            if (dtpBirthDate.Value.Date >= DateTime.Now.Date)
            {
                errorProvider1.SetError(dtpBirthDate, "Ngày sinh phải nhỏ hơn ngày hiện tại!");
                isValid = false;
            }

            // Kiểm tra Khóa học đã chọn chưa
            if (cboCourse.SelectedValue == null)
            {
                errorProvider1.SetError(cboCourse, "Vui lòng chọn khóa học!");
                isValid = false;
            }

            return isValid;
        }
        #endregion

        #region 3. Nút Đăng ký: Tổng hợp thông tin, in lên MessageBox và Lưu vào Database
        private void btnRegister_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra validation
            if (!ValidateFormInput())
            {
                return;
            }

            // 2. Thu thập dữ liệu từ các Control
            string fullName = txtFullName.Text.Trim();
            string phone = mtxtPhone.Text; // Chứa định dạng (xxx) xxx-xxxx
            string birthDateStr = dtpBirthDate.Value.ToString("dd/MM/yyyy");
            string gender = radMale.Checked ? "Nam" : "Nữ";
            string selectedCourseId = cboCourse.SelectedValue.ToString();
            string selectedCourseName = cboCourse.Text;
            bool isReceiveEmail = chkEmailNotify.Checked;

            // 3. Tổng hợp và in thông tin đăng ký lên MessageBox theo yêu cầu đề bài
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== THÔNG TIN ĐĂNG KÝ HỌC VIÊN ===");
            sb.AppendLine($"• Họ và tên: {fullName}");
            sb.AppendLine($"• Số điện thoại: {phone}");
            sb.AppendLine($"• Ngày sinh: {birthDateStr}");
            sb.AppendLine($"• Giới tính: {gender}");
            sb.AppendLine($"• Khóa học: {selectedCourseName} (Mã: {selectedCourseId})");
            sb.AppendLine($"• Nhận thông báo Email: {(isReceiveEmail ? "Có đăng ký" : "Không")}");
            sb.AppendLine("-----------------------------------");
            sb.AppendLine("Bạn có muốn lưu thông tin này vào Cơ sở dữ liệu không?");

            DialogResult confirm = MessageBox.Show(sb.ToString(), "Xác nhận Đăng ký", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            // 4. Nếu người dùng chọn Yes -> Thực hiện lưu vào CSDL SQL Server
            if (confirm == DialogResult.Yes)
            {
                SaveRegistrationToDatabase(fullName, phone, dtpBirthDate.Value.Date, gender, selectedCourseId, isReceiveEmail);
            }
        }

        private void SaveRegistrationToDatabase(string fullName, string phone, DateTime birthDate, string gender, string courseId, bool receiveEmail)
        {
            string insertQuery = @"INSERT INTO StudentRegistrations (FullName, Phone, BirthDate, Gender, CourseID, ReceiveEmail)
                                  VALUES (@FullName, @Phone, @BirthDate, @Gender, @CourseID, @ReceiveEmail)";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName;
                        cmd.Parameters.Add("@Phone", SqlDbType.VarChar, 20).Value = phone;
                        cmd.Parameters.Add("@BirthDate", SqlDbType.Date).Value = birthDate;
                        cmd.Parameters.Add("@Gender", SqlDbType.NVarChar, 10).Value = gender;
                        cmd.Parameters.Add("@CourseID", SqlDbType.NVarChar, 20).Value = courseId;
                        cmd.Parameters.Add("@ReceiveEmail", SqlDbType.Bit).Value = receiveEmail;

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            MessageBox.Show("Đã lưu thông tin đăng ký vào Cơ sở dữ liệu thành công!",
                                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ResetForm();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi kết nối hoặc thực thi CSDL: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 4. Nút Nhập lại (Reset Form)
        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        private void ResetForm()
        {
            txtFullName.Clear();
            mtxtPhone.Clear();
            dtpBirthDate.Value = DateTime.Now.AddYears(-18); // Đặt mặc định 18 tuổi
            radMale.Checked = true;
            if (cboCourse.Items.Count > 0) cboCourse.SelectedIndex = 0;
            chkEmailNotify.Checked = false;
            errorProvider1.Clear();
            txtFullName.Focus();
        }
        #endregion

        #region Các sự kiện giao diện (Events)
        private void txtFullName_TextChanged(object sender, EventArgs e)
        {

        }

        private void mtxtPhone_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void dtpBirthDate_ValueChanged(object sender, EventArgs e)
        {

        }

        private void cboCourse_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void radMale_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radFemale_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkEmailNotify_CheckedChanged(object sender, EventArgs e)
        {

        }
        #endregion
    }
}