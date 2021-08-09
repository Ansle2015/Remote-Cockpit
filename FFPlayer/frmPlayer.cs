using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;

namespace Remote_Cockpit
{

    public partial class frmPlayer : Form
    {
        public frmPlayer()
        {
            InitializeComponent();
        }
        tstRtmp rtmp = new tstRtmp();
        private System.Timers.Timer frmPlayerTimer;
        Thread thPlayer;      

        private void pic_Click(object sender, EventArgs e)
        {
            if (thPlayer != null)
            {
                rtmp.Stop();
                thPlayer = null;
                this.Text = "停止";
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
            else
            {              
                thPlayer = new Thread(DeCoding);
                thPlayer.IsBackground = true;
                this.Text = "运行";
                thPlayer.Start();
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
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
                rtmp.Start(show, "http://192.168.87.129:8080/?action=stream");
                //rtmp.Start(show, "rtmp://47.106.198.78:1935/live/1440163018362/2");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码
        private void InitializeComponent()
        {
            this.pic = new System.Windows.Forms.PictureBox();
            this.frmPlayerTimer = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.pic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.frmPlayerTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // pic
            // 
            this.pic.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pic.Location = new System.Drawing.Point(0, 0);
            this.pic.Name = "pic";
            this.pic.Size = new System.Drawing.Size(1017, 464);
            this.pic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic.TabIndex = 2;
            this.pic.TabStop = false;
            this.pic.Click += new System.EventHandler(this.pic_Click);
            // 
            // frmPlayerTimer
            // 
            this.frmPlayerTimer.Enabled = true;
            this.frmPlayerTimer.Interval = 200;
            this.frmPlayerTimer.SynchronizingObject = this;
            this.frmPlayerTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            // 
            // frmPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1017, 464);
            this.Controls.Add(this.pic);
            this.Name = "frmPlayer";
            this.ShowIcon = false;
            this.Text = "请启动";
            ((System.ComponentModel.ISupportInitialize)(this.pic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.frmPlayerTimer)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr GetActiveWindow();//获取当前窗体的活动状态

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (GetActiveWindow() != this.Handle)
            {
                this.Activate();
            }
        }

        private System.Windows.Forms.PictureBox pic;
    }
}
