using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace IntelligentScissors
{
    public partial class MainForm : Form
    {
        #region DATA
        List<int> parent_list;
        List<Point> AnchorPts;
        List<Point> Mainselction;   // LIST OF ALL SELECTED POINT
        bool ismousedown = false; //IS MOUSE CLICKED OR NOT ?
        bool AutoAnchor_WORK = false; // AUTO ANCHOR ACTIVATION 
        int frequancy = 57; // FREQUENCY OF AUTO ANCHOR
        int curr_source = -1, main_source = -1; // THE
        int Prev_mouse_pos;
        float[] DashPattern = { (float)1, (float)0.000000000001 }; // DRAWING
        float clr = 0.0f;
        float W8interval = .02f;
        Point AnchorSize = new Point(5, 5);  // SHAPE OF ANCHOR POINT 
        Point[] curr_path;
        Pen m_pen = new Pen(Brushes.Orange, 1); // MAIN SELECTION PEN
        Pen c_pen = new Pen(Brushes.Aqua, 1);  // CURRENET PATH PEN
        RGBPixel[,] ImageMatrix;  //2D-ARRAY THAT HOLDS THE IMAGE
        RGBPixel[,] Square_segment; // dikstra square
        Boundary SB; // dikstra square
        #endregion
        #region INITIALIZATION
        void init()
        {
            AnchorPts = new List<Point>();
            Mainselction = new List<Point>();
        }
        #endregion
        #region Constructor
        public MainForm()
        {
            InitializeComponent();
            init();
        }
        #endregion
        #region Reinitialize data variables
        void reset()
        {

            curr_path = null;
            AnchorPts.Clear();
            Mainselction.Clear();
            curr_source = -1;
            Prev_mouse_pos = -1;
            main_source = -1;
            ismousedown = false;
            AutoAnchor_WORK = false;
        }
        #endregion
        #region Click on open image
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                reset();
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }
        #endregion

        public void update(MouseEventArgs e)
        {
            var g = pictureBox1.CreateGraphics();
            if (clr > W8interval * 2)
            {
                if (ImageMatrix != null)
                {
                    var mouseNode = Helper.Flatten(e.X, e.Y, ImageOperations.GetWidth(ImageMatrix));

                    if (curr_source != -1 && Prev_mouse_pos != mouseNode)
                    {
                        Prev_mouse_pos = mouseNode;
                        if (Helper.IN_Boundary(mouseNode, SB, ImageOperations.GetWidth(ImageMatrix)))
                        {
                            int Segment_mouse = Helper.crosspond(mouseNode, SB,
                                ImageOperations.GetWidth(ImageMatrix), ImageOperations.GetWidth(Square_segment));
                            List<Point> segmentpath = new List<Point>();
                            segmentpath = ShortestPath_Operations.Backtracking(parent_list, Segment_mouse, ImageOperations.GetWidth(Square_segment));
                            List<Point> Curpath = Helper.crosspond(segmentpath, SB);
                            curr_path = Curpath.ToArray();
                            if (AutoAnchor_WORK)
                            {
                                double freq = (double)frequancy / 1000;
                                Autoancor.Update(Curpath, freq);
                                List<Point> cooledpath = Autoancor.anchor_path();
                                if (cooledpath.Count > 0)
                                {
                                    Point anchor = cooledpath[cooledpath.Count - 1];
                                    AnchorPts.Add(anchor);
                                    curr_source = Helper.Flatten(anchor.X, anchor.Y, ImageOperations.GetWidth(ImageMatrix));
                                    //curr_path = cooledpath.ToArray();
                                    Helper.AppendToList<Point>(Mainselction, cooledpath);
                                    SB = new Boundary();
                                    SB = ShortestPath_Operations.Square_Boundary(curr_source,
                                        ImageOperations.GetWidth(ImageMatrix) - 1, ImageOperations.GetHeight(ImageMatrix) - 1);
                                    //make a square segment
                                    Square_segment = Helper.COPY_Segment(ImageMatrix, SB);
                                    // currsrc in segment
                                    int newsrc = Helper.crosspond(curr_source, SB, ImageOperations.GetWidth(ImageMatrix), ImageOperations.GetWidth(Square_segment));
                                    parent_list = ShortestPath_Operations.Dijkstra(newsrc, Square_segment);
                                    Autoancor.reset();
                                }
                            }
                        }
                        else
                        {
                            curr_path = ShortestPath_Operations.GenerateShortestPath(curr_source, mouseNode, ImageMatrix).ToArray();
                        }
                    }
                }
                clr = 0.0f;
            }
            if (clr > W8interval)
            {
                pictureBox1.Refresh();
                g.Dispose();
            }
            clr += .019f;
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void pictureBox1_MouseMove_1(object sender, MouseEventArgs e)
        {
            update(e);
            X_TXTBOX.Text = e.X.ToString(); //mouse text box 
            Y_TXTBOX.Text = e.Y.ToString();
            if (pictureBox1.Image != null)
                NODETXTBOX.Text = Helper.Flatten(e.X, e.Y, ImageOperations.GetWidth(ImageMatrix)).ToString();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (ImageMatrix != null)
            {
                var g = e.Graphics;
                for (int i = 0; i < AnchorPts.Count; i++)
                {
                    g.FillEllipse(Brushes.Yellow, new Rectangle(
                        new Point(AnchorPts[i].X - AnchorSize.X / 2, AnchorPts[i].Y - AnchorSize.Y / 2),
                        new Size(AnchorSize)));
                }
                if (curr_path != null)
                    if (curr_path.Length > 10)
                        customDrawer.drawDottedLine(g, c_pen, curr_path, DashPattern);
                if (Mainselction != null && Mainselction.Count > 5)
                    customDrawer.drawDottedLine(e.Graphics, m_pen, Mainselction.ToArray(), DashPattern);
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                var clicked_node = Helper.Flatten(e.X, e.Y, ImageOperations.GetWidth(ImageMatrix));
                if (curr_source != clicked_node)
                {
                    if (curr_source == -1) // in the first click save frist clicked anchor
                        main_source = clicked_node;
                    else
                        Helper.AppendToList<Point>(Mainselction, curr_path);
                    curr_source = clicked_node;
                    AnchorPts.Add(e.Location);
                    SB = new Boundary();
                    SB = ShortestPath_Operations.Square_Boundary(curr_source,
                        ImageOperations.GetWidth(ImageMatrix) - 1, ImageOperations.GetHeight(ImageMatrix) - 1);
                    //make a square segment
                    Square_segment = Helper.COPY_Segment(ImageMatrix, SB);
                    // currsrc in segment
                    int newsrc = Helper.crosspond(curr_source, SB, ImageOperations.GetWidth(ImageMatrix), ImageOperations.GetWidth(Square_segment));
                    parent_list = ShortestPath_Operations.Dijkstra(newsrc, Square_segment);
                    Autoancor.reset();
                }
            }
        }
        #region MOUSE CLICKED
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            ismousedown = true;
        }
        #endregion
        #region MOUSE RELEASE
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            ismousedown = false;
        }
        #endregion
        #region AUTO ANCHOR ACTIVATION
        private void AutoAnchor_Click(object sender, EventArgs e)
        {
            AutoAnchor_WORK= (AutoAnchor_WORK)? false : true;
        }
        #endregion
        #region CHANGE FREQUENCY
        private void frequancyNUD_ValueChanged(object sender, EventArgs e)
        {
            frequancy = (int)frequancyNUD.Value;
        }
        #endregion

        private void crop_Click(object sender, EventArgs e)
        {
            if (curr_source != main_source)
            {
                // check if first node in shortest path range of last node
                //if yes get it fast else try to get it by dikstra
                if (Helper.IN_Boundary(main_source, SB, ImageOperations.GetWidth(ImageMatrix)))
                {
                    int Segment_mouse = Helper.crosspond(main_source, SB, ImageOperations.GetWidth(ImageMatrix), ImageOperations.GetWidth(Square_segment));
                    List<Point> segmentpath = new List<Point>();
                    segmentpath = ShortestPath_Operations.Backtracking(parent_list, Segment_mouse, ImageOperations.GetWidth(Square_segment));
                    curr_path = Helper.crosspond(segmentpath, SB).ToArray();
                }
                else
                    curr_path = ShortestPath_Operations.GenerateShortestPath(curr_source, main_source, ImageMatrix).ToArray();
                Helper.AppendToList<Point>(Mainselction, curr_path);
                //flod fill and crop
                RGBPixel[,] selected_image = floodfill.fill(Mainselction, ImageMatrix);
                CropedImage CI = new CropedImage(selected_image);
                CI.Show();
                reset();
            }
        }

      

        #region CLEAR BUTTON
        private void clear_Click(object sender, EventArgs e)
        {
            reset();
        }
        #endregion

    }
}

#region DRAW THE LINES ON THE PICTURE BOX
public static class customDrawer
{
    
    public static void drawDottedLine(Graphics g, Pen p, Point[] arr, float[] _dash_vals)
    {
        p.DashPattern = _dash_vals;
        g.DrawCurve(p, arr);
    }

    public static void drawDottedLine(Graphics g, Pen p, Point A, Point B, float[] _dash_vals)
    {
        Point[] arr = new Point[2];
        arr[0] = A;
        arr[1] = B;

        drawDottedLine(g, p, arr, _dash_vals);
    }
}
#endregion
