using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Timer = System.Timers.Timer;

namespace PhotoBooth
{
    public partial class Form1 : Form
    {

        private PictureBox pictureBox1;

        private Panel staticPicturePanel;

        private Bitmap activeFrame;

        private Label countDownLabel;

        private int countDown;
        public Form1()
        {
            InitializeComponent();
            this.ResizeEnd += (sender, args) => Form1_ResizeEnd(sender, args);
            startVideoCapture();
            addItemsToPictureBoxControl();
        }

        private void addItemsToPictureBoxControl()
        {
            this.Controls.Clear();
            pictureBox1 = new PictureBox();
            pictureBox1.Size = new Size(this.Size.Width - Int32.Parse(Math.Round(this.Size.Width * 0.15).ToString()), this.Size.Height - 80);
            pictureBox1.Location = new Point(Int32.Parse(Math.Round(this.Size.Width * 0.125).ToString()), 20);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Name = "pictureBox1";
            this.Controls.Add(pictureBox1);
            
            pictureBox1.Controls.Clear();
            comboBox = new ComboBox();
            comboBox.Items.Add("3 seconds");
            comboBox.Items.Add("5 seconds");
            comboBox.Items.Add("10 seconds");
            comboBox.SelectedIndex = 0;
            comboBox.BackColor = Color.FromArgb(68,68,68);
            comboBox.Location = new Point(pictureBox1.Width / 2, pictureBox1.Height - 50);
            comboBox.AutoSize = true;
            pictureBox1.Controls.Add(comboBox);

            Button openPicturesButton = new Button();
            openPicturesButton.Text = "Show Images!";
            openPicturesButton.Size = new Size(100, 40);
            openPicturesButton.Click += (sender, args) => Process.Start("explorer.exe", Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../images")));
            openPicturesButton.Location = new Point(20, 50);
            this.Controls.Add(openPicturesButton);

            staticPicturePanel = new Panel();
            staticPicturePanel.Name = "staticPictureBox";
            staticPicturePanel.Size = new Size(50, 50);
            staticPicturePanel.Location = new Point(pictureBox1.Width / 2 - 75, pictureBox1.Height - 65);
            staticPicturePanel.Click += (sender, args) => startCountDown();
            
            countDownLabel = new Label();
            countDownLabel.Text = "\0";
            countDownLabel.Location = new Point(pictureBox1.Width - 70, 10);
            countDownLabel.Size = new Size(70, 70);
            countDownLabel.Font = new Font("Times New Roman", 40);
            countDownLabel.ForeColor = Color.Red;
            countDownLabel.BackColor = Color.Transparent;
            pictureBox1.Controls.Add(countDownLabel);
            
            PictureBox box = new PictureBox();
            box.Image = Image.FromFile("../../workspace/camera-icon3.png");
            box.Click += (sender, args) => startCountDown();
            staticPicturePanel.Controls.Add(box);
            pictureBox1.Controls.Add(staticPicturePanel);
        }

        private void startVideoCapture()
        {
            VideoCaptureDevice captureDevice =
                new VideoCaptureDevice(new FilterInfoCollection(FilterCategory.VideoInputDevice)[0].MonikerString);
            captureDevice.NewFrame += (sender, args) => newFrame(sender, args);
            captureDevice.Start();
        }

        private void newFrame(object sender, NewFrameEventArgs e)
        {
            Bitmap source = (Bitmap)e.Frame.Clone();
            source.RotateFlip(RotateFlipType.RotateNoneFlipX);
            activeFrame = source;
            pictureBox1.Image = source;

            Bitmap target = new Bitmap(50, 50);
            target.RotateFlip(RotateFlipType.RotateNoneFlipX);
            using (Graphics graphics = Graphics.FromImage(target))
            {
                graphics.DrawImage(source, new Rectangle(0,0, target.Width, target.Height), new Rectangle(pictureBox1.Width / 2 - 75, pictureBox1.Height - 65, 50, 50), GraphicsUnit.Pixel);
            }

            staticPicturePanel.Controls[0].BackgroundImage = target;
            staticPicturePanel.Controls[0].BackgroundImageLayout = ImageLayout.Stretch;
            staticPicturePanel.BackgroundImage = target;
            staticPicturePanel.BackgroundImageLayout = ImageLayout.Stretch;
            staticPicturePanel.BackColor = Color.Transparent;
            staticPicturePanel.Controls[0].BackColor = Color.Transparent;
        }

        private void takePicture(Timer timer)
        {
            if (countDown > 0)
            {
                countDownLabel.Text = $"{countDown}";
                Console.WriteLine(countDown);
                countDown--;
            }
            else
            {
                timer.Stop();
                timer.Dispose();
                countDownLabel.Text = "\0";
                Console.WriteLine("Entered");
                try
                {
                    string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../images/"));
                    path = Path.Combine(path, "Photobooth-" + dateTime + ".png");
                    Console.WriteLine(path);
                    Bitmap saveImage = new Bitmap(activeFrame);
                    saveImage.Save(path, ImageFormat.Png);
                }
                catch (ExternalException e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void startCountDown()
        {
            string countDownString = comboBox.SelectedItem.ToString();
            countDown = int.Parse(countDownString.Split(' ')[0]);
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += (sender, args) => takePicture(timer);
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            addItemsToPictureBoxControl();
        }
        
    }
}