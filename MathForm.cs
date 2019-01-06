using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyMathTool
{
    public partial class MathForm : Form
    {
        Point Original_Point = new Point();
        public MathForm()
        {
            InitializeComponent();
            Original_Point.X = pictureBox1.Width / 2;
            Original_Point.Y = pictureBox1.Height / 2;
        }

        private void DrawAxis(PictureBox pictureBox)
        {

        }

        private void DrawVerticalLine(PictureBox pictureBox, int y)
        {

        }

        private void DrawLevelLine(PictureBox pictureBox,int x)
        {

        }
        private void DrawPoint(PictureBox pictureBox)
        {

        }
    }
}
