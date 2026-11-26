using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using BUS;
using DTO;

namespace GUI
{
    public class frmAtmReconciliation : Form
    {
        private readonly AtmBUS _bus = new AtmBUS();
        private CancellationTokenSource _cts;

        private Button btnStartReconcile;
        private Button btnStopReconcile;
        private ProgressBar prgLocal;
        private Label lblStatus;
        private DataGridView dgvHistory;

        public frmAtmReconciliation()
        {
            InitializeLayout();
            BindEvents();
            LoadReconciliationHistory();
        }

        private void InitializeLayout()
        {
            this.Text = "Đối soát Giao dịch Cuối ngày (Async/Await)";
            this.Size = new Size(820, 480);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(60, 60);

            btnStartReconcile = new Button
            {
                Text = "Chạy Đối Soát Tự Động",
                Location = new Point(16, 16),
                Size = new Size(190, 36),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnStopReconcile = new Button
            {
                Text = "Dừng Đối Soát",
                Location = new Point(218, 16),
                Size = new Size(150, 36),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            lblStatus = new Label
            {
                Text = "Trạng thái: Chờ kích hoạt phiên đối soát sao kê ATM...",
                Location = new Point(16, 64),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            prgLocal = new ProgressBar
            {
                Location = new Point(16, 90),
                Size = new Size(770, 22),
                Minimum = 0,
                Maximum = 100
            };

            dgvHistory = new DataGridView
            {
                Location = new Point(16, 126),
                Size = new Size(770, 300),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[]
            {
                btnStartReconcile,
                btnStopReconcile,
                lblStatus,
                prgLocal,
                dgvHistory
            });
        }

        private void BindEvents()
        {
            btnStartReconcile.Click += BtnStartReconcile_Click;
            btnStopReconcile.Click += BtnStopReconcile_Click;
            this.FormClosing += (s, e) => _cts?.Cancel();
        }

        private async void BtnStartReconcile_Click(object sender, EventArgs e)
        {
            btnStartReconcile.Enabled = false;
            btnStopReconcile.Enabled = true;
            prgLocal.Value = 0;
            lblStatus.ForeColor = Color.DarkBlue;
            lblStatus.Text = "Đang tải 250.000 giao dịch từ máy chủ Core Banking...";

            _cts = new CancellationTokenSource();
            var mainForm = this.MdiParent as frmBankAtmMain;

            var progress = new Progress<int>(percent =>
            {
                prgLocal.Value = percent;
                lblStatus.Text = $"Đang đối chiếu sao kê ATM: {percent}% (UI Thread vẫn hoạt động mượt)...";
                mainForm?.UpdateReconciliationProgress(percent, $"Đối soát ATM: {percent}%");
            });

            try
            {
                ReconciliationResultDTO result = await _bus.RunEndOfDayReconciliationAsync(progress, _cts.Token);

                lblStatus.ForeColor = Color.DarkGreen;
                lblStatus.Text = $"Hoàn tất! Tổng GD: {result.TotalTransactions:N0} | Khớp: {result.MatchedCount:N0} | Lệch: {result.DiscrepancyCount}";
                mainForm?.UpdateReconciliationProgress(100, "Đối soát hoàn tất");
                LoadReconciliationHistory();
            }
            catch (OperationCanceledException)
            {
                lblStatus.ForeColor = Color.DarkOrange;
                lblStatus.Text = "Tiến trình đối soát đã bị người dùng hủy ngang.";
                mainForm?.UpdateReconciliationProgress(0, "Đã hủy đối soát");
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "Lỗi đối soát: " + ex.Message;
                mainForm?.UpdateReconciliationProgress(0, "Lỗi đối soát");
            }
            finally
            {
                btnStartReconcile.Enabled = true;
                btnStopReconcile.Enabled = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void BtnStopReconcile_Click(object sender, EventArgs e)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                btnStopReconcile.Enabled = false;
                lblStatus.Text = "Đang gửi tín hiệu dừng an toàn...";
                _cts.Cancel();
            }
        }

        private void LoadReconciliationHistory()
        {
            try
            {
                dgvHistory.DataSource = _bus.GetReconciliationLogs();
                if (dgvHistory.Columns["LogId"] != null) dgvHistory.Columns["LogId"].HeaderText = "Mã phiên";
                if (dgvHistory.Columns["ReconcileTime"] != null)
                {
                    dgvHistory.Columns["ReconcileTime"].HeaderText = "Thời gian đối soát";
                    dgvHistory.Columns["ReconcileTime"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
                }
                if (dgvHistory.Columns["TotalTransactions"] != null)
                {
                    dgvHistory.Columns["TotalTransactions"].HeaderText = "Tổng giao dịch";
                    dgvHistory.Columns["TotalTransactions"].DefaultCellStyle.Format = "N0";
                }
                if (dgvHistory.Columns["MatchedCount"] != null)
                {
                    dgvHistory.Columns["MatchedCount"].HeaderText = "Số GD khớp";
                    dgvHistory.Columns["MatchedCount"].DefaultCellStyle.Format = "N0";
                }
                if (dgvHistory.Columns["DiscrepancyCount"] != null) dgvHistory.Columns["DiscrepancyCount"].HeaderText = "Số GD lệch";
                if (dgvHistory.Columns["ResultStatus"] != null) dgvHistory.Columns["ResultStatus"].HeaderText = "Kết luận";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch sử đối soát: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}