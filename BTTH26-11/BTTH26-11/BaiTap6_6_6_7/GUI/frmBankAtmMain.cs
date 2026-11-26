
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GUI
{
    public class frmBankAtmMain : Form
    {
        private MenuStrip mnuMain;
        private ToolStripMenuItem mnuSystem;
        private ToolStripMenuItem mnuLogout;
        private ToolStripMenuItem mnuLockScreen;
        private ToolStripMenuItem mnuExit;

        private ToolStripMenuItem mnuAtmManagement;
        private ToolStripMenuItem mnuAtmStations;
        private ToolStripMenuItem mnuAtmReconciliation;

        private ToolStripMenuItem mnuWindow;
        private ToolStripMenuItem mnuCascade;
        private ToolStripMenuItem mnuTileHorizontal;
        private ToolStripMenuItem mnuTileVertical;
        private ToolStripMenuItem mnuCloseAll;

        private StatusStrip stsMain;
        private ToolStripStatusLabel lblTeller;
        private ToolStripStatusLabel lblCoreConnection;
        private ToolStripStatusLabel lblProgressText;
        private ToolStripProgressBar prgMain;

        private bool _isLocked;

        public frmBankAtmMain()
        {
            InitializeLayout();
            BindEvents();
        }

        private void InitializeLayout()
        {
            this.Text = "HỆ THỐNG MDI ĐIỀU HÀNH MẠNG LƯỚI CÂY ATM NGÂN HÀNG";
            this.Size = new Size(1220, 740);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.IsMdiContainer = true;

            mnuMain = new MenuStrip();

            mnuSystem = new ToolStripMenuItem("Hệ thống");
            mnuLogout = new ToolStripMenuItem("Đăng xuất");
            mnuLockScreen = new ToolStripMenuItem("Khóa màn hình");
            mnuExit = new ToolStripMenuItem("Thoát");
            mnuSystem.DropDownItems.AddRange(new ToolStripItem[]
            {
                mnuLogout,
                mnuLockScreen,
                new ToolStripSeparator(),
                mnuExit
            });

            mnuAtmManagement = new ToolStripMenuItem("Quản lý ATM");
            mnuAtmStations = new ToolStripMenuItem("Danh sách Trạm ATM & Tồn quỹ");
            mnuAtmReconciliation = new ToolStripMenuItem("Đối soát Giao dịch Cuối ngày");
            mnuAtmManagement.DropDownItems.AddRange(new ToolStripItem[]
            {
                mnuAtmStations,
                mnuAtmReconciliation
            });

            mnuWindow = new ToolStripMenuItem("Cửa sổ");
            mnuCascade = new ToolStripMenuItem("Xếp tầng (Cascade)");
            mnuTileHorizontal = new ToolStripMenuItem("Xếp ngang (Tile Horizontal)");
            mnuTileVertical = new ToolStripMenuItem("Xếp dọc (Tile Vertical)");
            mnuCloseAll = new ToolStripMenuItem("Đóng tất cả");
            mnuWindow.DropDownItems.AddRange(new ToolStripItem[]
            {
                mnuCascade,
                mnuTileHorizontal,
                mnuTileVertical,
                new ToolStripSeparator(),
                mnuCloseAll
            });

            mnuMain.Items.AddRange(new ToolStripItem[]
            {
                mnuSystem,
                mnuAtmManagement,
                mnuWindow
            });
            mnuMain.MdiWindowListItem = mnuWindow;

            stsMain = new StatusStrip();
            lblTeller = new ToolStripStatusLabel("GDV trực: Nguyễn Trường Giang (ID: 24810310200)")
            {
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                Padding = new Padding(4, 0, 12, 0)
            };
            lblCoreConnection = new ToolStripStatusLabel("Core Banking: Đã kết nối (Online)")
            {
                ForeColor = Color.DarkGreen,
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                Padding = new Padding(8, 0, 12, 0)
            };
            lblProgressText = new ToolStripStatusLabel("Sẵn sàng")
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 8, 0)
            };
            prgMain = new ToolStripProgressBar
            {
                Size = new Size(180, 18),
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            stsMain.Items.AddRange(new ToolStripItem[]
            {
                lblTeller,
                lblCoreConnection,
                lblProgressText,
                prgMain
            });

            this.MainMenuStrip = mnuMain;
            this.Controls.Add(mnuMain);
            this.Controls.Add(stsMain);
        }

        private void BindEvents()
        {
            mnuAtmStations.Click += (s, e) => OpenOrActivateChildForm(() => new frmAtmStations());
            mnuAtmReconciliation.Click += (s, e) => OpenOrActivateChildForm(() => new frmAtmReconciliation());

            mnuCascade.Click += (s, e) => this.LayoutMdi(MdiLayout.Cascade);
            mnuTileHorizontal.Click += (s, e) => this.LayoutMdi(MdiLayout.TileHorizontal);
            mnuTileVertical.Click += (s, e) => this.LayoutMdi(MdiLayout.TileVertical);
            mnuCloseAll.Click += (s, e) => CloseAllMdiChildren();

            mnuLockScreen.Click += MnuLockScreen_Click;
            mnuLogout.Click += MnuLogout_Click;
            mnuExit.Click += (s, e) => Application.Exit();
        }

        private void OpenOrActivateChildForm<T>(Func<T> factory) where T : Form
        {
            T existingForm = this.MdiChildren.OfType<T>().FirstOrDefault();
            if (existingForm != null)
            {
                if (existingForm.WindowState == FormWindowState.Minimized)
                {
                    existingForm.WindowState = FormWindowState.Normal;
                }
                existingForm.Activate();
                return;
            }

            T childForm = factory();
            childForm.MdiParent = this;
            childForm.Show();
        }

        private void CloseAllMdiChildren()
        {
            foreach (Form child in this.MdiChildren)
            {
                child.Close();
            }
        }

        private void MnuLockScreen_Click(object sender, EventArgs e)
        {
            _isLocked = !_isLocked;
            mnuAtmManagement.Enabled = !_isLocked;
            mnuWindow.Enabled = !_isLocked;
            mnuLockScreen.Text = _isLocked ? "Mở khóa màn hình" : "Khóa màn hình";
            lblCoreConnection.Text = _isLocked ? "Trạng thái: Đã khóa phiên làm việc" : "Core Banking: Đã kết nối (Online)";
            lblCoreConnection.ForeColor = _isLocked ? Color.DarkOrange : Color.DarkGreen;

            foreach (Form child in this.MdiChildren)
            {
                child.Enabled = !_isLocked;
            }
        }

        private void MnuLogout_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                "Xác nhận đăng xuất phiên làm việc hiện tại?",
                "Đăng xuất hệ thống",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                CloseAllMdiChildren();
                lblTeller.Text = "GDV trực: Chưa đăng nhập";
                lblCoreConnection.Text = "Core Banking: Ngắt kết nối";
                lblCoreConnection.ForeColor = Color.Red;
                mnuAtmManagement.Enabled = false;
                mnuWindow.Enabled = false;
            }
        }

        public void UpdateReconciliationProgress(int percent, string message)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateReconciliationProgress(percent, message)));
                return;
            }

            prgMain.Value = Math.Max(prgMain.Minimum, Math.Min(prgMain.Maximum, percent));
            lblProgressText.Text = message;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // frmBankAtmMain
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "frmBankAtmMain";
            this.Load += new System.EventHandler(this.frmBankAtmMain_Load);
            this.ResumeLayout(false);

        }

        private void frmBankAtmMain_Load(object sender, EventArgs e)
        {

        }
    }
}