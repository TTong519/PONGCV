using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PONGCV
{
    public partial class Form1 : Form
    {
        private readonly VideoCapture capture = new VideoCapture();
        private readonly Stopwatch loopStopwatch = new Stopwatch();
        private CancellationTokenSource? loopCts;
        private const double CameraIntervalMs = 33.0; // ~30 Hz
        private const double DetectionIntervalMs = 100.0; // ~10 Hz
        private const double PhysicsIntervalMs = 16.0; // ~60 Hz
        private double camAcc = 0.0;
        private double detAcc = 0.0;
        private double physAcc = 0.0;
        private Rectangle rect = Rectangle.Empty;
        private const int ballSize = 20;
        private float ballX = 200f;
        private float ballY = 200f;
        private float vx = 200f;
        private float vy = 200f;
        private double simTime = 0.0;

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
        }

        private void StartLoop()
        {
            if (loopCts != null) return;
            loopCts = new CancellationTokenSource();
            var token = loopCts.Token;
            loopStopwatch.Restart();
            camAcc = detAcc = physAcc = 0.0;
            simTime = 0.0;
            Task.Run(async () =>
            {
                long prevMs = loopStopwatch.ElapsedMilliseconds;
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        long nowMs = loopStopwatch.ElapsedMilliseconds;
                        double deltaMs = nowMs - prevMs;
                        prevMs = nowMs;
                        camAcc += deltaMs; detAcc += deltaMs; physAcc += deltaMs; simTime += deltaMs / 1000.0;
                        if (camAcc >= CameraIntervalMs)
                        {
                            camAcc -= CameraIntervalMs;
                            if (!chkSimulate.Checked)
                            {
                                using Mat frame = capture.QueryFrame();
                                if (frame != null && !frame.IsEmpty)
                                {
                                    using Mat flipped = new Mat();
                                    CvInvoke.Flip(frame, flipped, FlipType.Horizontal);
                                    this.BeginInvoke((Action)(() =>
                                    {
                                        imageBox1.Image?.Dispose();
                                        imageBox1.Image = flipped.Clone();
                                    }));
                                }
                            }
                            else
                            {
                                this.BeginInvoke((Action)(() =>
                                {
                                    int w = Math.Max(1, imageBox1.ClientSize.Width);
                                    int h = Math.Max(1, imageBox1.ClientSize.Height);
                                    imageBox1.Image?.Dispose();
                                    Mat blank = new Mat(h, w, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
                                    blank.SetTo(new MCvScalar(0, 0, 0));
                                    imageBox1.Image = blank.Clone();
                                }));
                            }
                        }
                        if (detAcc >= DetectionIntervalMs)
                        {
                            detAcc -= DetectionIntervalMs;
                            Mat? src = null;
                            try
                            {
                                this.Invoke((Action)(() => { src = imageBox1.Image as Mat; }));
                                if (chkSimulate.Checked)
                                {
                                    int w = imageBox1.ClientSize.Width;
                                    int h = imageBox1.ClientSize.Height;
                                    int paddleW = 30;
                                    int paddleH = Math.Max(40, h / 4);
                                    int x = Math.Max(0, w - paddleW - 10);
                                    int y = (int)((Math.Sin(simTime * 1.5) * 0.5 + 0.5) * (h - paddleH));
                                    rect = new Rectangle(x, y, paddleW, paddleH);
                                    this.BeginInvoke((Action)(() =>
                                    {
                                        imageBox2.Image?.Dispose();
                                        imageBox2.Image = null;
                                    }));
                                }
                                else
                                {
                                    if (src == null) { rect = Rectangle.Empty; continue; }
                                    if (src.IsEmpty) { rect = Rectangle.Empty; continue; }
                                    using Mat smallFrame = new Mat();
                                    CvInvoke.Resize(src, smallFrame, new System.Drawing.Size(Math.Max(1, src.Width / 2), Math.Max(1, src.Height / 2)), 0, 0, Inter.Linear);
                                    using Mat hsv = new Mat();
                                    CvInvoke.CvtColor(smallFrame, hsv, ColorConversion.Bgr2Hsv);
                                    (MCvScalar lower, MCvScalar upper) = GetColorThresholds();
                                    using Mat mask = new Mat();
                                    CvInvoke.InRange(hsv, new ScalarArray(lower), new ScalarArray(upper), mask);
                                    CvInvoke.MedianBlur(mask, mask, 7);
                                    using VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                                    using Mat hierarchy = new Mat();
                                    CvInvoke.FindContours(mask, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                                    if (contours.Size == 0)
                                    {
                                        rect = Rectangle.Empty;
                                        this.BeginInvoke((Action)(() =>
                                        {
                                            imageBox2.Image?.Dispose();
                                            imageBox2.Image = mask.Clone();
                                        }));
                                    }
                                    else
                                    {
                                        int bestIdx = 0; double bestArea = 0;
                                        for (int i = 0; i < contours.Size; i++)
                                        {
                                            double area = CvInvoke.ContourArea(contours[i]);
                                            if (area > bestArea) { bestArea = area; bestIdx = i; }
                                        }
                                        Rectangle smallRect = CvInvoke.BoundingRectangle(contours[bestIdx]);
                                        float scaleX = (float)src.Width / smallFrame.Width;
                                        float scaleY = (float)src.Height / smallFrame.Height;
                                        rect = new Rectangle((int)(smallRect.X * scaleX), (int)(smallRect.Y * scaleY), (int)(smallRect.Width * scaleX), (int)(smallRect.Height * scaleY));
                                        using Mat display = src.Clone();
                                        CvInvoke.Rectangle(display, rect, new MCvScalar(0, 255, 255), 4);
                                        Moments m = CvInvoke.Moments(contours[bestIdx]);
                                        if (m.M00 != 0)
                                        {
                                            int cx = (int)((m.M10 / m.M00) * scaleX);
                                            int cy = (int)((m.M01 / m.M00) * scaleY);
                                            CvInvoke.DrawMarker(display, new System.Drawing.Point(cx, cy), new MCvScalar(0, 255, 0), MarkerTypes.TiltedCross, 20, 2);
                                        }
                                        this.BeginInvoke((Action)(() =>
                                        {
                                            imageBox1.Image?.Dispose();
                                            imageBox1.Image = display.Clone();
                                            imageBox2.Image?.Dispose();
                                            imageBox2.Image = mask.Clone();
                                        }));
                                    }
                                }
                            }
                            catch { rect = Rectangle.Empty; }
                            finally { src?.Dispose(); }
                        }
                        if (physAcc >= PhysicsIntervalMs)
                        {
                            double steps = Math.Floor(physAcc / PhysicsIntervalMs);
                            for (int s = 0; s < steps; s++)
                            {
                                physAcc -= PhysicsIntervalMs;
                                float dt = (float)(PhysicsIntervalMs / 1000.0);
                                ballX += vx * dt;
                                ballY += vy * dt;
                                int maxX = imageBox1.ClientSize.Width - ballSize;
                                int maxY = imageBox1.ClientSize.Height - ballSize;
                                if (ballX < 0) { ballX = 0; vx = Math.Abs(vx); }
                                else if (ballX > maxX) { ballX = maxX; vx = -Math.Abs(vx); }
                                if (ballY < 0) { ballY = 0; vy = Math.Abs(vy); }
                                else if (ballY > maxY) { ballY = maxY; vy = -Math.Abs(vy); }
                                if (rect != Rectangle.Empty)
                                {
                                    RectangleF ballF = new RectangleF(ballX, ballY, ballSize, ballSize);
                                    RectangleF rectF = rect;
                                    if (ballF.IntersectsWith(rectF))
                                    {
                                        RectangleF intersection = RectangleF.Intersect(ballF, rectF);
                                        if (intersection.Width < intersection.Height)
                                        {
                                            if (ballX < rectF.X) { ballX = rectF.X - ballSize - 1; vx = -Math.Abs(vx); }
                                            else { ballX = rectF.Right + 1; vx = Math.Abs(vx); }
                                        }
                                        else
                                        {
                                            if (ballY < rectF.Y) { ballY = rectF.Y - ballSize - 1; vy = -Math.Abs(vy); }
                                            else { ballY = rectF.Bottom + 1; vy = Math.Abs(vy); }
                                        }
                                        float ballCenterY = ballY + ballSize / 2f;
                                        float rectCenterY = rectF.Y + rectF.Height / 2f;
                                        float offset = (ballCenterY - rectCenterY) / rectF.Height;
                                        vy += offset * 200f;
                                        float maxSpeed = 800f; float minSpeed = 50f;
                                        float speed = (float)Math.Sqrt(vx * vx + vy * vy);
                                        if (speed > maxSpeed) { float scale = maxSpeed / speed; vx *= scale; vy *= scale; }
                                        else if (speed < minSpeed) { float scale = minSpeed / (speed + 0.001f); vx *= scale; vy *= scale; }
                                    }
                                }
                            }
                            this.BeginInvoke((Action)(() =>
                            {
                                Mat src2 = imageBox1.Image as Mat;
                                if (src2 == null) return;
                                using Mat display = src2.Clone();
                                CvInvoke.Rectangle(display, new System.Drawing.Rectangle((int)ballX, (int)ballY, ballSize, ballSize), new MCvScalar(0, 0, 255), -1);
                                imageBox1.Image?.Dispose();
                                imageBox1.Image = display.Clone();
                            }));
                        }
                        await Task.Delay(1, token);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception) { }
                finally { loopStopwatch.Stop(); }
            }, token);
        }

        private (MCvScalar lower, MCvScalar upper) GetColorThresholds()
        {
            if (comboPaddleColor.InvokeRequired)
            {
                return (new MCvScalar(110, 90, 50), new MCvScalar(126, 255, 255));
            }
            string sel = comboPaddleColor.SelectedItem as string ?? "Blue";
            return sel switch
            {
                "Red" => (new MCvScalar(0, 120, 50), new MCvScalar(10, 255, 255)),
                "Green" => (new MCvScalar(35, 80, 40), new MCvScalar(85, 255, 255)),
                _ => (new MCvScalar(100, 90, 50), new MCvScalar(140, 255, 255)), // Blue default
            };
        }

        private void imageBox1_Click(object sender, EventArgs e)
        {
            if (loopCts == null)
            {
                StartLoop();
            }
            else
            {
                loopCts.Cancel(); loopCts = null; loopStopwatch.Reset();
            }
        }

        private void comboPaddleColor_SelectedIndexChanged(object? sender, EventArgs e) { }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (loopCts != null) { loopCts.Cancel(); loopCts = null; }
            imageBox1.Image?.Dispose(); imageBox2.Image?.Dispose();
            capture?.Dispose();
        }
    }
}
