using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;  //调用WINDOWS API函数时要用到
using Microsoft.Win32;  //写入注册表时要用到
using System.Drawing.Imaging;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace OneDayOneBing
{
    public partial class Form1 : Form
    {
        Timer CloseTimer = new Timer();
        public Form1()
        {
            InitializeComponent();
        }

        private void Logging(string s)
        {
            textBox1.Text += s + "\n";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                this.Hide();
                Uri url = new Uri("http://cn.bing.com/");
                var html = GetHtml(url.AbsoluteUri);
                //Logging(html);

                var re = new Regex(@"g_img=\{url: \""(.+?)\""");
                var image = re.Match(html).Groups[1].Value;

                Uri imageUrl = new Uri(url, image);
                Logging("image url:" + imageUrl);

                string savePath = Path.Combine(Application.StartupPath, "images");
                if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
                string filename = Path.Combine(savePath, Path.GetFileName(image));
                DownloadRemoteImageFile(imageUrl.AbsoluteUri, filename);

                pictureBox1.Image = new Bitmap(filename);

                SetWallpaper(filename);
                //Logging("Happy for use.");
                //Logging("Closing in 5s...");
                //CloseTimer.Interval = 5000;
                //CloseTimer.Start();
                //CloseTimer.Tick += CloseTimer_Tick;
                Close();
            }
            catch(Exception ex)
            {
                Logging(ex.ToString());
                Logging(ex.StackTrace);
                this.Show();
            }
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            Close();
        }

        private static void DownloadRemoteImageFile(string uri, string fileName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {

                // if the remote file was found, download oit
                using (Stream inputStream = response.GetResponseStream())
                using (Stream outputStream = File.OpenWrite(fileName))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
            }
        }

        private string GetHtml(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.Headers["Accept-Language"] = "zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4,zh-TW;q=0.2";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.96 Safari/537.36";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            using (Stream receiveStream = response.GetResponseStream())
            {

                if (response.CharacterSet == null)
                {
                    using (var readStream = new StreamReader(receiveStream))
                        return readStream.ReadToEnd();
                }
                else
                {
                    using (var readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet)))
                        return readStream.ReadToEnd();
                }
            }
        }

        /// <summary>
        ///          TileWallpaper    WallpaperStyle
        ///  居中：         0                0
        ///  平铺：         1                0
        ///  拉伸：         0                2  
        /// </summary>
        /// <param name="TileWallpaper"></param>
        /// <param name="WallpaperStyle"></param>
        private void SetWallpaperMode(string TileWallpaper, string WallpaperStyle)
        {
            //设置墙纸显示方式
            RegistryKey myRegKey = Registry.CurrentUser.OpenSubKey("Control Panel/desktop", true);
            myRegKey.SetValue("TileWallpaper", TileWallpaper);
            myRegKey.SetValue("WallpaperStyle", WallpaperStyle);

            //关闭该项,并将改动保存到磁盘
            myRegKey.Close();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, String pvParam, UInt32 fWinIni);
        private static UInt32 SPI_SETDESKWALLPAPER = 20;
        private static UInt32 SPIF_UPDATEINIFILE = 0x1;

        /// <summary>
        /// 设置壁纸
        /// </summary>
        /// <param name="filename">壁纸的路径，值为null时使用pictureBox1中的值</param>
        private void SetWallpaper(string filename = null)
        {

            if (filename == null)
            {
                Bitmap bmpWallpaper = (Bitmap)pictureBox1.Image;
                bmpWallpaper.Save("resource.bmp", ImageFormat.Bmp); //图片保存路径为相对路径，保存在程序的目录下
                filename = Application.StartupPath + "/resource.bmp";
            }
            SystemParametersInfo(SPI_SETDESKWALLPAPER, SPIF_UPDATEINIFILE, filename, 1);

        }
    }
}
