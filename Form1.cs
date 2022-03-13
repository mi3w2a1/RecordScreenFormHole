using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RecordScreenFormHole
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // 穴にフォームのクライアントサイズを合わせる
            this.ClientSize = new Size(HoleRectangle.Width + 20, HoleRectangle.Height + 70);
            this.TransparencyKey = Color.Red;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        Point MouseDownPoint = Point.Empty;
        Point MouseDownLocation = Point.Empty;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Form.Locationをフィールド変数に保存
            MouseDownLocation = this.Location;

            // マウスボタンが押された座標をスクリーン座標として保存
            MouseDownPoint = this.PointToScreen(new Point(e.X, e.Y));
            this.Capture = true;

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.Capture)
            {
                // マウスカーソルがある位置をスクリーン座標として取得する
                Point mousePoint = this.PointToScreen(new Point(e.X, e.Y));

                // マウスボタンが押された位置からどれだけ移動させるべきかを計算する
                int dx = mousePoint.X - MouseDownPoint.X;
                int dy = mousePoint.Y - MouseDownPoint.Y;

                // 計算して得られた量だけ移動させる
                this.Location = new Point(MouseDownLocation.X + dx, MouseDownLocation.Y + dy);
            }

            base.OnMouseMove(e);
        }

        protected override void WndProc(ref Message m)
        {
            int WM_NCLBUTTONDOWN = 0x00A1;
            int HTCAPTION = 2;

            if (m.Msg == WM_NCLBUTTONDOWN && m.WParam.ToInt32() == HTCAPTION)
            {
                MouseDownPoint = Control.MousePosition;
                MouseDownLocation = this.Location;
                this.Capture = true;

                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);
        }

        Rectangle HoleRectangle = new Rectangle(10, 50, 480, 360);

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Red, HoleRectangle);

            base.OnPaint(e);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            AForge.Video.VFW.AVIWriter aviWriter = new AForge.Video.VFW.AVIWriter();

            int width = HoleRectangle.Width;
            int height = HoleRectangle.Height;

            Bitmap bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);

            // aviWriter.Openに渡す幅と高さは2の倍数にすること。さもないと例外が発生する
            aviWriter.FrameRate = 10;
            aviWriter.Open("test.avi", width, height);

            // 0.1秒おき300回（30秒間）録画する
            for (int i = 0; i < 30 * 10; i++)
            {
                Point pt = this.PointToScreen(new Point(HoleRectangle.X, HoleRectangle.Y));

                g.CopyFromScreen(pt, new Point(0, 0), bitmap.Size);
                aviWriter.AddFrame(bitmap);
                await Task.Delay(100);
            }

            bitmap.Dispose();
            g.Dispose();

            aviWriter.Close();

            MessageBox.Show("録画が完了しました。");
        }
    }
}
