using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Renci.SshNet;
using System.Threading;

namespace Phone_Farm_SSH
{
    public partial class Frm_PhoneFarmSSHClient : Form
    {
        Bitmap logoGoogle, numPage;
        public Frm_PhoneFarmSSHClient()
        {
            InitializeComponent();
            LoadData();
        }

        void LoadData()
        {
            logoGoogle = (Bitmap)Bitmap.FromFile("image//LogoGoogle.png");
            numPage = (Bitmap)Bitmap.FromFile("image//NumPage.png");
        }

        DataTable table = new DataTable();
        String[] filePaths;
        private SshClient _client;
        string Host, Username, Password;
        bool isStop = false;
        int index;
        int numAds, notAds;

        void Connect_SSH(int index)
        {
            Host = dgv.Rows[index].Cells["Host"].Value.ToString();
            Username = dgv.Rows[index].Cells["Username"].Value.ToString();
            Password = dgv.Rows[index].Cells["Password"].Value.ToString();
            try
            {
                if (_client != null)
                {
                    _client.Disconnect();
                    label2.ForeColor = Color.Black;
                }
                _client = new SshClient(Host, Username, Password);
                _client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
                _client.Connect();
                ForwardedPortDynamic port = new ForwardedPortDynamic("127.0.0.1", Convert.ToUInt16(txtPort.Text));
                _client.AddForwardedPort(port);
                if (_client.IsConnected)
                {
                    port.Start();
                    //label2.Visible = true;
                    //label2.ForeColor = Color.Green;
                    //label2.Text = "Connected Host: " + Host;
                    dgv.Rows[index].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#abfeac");
                }
            }
            catch (Renci.SshNet.Common.SshOperationTimeoutException)
            {
                _client.Disconnect();
                dgv.Rows[index].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#fffcab");
                NextRow();

            }
            catch (Renci.SshNet.Common.SshAuthenticationException)
            {
                _client.Disconnect();
                dgv.Rows[index].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#ffabab");
                NextRow();
            }
            catch (Renci.SshNet.Common.SshConnectionException)
            {
                _client.Disconnect();
                dgv.Rows[index].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#ffabab");
                NextRow();
            }
            catch (System.Net.Sockets.SocketException)
            {
                //_client.Disconnect();
                //dgv.Rows[index].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#ffabab");
                //NextRow();
                MessageBox.Show("Port đã được sử dụng");
            }
            catch (Exception ex)
            {
                _client.Disconnect();
                MessageBox.Show("Lỗi: "+ex.ToString(), "Lỗi phát sinh");
            }
        }

        void Auto()
        {
            List<string> devices = new List<string>();
            var listDevice = KAutoHelper.ADBHelper.GetDevices();
            if (listDevice != null && listDevice.Count > 0)
            {
                devices = listDevice;
            }

            foreach (var deviceID in devices)
            {
                Task t = new Task(() =>
                {
                    numAds = 0; notAds = 0;
                    while (true)
                    {
                        // Click vào trình duyệt
                        if (isStop)
                            return;
                        KAutoHelper.ADBHelper.Tap(deviceID, 110, 625);
                        Delay(5);

                        // Mở menu
                        if (isStop)
                            return;
                        KAutoHelper.ADBHelper.Tap(deviceID, 765, 120);
                        Delay(1);

                        // Chọn tab ẩn danh
                        if (isStop)
                            return;
                        KAutoHelper.ADBHelper.Tap(deviceID, 591, 176);
                        Delay(1);

                        // Click vào ô địa chỉ
                        if (isStop)
                            return;
                        //KAutoHelper.ADBHelper.Tap(deviceID, 369, 124);
                        KAutoHelper.ADBHelper.Tap(deviceID, 455, 120);
                        Delay(1);

                        // Nhập địa chỉ
                        if (isStop)
                            return;
                        KAutoHelper.ADBHelper.InputText(deviceID, "http://thongdang.herokuapp.com");
                        Delay(1);

                        // Nhấn Enter
                        if (isStop)
                            return;
                        KAutoHelper.ADBHelper.Key(deviceID, KAutoHelper.ADBKeyEvent.KEYCODE_ENTER);
                        Delay(25);

                        // Kéo xuống
                        Random rnd = new Random();
                        int timesDown = rnd.Next(8, 12);
                        for (int i = 1; i < timesDown; i++)
                        {
                            // Kéo 50% trang
                            if (isStop)
                                return;
                            KAutoHelper.ADBHelper.Swipe(deviceID, 620, 1133, 748, 811);
                            Delay(2);
                        }

                        // Kéo lên
                        for (int j = 1; j < timesDown; j++)
                        {
                            // Kéo 100% trang
                            if (isStop)
                                return;
                            KAutoHelper.ADBHelper.Swipe(deviceID, 748, 811, 620, 1133);
                            Delay(2);
                        }

                        //if (isStop)
                        //    return;
                        //KAutoHelper.ADBHelper.Tap(deviceID, 381, 680);
                        //Delay(2);

                        //if (isStop)
                        //    return;
                        //KAutoHelper.ADBHelper.Tap(deviceID, 536, 1151);
                        //Delay(10);

                        // Thoát ứng dụng
                        if (isStop)
                            return;
                        KAutoHelper.ADBHelper.Key(deviceID, KAutoHelper.ADBKeyEvent.KEYCODE_APP_SWITCH);
                        Delay(1);

                        // Kéo ngang để đóng hẳn ứng dụng
                        if (isStop)
                            return;
                        KAutoHelper.ADBHelper.Swipe(deviceID, 144, 517, 672, 524);
                        Delay(2);
                    }
                });
                t.Start();
            }
        }

        bool CheckImg(string deviceID, Bitmap image)
        {
            try
            {
                var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
                var point = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, image);
                if (point != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        void Delay(int delay)
        {
            while (delay > 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                delay--;
                if (isStop)
                    break;
            }
        }

        void NextRow()
        {
            if (index < dgv.RowCount - 1)
            {
                dgv.ClearSelection();
                //dgv.Rows[++index].Selected = true;
                index = ++index;
                Connect_SSH(index);
            }
        }

        private void Frm_PhoneFarmSSHClient_Load(object sender, EventArgs e)
        {
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            btnDown.Visible = false;
            btnCon.Enabled = false;
            groupBox2.Enabled = false;
            table.Columns.Add("Host", typeof(string));
            table.Columns.Add("Username", typeof(string));
            table.Columns.Add("Password", typeof(string));
            table.Columns.Add("Country", typeof(string));
            table.Columns.Add("Region", typeof(string));
            table.Columns.Add("City", typeof(string));
            table.Columns.Add("Zip code", typeof(string));
            dgv.DataSource = table;
            index = dgv.CurrentCell.RowIndex;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                btnCon.Enabled = true;
                filePaths = openFileDialog.FileNames;
                foreach (string fileUrl in filePaths)
                {
                    string[] lines = File.ReadAllLines(fileUrl);
                    string[] values;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        values = lines[i].ToString().Split('|');
                        string[] row = new string[values.Length];
                        for (int j = 0; j < values.Length; j++)
                        {
                            row[j] = values[j].Trim();
                        }
                        table.Rows.Add(row);
                    }
                }
            }
        }

        private void txtPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            Task t = new Task(() =>
            {
                isStop = false;
                Auto();
            });
            t.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            isStop = true;
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            NextRow();
        }

        private void dgv_Click(object sender, EventArgs e)
        {
            if (txtPort.Text == "")
            {
                MessageBox.Show("Port không hợp lệ, vui lòng kiểm tra lại.");
            }
            else
            {
                index = dgv.CurrentCell.RowIndex;
                Connect_SSH(index);
            }
        }

        private void btnCon_Click(object sender, EventArgs e)
        {
            if (txtPort.Text == "")
            {
                MessageBox.Show("Port không hợp lệ, vui lòng kiểm tra lại.");
            }
            else
            {
                Connect_SSH(0);
                groupBox2.Enabled = true;
            }
        }
    }
}
