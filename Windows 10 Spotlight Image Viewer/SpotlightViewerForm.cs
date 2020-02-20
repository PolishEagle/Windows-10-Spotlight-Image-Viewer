using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Forms;

namespace Windows_10_Spotlight_Image_Viewer
{
    public partial class SpotlightViewerForm : Form
    {
        System.Timers.Timer timer = new System.Timers.Timer();

        // Asset path
        string savePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        string dirPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets");

        // Thumbnail sizes.
        private const int ThumbWidth = 100;
        private const int ThumbHeight = 100;
        Size imageSmallSize = new Size(ThumbWidth, ThumbHeight);
        Size imageLargeSize = new Size(600, 400);

        // The selected PictureBox.
        private PictureBox SelectedPictureBox = null;

        private string DefaultStatusBarText = "Left click to view larger and right click to save";

        public SpotlightViewerForm()
        {
            InitializeComponent();

            // Setup the paths
            txtPath.Text = savePath;
            toolStripStatusLabel1.Text = DefaultStatusBarText;

            // Setup the timer
            timer.Interval = 3000;
            timer.Elapsed += new ElapsedEventHandler(OnTimerEvent);

            // Fill the layout with the pictures
            PopulateFlowLayoutPanel();
        }

        private void PopulateFlowLayoutPanel()
        {
            // Remove all the panel controls and re-add
            List<Control> flowLayoutPanelControls = flowLayoutPanel1.Controls.Cast<Control>().ToList();

            foreach (Control curPic in flowLayoutPanelControls)
            {
                flowLayoutPanel1.Controls.Remove(curPic);
                (curPic as PictureBox).Image.Dispose();
                curPic.Dispose();
            }

            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);

            if (!dirInfo.Exists)
            {
                MessageBox.Show("Error loading Spotlight images", "Directory not found");
                return;
            }

            foreach (FileInfo finfo in dirInfo.GetFiles())
            {
                try
                {
                    Bitmap img = new Bitmap(finfo.FullName);
                    PictureBox pic = new PictureBox();
                    pic.ClientSize = imageSmallSize;
                    pic.Image = img;
                    pic.ImageLocation = finfo.FullName;

                    // If the picture isn't widescreen then move on
                    if (chkBoxWidescreen.Checked && (double)img.Width/img.Height < 1.1)
                    {
                        img.Dispose();
                        pic.Dispose();
                        continue;
                    }

                    // If the image is too big, zoom.
                    if ((pic.Image.Width > ThumbWidth) ||
                    (pic.Image.Height > ThumbHeight))
                    {
                        pic.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    else
                    {
                        pic.SizeMode = PictureBoxSizeMode.CenterImage;
                    }

                    // Add the Click event handler.
                    pic.Click += PictureBox_Click;

                    pic.Parent = flowLayoutPanel1;
                }
                catch (Exception e)
                {

                }
            }
        }

        // Select the clicked PictureBox.
        private void PictureBox_Click(object sender, EventArgs e)
        {
            MouseEventArgs mouseEvent = (MouseEventArgs)e;
            PictureBox pic = sender as PictureBox;

            if (mouseEvent.Button.Equals(MouseButtons.Left))
            {
                if (SelectedPictureBox == pic)
                {
                    SelectedPictureBox = null;
                    pic.ClientSize = imageSmallSize;
                    return;
                }

                // Deselect the previous PictureBox.
                if (SelectedPictureBox != null)
                    SelectedPictureBox.ClientSize = imageSmallSize;

                // Select the clicked PictureBox.
                pic.ClientSize = imageLargeSize;
                SelectedPictureBox = pic;
            }
            else if (mouseEvent.Button.Equals(MouseButtons.Right))
            {
                FileInfo finfo = new FileInfo(pic.ImageLocation);

                if (File.Exists(Path.Combine(txtPath.Text, finfo.Name + ".jpg")))
                {
                    toolStripStatusLabel1.ForeColor = Color.Red;
                    toolStripStatusLabel1.Text = "ERROR: Picture already exists";
                }
                else
                {
                    File.Copy(pic.ImageLocation, Path.Combine(txtPath.Text, finfo.Name + ".jpg"));
                    toolStripStatusLabel1.ForeColor = Color.DarkGreen;
                    toolStripStatusLabel1.Text = "Image saved!";
                }
            }
            
            timer.Enabled = true;
        }

        private void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                toolStripStatusLabel1.Text = DefaultStatusBarText;
                toolStripStatusLabel1.ForeColor = Color.Black;
            }
            catch (Exception e1) { }
            timer.Enabled = false;

        }

        private void chkBoxWidescreen_CheckedChanged(object sender, EventArgs e)
        {
            // repopulate the control with the new state
            PopulateFlowLayoutPanel();
        }
    }
}
