using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BUS;
using DTO;

namespace GUI
{
    public class frmAtmStations : Form
    {
        private readonly AtmBUS _bus = new AtmBUS();

        private DataGridView dgvStations;
        private TextBox txtStationId;
        private TextBox txtAddress;
        private TextBox txtCurrentCash;
        private ComboBox cboStatus;
        private TextBox txtImagePath;
        private PictureBox picStation;

        private Button btnChooseImage;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnRefresh;

        public frmAtmStations()
        {
            InitializeLayout();
            BindEvents();
            LoadStationsData();
        }

        private void InitializeLayout()
        {
            this.Text = "Danh sách Trạm ATM & Tồn quỹ";
            this.Size = new Size(920, 520);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(20, 20);

            dgvStations = new DataGridView
            {
                Location = new Point(16, 16),
                Size = new Size(560, 445),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            GroupBox grpDetail = new GroupBox
            {
                Text = "Thông tin chi tiết Trạm ATM",
                Location = new Point(592, 16),
                Size = new Size(295, 445)
            };

            Label lblId = new Label { Text = "Mã trạm:", Location = new Point(14, 28), AutoSize = true };
            txtStationId = new TextBox { Location = new Point(14, 48), Width = 265 };

            Label lblAddr = new Label { Text = "Địa chỉ đặt máy:", Location = new Point(14, 78), AutoSize = true };
            txtAddress = new TextBox { Location = new Point(14, 98), Width = 265 };

            Label lblCash = new Label { Text = "Tồn quỹ hiện tại (VNĐ):", Location = new Point(14, 128), AutoSize = true };
            txtCurrentCash = new TextBox { Location = new Point(14, 148), Width = 265 };

            Label lblSts = new Label { Text = "Trạng thái hoạt động:", Location = new Point(14, 178), AutoSize = true };
            cboStatus = new ComboBox
            {
                Location = new Point(14, 198),
                Width = 265,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboStatus.Items.AddRange(new object[] { "Đang hoạt động", "Sắp hết tiền", "Bảo trì", "Mất kết nối" });
            cboStatus.SelectedIndex = 0;

            btnChooseImage = new Button
            {
                Text = "Đính kèm ảnh hiện trường...",
                Location = new Point(14, 232),
                Width = 265,
                Height = 28
            };

            txtImagePath = new TextBox
            {
                Location = new Point(14, 265),
                Width = 265,
                ReadOnly = true
            };

            picStation = new PictureBox
            {
                Location = new Point(14, 294),
                Size = new Size(265, 98),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            btnAdd = new Button { Text = "Thêm mới", Location = new Point(14, 402), Size = new Size(84, 30) };
            btnUpdate = new Button { Text = "Cập nhật", Location = new Point(104, 402), Size = new Size(84, 30) };
            btnRefresh = new Button { Text = "Làm mới", Location = new Point(195, 402), Size = new Size(84, 30) };

            grpDetail.Controls.AddRange(new Control[]
            {
                lblId, txtStationId,
                lblAddr, txtAddress,
                lblCash, txtCurrentCash,
                lblSts, cboStatus,
                btnChooseImage, txtImagePath, picStation,
                btnAdd, btnUpdate, btnRefresh
            });

            this.Controls.Add(dgvStations);
            this.Controls.Add(grpDetail);
        }

        private void BindEvents()
        {
            dgvStations.SelectionChanged += DgvStations_SelectionChanged;
            btnChooseImage.Click += BtnChooseImage_Click;
            btnAdd.Click += (s, e) => SaveStation(true);
            btnUpdate.Click += (s, e) => SaveStation(false);
            btnRefresh.Click += (s, e) =>
            {
                ClearInputs();
                LoadStationsData();
            };
        }

        private void LoadStationsData()
        {
            try
            {
                dgvStations.DataSource = _bus.GetAllStations();
                if (dgvStations.Columns["StationId"] != null) dgvStations.Columns["StationId"].HeaderText = "Mã trạm";
                if (dgvStations.Columns["Address"] != null) dgvStations.Columns["Address"].HeaderText = "Địa chỉ đặt máy";
                if (dgvStations.Columns["CurrentCash"] != null)
                {
                    dgvStations.Columns["CurrentCash"].HeaderText = "Tồn quỹ (VNĐ)";
                    dgvStations.Columns["CurrentCash"].DefaultCellStyle.Format = "N0";
                }
                if (dgvStations.Columns["Status"] != null) dgvStations.Columns["Status"].HeaderText = "Trạng thái";
                if (dgvStations.Columns["ImagePath"] != null) dgvStations.Columns["ImagePath"].HeaderText = "Đường dẫn ảnh";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu trạm ATM: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvStations_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStations.CurrentRow?.DataBoundItem is AtmStationDTO selected)
            {
                txtStationId.Text = selected.StationId;
                txtAddress.Text = selected.Address;
                txtCurrentCash.Text = selected.CurrentCash.ToString("0");
                cboStatus.SelectedItem = selected.Status;
                txtImagePath.Text = selected.ImagePath;
                DisplayPreviewImage(selected.ImagePath);
            }
        }

        private void BtnChooseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn ảnh chụp hiện trường trạm ATM";
                ofd.Filter = "Hình ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtImagePath.Text = ofd.FileName;
                    DisplayPreviewImage(ofd.FileName);
                }
            }
        }

        private void DisplayPreviewImage(string path)
        {
            if (picStation.Image != null)
            {
                picStation.Image.Dispose();
                picStation.Image = null;
            }

            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                using (var tempImg = Image.FromFile(path))
                {
                    picStation.Image = new Bitmap(tempImg);
                }
            }
        }

        private void SaveStation(bool isNew)
        {
            try
            {
                if (!decimal.TryParse(txtCurrentCash.Text.Trim(), out decimal cash))
                {
                    MessageBox.Show("Vui lòng nhập số tiền tồn quỹ hợp lệ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var dto = new AtmStationDTO
                {
                    StationId = txtStationId.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    CurrentCash = cash,
                    Status = cboStatus.SelectedItem?.ToString() ?? "Đang hoạt động",
                    ImagePath = txtImagePath.Text.Trim()
                };

                if (_bus.SaveStation(dto, isNew))
                {
                    MessageBox.Show(isNew ? "Thêm trạm ATM thành công." : "Cập nhật trạm ATM thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStationsData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thao tác thất bại: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearInputs()
        {
            txtStationId.Clear();
            txtAddress.Clear();
            txtCurrentCash.Clear();
            cboStatus.SelectedIndex = 0;
            txtImagePath.Clear();
            DisplayPreviewImage(null);
        }
    }
}