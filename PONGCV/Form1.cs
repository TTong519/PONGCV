using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PONGCV
{
    public partial class Form1 : Form
    {
        VideoCapture capture = new VideoCapture();
        Rectangle rect = new();
        Rectangle ballRect = new(200, 200, ballSize, ballSize);
        const int speed = 10;
        const int ballSize = 20;
        int xSpeed = speed;
        int ySpeed = speed;
        public Form1()
        {
            InitializeComponent();
        }
        public void GetFrame(object sender, EventArgs e)
        {

            if (!capture.IsOpened) return;
            if (!capture.Grab()) return;

            using Mat currentFrame = capture.QueryFrame();
            using Mat flipped = new Mat();
            CvInvoke.Flip(currentFrame, flipped, Emgu.CV.CvEnum.FlipType.Horizontal);
            imageBox1.Image = flipped.Clone();
        }
        public void IDPaddle(object sender, EventArgs e)
        {
            Mat HSV = new Mat();
            CvInvoke.CvtColor(imageBox1.Image, HSV, Emgu.CV.CvEnum.ColorConversion.Rgb2Hsv);
            Mat mask = new Mat();
            CvInvoke.InRange(HSV, (ScalarArray)new MCvScalar(110, 190, 50), (ScalarArray)new MCvScalar(126, 255, 255), mask);
            CvInvoke.MedianBlur(mask, mask, 25);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(mask, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxNone);
            CvInvoke.CvtColor(HSV, HSV, ColorConversion.Hsv2Rgb);
            CvInvoke.DrawContours(HSV, contours, -1, new MCvScalar(255, 255, 255), 1);
            rect = CvInvoke.BoundingRectangle(contours[0]);
            CvInvoke.Rectangle(HSV, rect, new MCvScalar(0, 255, 255), 5);
            Moments moments = CvInvoke.Moments(contours[0]);
            double centerX = moments.M10 / moments.M00;
            double centerY = moments.M01 / moments.M00;

            CvInvoke.DrawMarker(HSV, new Point((int)centerX, (int)centerY), new MCvScalar(0, 255, 0), MarkerTypes.TiltedCross, 20, 5);
            imageBox1.Image = HSV;
            imageBox2.Image = mask;

        }
        public void doGame(object sender, EventArgs e)
        {
            Mat img = imageBox1.Image as Mat;

            // Ball collision with left and right walls
            if (ballRect.X < 0)
            {
                xSpeed = speed;
            }
            else if (ballRect.X > Size.Width - ballRect.Width - 150)
            {
                xSpeed = -speed;
            }

            // Ball collision with top wall
            if (ballRect.Y < 0)
            {
                ySpeed = speed;
            }

            // Ball collision with bottom wall
            else if (ballRect.Y > Size.Height - ballRect.Height)
            {
                ySpeed = -speed;
            }

            // Ball collision with paddle
            if (ballRect.IntersectsWith(rect))
            {
                // Calculate intersection rectangle
                Rectangle intersection = Rectangle.Intersect(ballRect, rect);

                // Determine collision side
                if (intersection.Width < intersection.Height)
                {
                    // Collision on left or right side of paddle
                    xSpeed = -xSpeed;
                    // Move ball outside the paddle to prevent sticking
                    if (ballRect.X < rect.X)
                        ballRect.X = rect.X - ballRect.Width;
                    else
                        ballRect.X = rect.X + rect.Width;
                }
                else
                {
                    // Collision on top or bottom of paddle
                    ySpeed = -ySpeed;
                    // Move ball outside the paddle to prevent sticking
                    if (ballRect.Y < rect.Y)
                        ballRect.Y = rect.Y - ballRect.Height;
                    else
                        ballRect.Y = rect.Y + rect.Height;
                }
            }

            // Update ball position
            ballRect.X += xSpeed;
            ballRect.Y += ySpeed;

            // Draw the ball
            CvInvoke.Rectangle(img, ballRect, new MCvScalar(0, 0, 255), -1);
            imageBox1.Image = img;
        }
        private void imageBox1_Click(object sender, EventArgs e)
        {
            Application.Idle += GetFrame;
            Application.Idle += IDPaddle;
            Application.Idle += doGame;
        }
    }
}
