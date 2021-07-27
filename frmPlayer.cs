using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Remote_Cockpit
{
    public partial class frmPlayer : Form
    {
        public frmPlayer()
        {
            InitializeComponent();
        }
        tstRtmp rtmp = new tstRtmp();
        Thread thPlayer;
        private void pic_Click(object sender, EventArgs e)
        {
            if (thPlayer != null)
            {
                rtmp.Stop();
                thPlayer = null;
                this.Text = "停止";
            }
            else
            {
                thPlayer = new Thread(DeCoding);
                thPlayer.IsBackground = true;
                this.Text = "运行";
                thPlayer.Start();
            }
        }
        /// <summary>
        /// 播放线程执行方法
        /// </summary>
        private unsafe void DeCoding()
        {
            try
            {
                Console.WriteLine("DeCoding run...");
                Bitmap oldBmp = null;


                // 更新图片显示
                tstRtmp.ShowBitmap show = (bmp) =>
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        this.pic.Image = bmp;
                        if (oldBmp != null)
                        {
                            oldBmp.Dispose();
                        }
                        oldBmp = bmp;
                    }));
                };
                //rtmp.Start(show, "http://192.168.87.129:8080/?action=stream");
                rtmp.Start(show, "rtmp://47.106.198.78:1935/live/1440163018362/2");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
