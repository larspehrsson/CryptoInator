using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;

// icon from http://www.iconfinder.com/icondetails/8794/128/cryptography_key_lock_log_in_login_password_security_icon

namespace CryptoInator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _currentexe;
        private byte[] _imgbytes;
        private Point _origin;
        private int _sleep = 1000;
        private Point _start;

        public MainWindow()
        {
            InitializeComponent();

            _currentexe = Assembly.GetExecutingAssembly().Location;
            // Set _currentexe to an existing encryptet file to debug decrypt part
            //_currentexe = @"\\virfil15\brugere$\LPE\OfficeXP\Test.exe";

            WPFWindow.MouseWheel += MainWindowMouseWheel;

            image.MouseLeftButtonDown += ImageMouseLeftButtonDown;
            image.MouseLeftButtonUp += ImageMouseLeftButtonUp;
            image.MouseMove += ImageMouseMove;

            if (CheckForImage())
            {
                GridLayoutRoot.RowDefinitions[0].MaxHeight = 0;
                OpenBTN.IsDefault = true;
                OpenPassWord.Focus();
                OpenBTN.Height = 0;
                OpenBTN.Width = 0;
            }
            else
            {
                GridLayoutRoot.RowDefinitions[1].MaxHeight = 0;
                GenerateBTN.IsDefault = true;
                FilenameTB.Focus();
            }
        }

        private void ImageMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            image.ReleaseMouseCapture();
        }

        /// <summary>
        /// Moves the image the way the mouse is moved. Moves relative to the saved position.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageMouseMove(object sender, MouseEventArgs e)
        {
            if (!image.IsMouseCaptured) return;
            Point p = e.MouseDevice.GetPosition(border);

            Matrix m = image.RenderTransform.Value;
            m.OffsetX = _origin.X + (p.X - _start.X);
            m.OffsetY = _origin.Y + (p.Y - _start.Y);

            image.RenderTransform = new MatrixTransform(m);

        }

        /// <summary>
        /// When mouse is first pressed, save the position of the mouse (_start) and the offset of the image (_origin)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (image.IsMouseCaptured) return;
            image.CaptureMouse();

            _start = e.GetPosition(border);
            _origin.X = image.RenderTransform.Value.OffsetX;
            _origin.Y = image.RenderTransform.Value.OffsetY;
        }


        /// <summary>
        /// Zoomes image on mousewheel. ScaleAtPrepend keep the focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowMouseWheel(object sender, MouseWheelEventArgs e)
        {

            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                double y = 0;
                if (e.Delta > 0)
                    y = GridLayoutRoot.RowDefinitions[2].ActualHeight / 15;
                else
                    y = -(GridLayoutRoot.RowDefinitions[2].ActualHeight / 15);

                Matrix m = image.RenderTransform.Value;
                m.OffsetY += y;
                image.RenderTransform = new MatrixTransform(m);
            }
            else
            {
                Point p = e.MouseDevice.GetPosition(image);

                Matrix m = image.RenderTransform.Value;
                if (e.Delta > 0)
                    m.ScaleAtPrepend(1.1, 1.1, p.X, p.Y);
                else
                    m.ScaleAtPrepend(1 / 1.1, 1 / 1.1, p.X, p.Y);

                image.RenderTransform = new MatrixTransform(m);
            }
        }

        /// <summary>
        /// Scan button is clicked. Aquire image from WIA compatible scanner
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScanBtnClick(object sender, RoutedEventArgs e)
        {
            using (var adapter = new WiaScannerAdapter())
            {
                try
                {
                    _imgbytes = adapter.ScanImage(System.Drawing.Imaging.ImageFormat.Tiff);

                    // Update image from decryptet byte array
                    GenerateImageFromByteArray();
                }
                catch (WiaOperationException ex)
                {
                    MessageBox.Show(ex.Message + " Error Code: " + ex.ErrorCode);
                }
            }
        }

        /// <summary>
        /// Convert BYTE array to an Image
        /// </summary>
        private void GenerateImageFromByteArray()
        {
            var memoryStream = new MemoryStream(_imgbytes);
            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = memoryStream;
            imageSource.EndInit();

            image.Source = imageSource;
            image.Stretch = Stretch.Uniform;

            _imagesize = _imgbytes.Length;
        }

        /// <summary>
        /// OpenBTn clicked. Tries to decrypt image with specified password.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenBtnClick(object sender, RoutedEventArgs e)
        {
            if (OpenPassWord.Password.Length == 0)
            {
                MessageBox.Show("Password can not be empty");
                return;
            }

            WPFWindow.Cursor = Cursors.Wait;

            GridLayoutRoot.RowDefinitions[1].MaxHeight = 0;

            OpenBTN.IsEnabled = false;
            OpenPassWord.IsEnabled = false;

            // Image is decryptet in the background. The only reason for this is that I wanted to hide
            // the password grid, but that would only work if I decrypt in a background thread
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += BackgroundGetImage;
            worker.RunWorkerCompleted += CheckForImageDecryption;
            worker.RunWorkerAsync(OpenPassWord.Password);
        }


        /// <summary>
        /// Called after image decryption has been called. Check e to check for successfull decryption
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckForImageDecryption(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                Thread.Sleep(_sleep);
                _sleep += 1000;
                OpenBTN.IsEnabled = true;
                OpenPassWord.IsEnabled = true;
                OpenPassWord.Password = "";
                OpenPassWord.Focus();
                GridLayoutRoot.RowDefinitions[1].MaxHeight = 100;
                WPFWindow.GridLayoutRoot.UpdateLayout();
            }
            else
            {
                // Update image from decryptet byte array
                GenerateImageFromByteArray();

                // Hide password prompt
                GridLayoutRoot.RowDefinitions[1].MaxHeight = 0;

                // Enable keyboard navigation
                WPFWindow.PreviewKeyDown += WpfWindowPreviewKeyDown;

                // Change programname
                this.Title = this.Title + " - " + System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            }
            WPFWindow.Cursor = null;
        }

        /// <summary>
        /// File button clicked. Get imagename from user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileBtnClick(object sender, RoutedEventArgs e)
        {
            FilenameTB.Text = GetFileName();
            if (_imagesize == 0 & FilenameTB.Text != "")
            {
                image.Source = new BitmapImage(new Uri(FilenameTB.Text));

                FileStream fileStream = File.OpenRead(FilenameTB.Text);

                _imgbytes = new byte[fileStream.Length];
                fileStream.Read(_imgbytes, 0, _imgbytes.Length);
                fileStream.Close();

                Passwd1TB.Focus();
            }
        }

        /// <summary>
        /// Generates the destination file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateBtnClick(object sender, RoutedEventArgs e)
        {
            if (Passwd2TB.Password != Passwd1TB.Password)
            {
                MessageBox.Show("Passwords does not match");
                return;
            }

            if (FilenameTB.Text == "")
            {
                MessageBox.Show("Filename cat not be empty");
                return;
            }

            //Cursor cur = WPFWindow.Cursor;

            WPFWindow.Cursor = Cursors.Wait;
            GridLayoutRoot.RowDefinitions[0].MaxHeight = 0;
            GridLayoutRoot.RowDefinitions[1].MaxHeight = 0;

            GenerateBTN.IsEnabled = false;
            var exefile = SaveImage(FilenameTB.Text, Passwd2TB.Password);
            GenerateBTN.IsEnabled = true;

            System.Diagnostics.Process.Start("explorer.exe", @"/select, " + exefile);
            WPFWindow.Cursor = null;
        }

        /// <summary>
        ///  Updates password quality meter when data is being intered in the password field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Passwd1TbPasswordChanged(object sender, RoutedEventArgs e)
        {
            int passwordscore = PasswordQuality.GetPasswordScore(Passwd1TB.Password);
            PasswordBar.Value = passwordscore;
        }

        /// <summary>
        /// Resets zoom and offset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WpfWindowMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GenerateImageFromByteArray();

            Matrix m = image.RenderTransform.Value;
            m.OffsetX = 0;
            m.OffsetY = 0;
            m.M11 = 1;
            m.M22 = 1;

            image.RenderTransform = new MatrixTransform(m);
        }

        /// <summary>
        /// Moves image on keyboard events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WpfWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            double x = 0;
            double y = 0;
            switch (e.Key)
            {
                case Key.Right:
                    x = -100;
                    break;

                case Key.Down:
                    y = -100;
                    break;

                case Key.Left:
                    x = 100;
                    break;

                case Key.Up:
                    y = 100;
                    break;

                case Key.PageUp:
                    y = GridLayoutRoot.RowDefinitions[2].ActualHeight;
                    break;

                case Key.PageDown:
                    y = -GridLayoutRoot.RowDefinitions[2].ActualHeight;
                    break;
            }
            Matrix m = image.RenderTransform.Value;
            m.OffsetX += x;
            m.OffsetY += y;

            image.RenderTransform = new MatrixTransform(m);
            image.Focus();
        }
    }
}