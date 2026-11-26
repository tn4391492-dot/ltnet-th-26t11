using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BUS;
using DTO;

namespace GUI
{
    public class frmPowerGridMonitor : Form
    {
        private readonly PowerGridBUS _bus = new PowerGridBUS();
        private CancellationTokenSource _scadaCts;
        private int _scanCycles;

        private ComboBox cboSubstation;
        private Button btnStartScada;
        private Button btnDisconnectScada;

        private Label lblVoltage;
        private Label lblLoadPercent;
        private Label lblAlertBanner;

        private DataGridView dgvGridLogs;

        private StatusStrip stsScada;
        private ToolStripStatusLabel lblLastPacket;
        private ToolStripStatusLabel lblCycleCount;

        public frmPowerGridMonitor()
        {
            InitializeLayout();
            ConfigureLogGridColumns();
            LoadSubstations();
            BindEvents();
        }

        private void InitializeLayout()
        {
            this.Text = "TRUNG TÂM ĐIỀU ĐỘ & GIÁM SÁT LƯỚI ĐIỆN THỜI GIAN THỰC (SCADA)";
            this.Size = new Size(920, 620);
            this.StartPosition = FormStartPosition.CenterScreen;

            GroupBox grpControl = new GroupBox
            {
                Text = "Khu vực điều khiển SCADA",
                Location = new Point(16, 12),
                Size = new Size(872, 74)
            };

            Label lblStation = new Label
            {
                Text = "Chọn Trạm biến áp:",
                Location = new Point(16, 32),
                AutoSize = true
            };

            cboSubstation = new ComboBox
            {
                Location = new Point(140, 28),
                Width = 270,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnStartScada = new Button
            {
                Text = "Bắt đầu Giám sát SCADA",
                Location = new Point(430, 24),
                Size = new Size(200, 34),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnDisconnectScada = new Button
            {
                Text = "Ngắt Kết nối",
                Location = new Point(645, 24),
                Size = new Size(160, 34),
                BackColor = Color.Firebrick,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            grpControl.Controls.AddRange(new Control[]
            {
                lblStation,
                cboSubstation,
                btnStartScada,
                btnDisconnectScada
            });

            GroupBox grpDashboard = new GroupBox
            {
                Text = "Bảng thông số vận hành trực quan (Real-time Dashboard)",
                Location = new Point(16, 96),
                Size = new Size(872, 145)
            };

            Label lblVoltageTitle = new Label
            {
                Text = "ĐIỆN ÁP HIỆN TẠI (kV)",
                Location = new Point(40, 28),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.DimGray
            };

            lblVoltage = new Label
            {
                Text = "--- kV",
                Location = new Point(36, 54),
                AutoSize = true,
                Font = new Font("Segoe UI", 24f, FontStyle.Bold),
                ForeColor = Color.MidnightBlue
            };

            Label lblLoadTitle = new Label
            {
                Text = "MỨC MANG TẢI (%)",
                Location = new Point(360, 28),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.DimGray
            };

            lblLoadPercent = new Label
            {
                Text = "--- %",
                Location = new Point(356, 54),
                AutoSize = true,
                Font = new Font("Segoe UI", 24f, FontStyle.Bold),
                ForeColor = Color.ForestGreen
            };

            lblAlertBanner = new Label
            {
                Text = "TRẠNG THÁI: CHỜ KẾT NỐI TÍN HIỆU",
                Location = new Point(40, 108),
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.SlateGray
            };

            grpDashboard.Controls.AddRange(new Control[]
            {
                lblVoltageTitle,
                lblVoltage,
                lblLoadTitle,
                lblLoadPercent,
                lblAlertBanner
            });

            GroupBox grpLogs = new GroupBox
            {
                Text = "Nhật ký sự cố Quá tải lưới điện (> 85%)",
                Location = new Point(16, 250),
                Size = new Size(872, 295)
            };

            dgvGridLogs = new DataGridView
            {
                Location = new Point(14, 24),
                Size = new Size(844, 256),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            grpLogs.Controls.Add(dgvGridLogs);

            stsScada = new StatusStrip();
            lblLastPacket = new ToolStripStatusLabel("Gói tin cuối cùng: --:--:--")
            {
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                Padding = new Padding(4, 0, 16, 0)
            };
            lblCycleCount = new ToolStripStatusLabel("Tổng số chu kỳ đã quét: 0")
            {
                Padding = new Padding(10, 0, 10, 0)
            };
            stsScada.Items.AddRange(new ToolStripItem[] { lblLastPacket, lblCycleCount });

            this.Controls.AddRange(new Control[]
            {
                grpControl,
                grpDashboard,
                grpLogs,
                stsScada
            });
        }

        private void ConfigureLogGridColumns()
        {
            dgvGridLogs.Columns.Clear();

            dgvGridLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTime",
                HeaderText = "Thời gian ghi nhận",
                Width = 160
            });
            dgvGridLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStation",
                HeaderText = "Trạm biến áp",
                Width = 200
            });
            dgvGridLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colVoltage",
                HeaderText = "Điện áp (kV)",
                Width = 120
            });
            dgvGridLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colLoad",
                HeaderText = "Mức tải (%)",
                Width = 120
            });
            dgvGridLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colAlert",
                HeaderText = "Nội dung cảnh báo",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private void LoadSubstations()
        {
            try
            {
                var stations = _bus.GetSubstations();
                cboSubstation.DataSource = stations;
                cboSubstation.DisplayMember = "SubstationName";
                cboSubstation.ValueMember = "SubstationId";
            }
            catch
            {
                cboSubstation.Items.Add("Trạm 110kV Hà Đông");
                cboSubstation.Items.Add("Trạm 220kV Chèm");
                cboSubstation.SelectedIndex = 0;
            }
        }

        private void BindEvents()
        {
            btnStartScada.Click += BtnStartScada_Click;
            btnDisconnectScada.Click += BtnDisconnectScada_Click;
            this.FormClosing += (s, e) => _scadaCts?.Cancel();
        }

        private void BtnStartScada_Click(object sender, EventArgs e)
        {
            btnStartScada.Enabled = false;
            cboSubstation.Enabled = false;
            btnDisconnectScada.Enabled = true;

            _scanCycles = 0;
            _scadaCts = new CancellationTokenSource();
            string selectedStation = cboSubstation.Text;
            CancellationToken token = _scadaCts.Token;

            Task.Run(async () =>
            {
                var random = new Random();

                while (!token.IsCancellationRequested)
                {
                    double voltage = Math.Round(108.0 + random.NextDouble() * 7.0, 2);
                    double loadPercent = Math.Round(50.0 + random.NextDouble() * 48.0, 1);
                    DateTime timestamp = DateTime.Now;

                    UpdateDashboardCrossThread(selectedStation, voltage, loadPercent, timestamp);

                    try
                    {
                        await Task.Delay(2000, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }, token);
        }

        private void BtnDisconnectScada_Click(object sender, EventArgs e)
        {
            _scadaCts?.Cancel();
            _scadaCts?.Dispose();
            _scadaCts = null;

            btnStartScada.Enabled = true;
            cboSubstation.Enabled = true;
            btnDisconnectScada.Enabled = false;

            lblAlertBanner.Text = "TRẠNG THÁI: ĐÃ NGẮT KẾT NỐI SCADA";
            lblAlertBanner.ForeColor = Color.SlateGray;
        }

        private void UpdateDashboardCrossThread(string stationName, double voltage, double loadPercent, DateTime timestamp)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    ApplyTelemetryToUi(stationName, voltage, loadPercent, timestamp);
                }));
            }
            else
            {
                ApplyTelemetryToUi(stationName, voltage, loadPercent, timestamp);
            }
        }

        private void ApplyTelemetryToUi(string stationName, double voltage, double loadPercent, DateTime timestamp)
        {
            _scanCycles++;
            lblVoltage.Text = $"{voltage:F2} kV";
            lblLoadPercent.Text = $"{loadPercent:F1} %";

            lblLastPacket.Text = $"Gói tin cuối cùng: {timestamp:HH:mm:ss}";
            lblCycleCount.Text = $"Tổng số chu kỳ đã quét: {_scanCycles}";

            if (_bus.IsOverloaded(loadPercent))
            {
                string alertText = $"CẢNH BÁO QUÁ TẢI: {stationName} đạt {loadPercent:F1}% (> 85%)";
                lblLoadPercent.ForeColor = Color.Red;
                lblAlertBanner.Text = alertText;
                lblAlertBanner.ForeColor = Color.Red;

                int rowIndex = dgvGridLogs.Rows.Add(
                    timestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                    stationName,
                    $"{voltage:F2}",
                    $"{loadPercent:F1}%",
                    alertText
                );
                dgvGridLogs.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                dgvGridLogs.FirstDisplayedScrollingRowIndex = rowIndex;

                try
                {
                    _bus.RecordOverload(new GridOverloadLogDTO
                    {
                        SubstationName = stationName,
                        RecordedTime = timestamp,
                        VoltageKV = voltage,
                        LoadPercent = loadPercent,
                        AlertMessage = alertText
                    });
                }
                catch
                {
                }
            }
            else
            {
                lblLoadPercent.ForeColor = Color.ForestGreen;
                lblAlertBanner.Text = $"TRẠNG THÁI ỔN ĐỊNH: {stationName} vận hành bình thường";
                lblAlertBanner.ForeColor = Color.ForestGreen;
            }
        }
    }
}