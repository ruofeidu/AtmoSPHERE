using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO.Ports;
using Microsoft.Kinect;
using System.Timers;

namespace WpfApplicationKinectSkeletonTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The timer
        /// </summary>
        DispatcherTimer m_timer, m_circleTimer;
        /// <summary>
        /// x, y and counter
        /// </summary>
        Point m_point, m_prevPoint, m_minPoint, m_maxPoint, m_thresPoint; 
        int x = 0, y = 0, z = 0;
        int counter = 0;
        SerialPort port;
        int circleStaff = 0;

        bool m_serial_out = true;
        bool m_calibarting = false; 
        bool m_trackingPeople = false;
        bool m_circleTracking = true; 

        public MainWindow()
        {
            InitializeComponent();
            m_timer = new DispatcherTimer();
            m_timer.Interval = TimeSpan.FromMilliseconds(500);
            if (m_trackingPeople)
            {
                m_timer.Tick += new EventHandler(TimerTick);
                m_timer.Start();
            }

            m_circleTimer = new DispatcherTimer();
            m_circleTimer.Interval = TimeSpan.FromMilliseconds(500);
            
            if (!m_trackingPeople)
            {
                m_circleTimer.Tick += new EventHandler(CircleTimerTick);
                m_circleTimer.Start();
            }
            
            
            //m_minPoint = new Point(1000, 1000);
            //m_maxPoint = new Point(0, 0);
            
            // the room
            //m_minPoint = new Point(79, 191);
            //m_maxPoint = new Point(593, 448);
            
            // the hcil
            m_minPoint = new Point(150, 150);
            m_maxPoint = new Point(388, 250);
            
            
            //m_minPoint = new Point(200, 203);
            //m_maxPoint = new Point(388, 213);
            
            //m_thresPoint = new Point(450, 315);
            //m_thresPoint = new Point(270, 230); 
            //m_thresPoint = new Point(380, 315);
            m_thresPoint = new Point(420, 280); 

            if (m_serial_out){
                Console.WriteLine("Listing Serial Ports: " + SerialPort.GetPortNames().Length);
                foreach (String name in SerialPort.GetPortNames())
                {
                    Console.WriteLine(name);
                    PortList.Items.Add(name);
                }
                PortList.Text = PortList.Items[0].ToString();
            }

            m_point.X = 0;
            m_point.Y = 0;
            this.sendMessage(); 
        }

        private void sendMessage()
        {
            if (!m_serial_out) return; 
            if (port != null)
            {
                if (m_trackingPeople)
                {
                    m_point.X = Math.Max(0, m_point.X);
                    m_point.Y = Math.Max(0, m_point.Y);
                    m_point.X = Math.Min(m_thresPoint.X, m_point.X);
                    m_point.Y = Math.Min(m_thresPoint.Y, m_point.Y);
                    port.WriteLine((m_thresPoint.Y - m_point.Y) + "," + m_point.X);
                }
                else
                {
                    port.WriteLine((m_point.Y) + "," + (m_point.X));
                }
            }
            Console.Write(port.ReadLine());

        }

        protected Point mapping(Point pt)
        {
            Point ans = new Point(pt.X, pt.Y);

            if (m_calibarting)
            {
                if (pt.X > m_maxPoint.X) m_maxPoint.X = pt.X;
                if (pt.Y > m_maxPoint.Y) m_maxPoint.Y = pt.Y;
                if (pt.X != 0 && pt.X < m_minPoint.X) m_minPoint.X = pt.X;
                if (pt.Y != 0 && pt.Y < m_minPoint.Y) m_minPoint.Y = pt.Y;
            }

            //if (ans.X < m_minPoint.X) ans.X = m_minPoint.X;
            //if (ans.Y < m_minPoint.Y) ans.Y = m_minPoint.Y;

            ans.X = (ans.X - m_minPoint.X) / (m_maxPoint.X - m_minPoint.X) * m_thresPoint.X;
            ans.Y = (ans.Y - m_minPoint.Y) / (m_maxPoint.Y - m_minPoint.Y) * m_thresPoint.Y; 

            if (ans.X > m_thresPoint.X) ans.X = m_thresPoint.X; if (ans.X < 0) ans.X = 0;
            if (ans.Y > m_thresPoint.Y) ans.Y = m_thresPoint.Y; if (ans.Y < 0) ans.Y = 0; 
            return ans;
        }

        public void UpdateDebug()
        {
            txtDebug.Text = "Kinect: " + m_kinect.currentX + "," + m_kinect.currentY + "," + m_kinect.currentZ + "@" + m_kinect.currentPlayer + "\n" +
                "\n" + "Amplitude: " + m_audio.amplitude + "\n" + 
               "Sandbox: " + m_point.X + "," + m_point.Y + "\n" +
               "Calibration: " + m_minPoint.X + "," + m_minPoint.Y + "," + m_maxPoint.X + "," + m_maxPoint.Y + "\n" +
               "Ratio: " + m_thresPoint.X / (m_maxPoint.X - m_minPoint.X) + "," + m_thresPoint.Y / (m_maxPoint.Y - m_minPoint.Y);
        }

        public void CircleTimerTick(object sender, EventArgs e)
        {
            int total = 16; 
            circleStaff = (circleStaff + 1) % total;
            double para = (double)circleStaff / total; 
            
            double radius = Math.Min( m_thresPoint.X, m_thresPoint.Y) / 2;
            double m = Math.Min(m_thresPoint.X, m_thresPoint.Y); 
            double n = m;

            if (m_circleTracking)
            {
                switch (m_kinect.currentPlayer)
                {
                    case 0: return; 
                    case 1: radius = radius / 2; break;
                    case 2: radius = radius / 3 * 2; break;
                    case 3: radius = radius / 4 * 3; break;
                    case 4: radius = radius / 5 * 4; break;
                    case 5: radius = radius / 2; break;
                    default: radius = radius - 10; break;
                }
            }

            m_prevPoint = m_point;
            m_point = new Point(m / 2 + Math.Cos(2 * Math.PI * para) * radius, n / 2 + Math.Sin(2 * Math.PI * para) * radius);
            UpdateDebug(); 
            Line line = new Line();
            line.Stroke = System.Windows.Media.Brushes.Red;
            line.X1 = m_prevPoint.X;
            line.Y1 = m_prevPoint.Y;
            line.X2 = m_point.X;
            line.Y2 = m_point.Y;
            line.HorizontalAlignment = HorizontalAlignment.Left;
            line.VerticalAlignment = VerticalAlignment.Top;
            line.StrokeThickness = 3;
            m_grid.Children.Add(line);

            //this.txtDebug.Text = "Current Players: " + m_kinect.currentPlayer; 
            this.sendMessage(); 
        }

        public void TimerTick(object sender, EventArgs e)
        {
            if (m_kinect.currentPlayer == 0) return; 
            Line line = new Line();
            if (!m_calibarting)
            {
                line.Stroke = System.Windows.Media.Brushes.Red;
                m_prevPoint = m_point;
                line.X1 = m_prevPoint.X;
                line.Y1 = m_prevPoint.Y;
            }

            m_point = new Point(m_kinect.currentX, m_kinect.currentY);
            m_point = mapping(m_point);

            if (!m_calibarting)
            {
                line.X2 = m_point.X;
                line.Y2 = m_point.Y;
                line.HorizontalAlignment = HorizontalAlignment.Left;
                line.VerticalAlignment = VerticalAlignment.Top;
                line.StrokeThickness = 3;
            }
            UpdateDebug(); 

            if (!m_point.Equals(m_prevPoint)) 
            {
                m_grid.Children.Add(line);
            }

            this.sendMessage(); 
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
        }

        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor oldSensor = (KinectSensor)e.OldValue;
            StopKinect(oldSensor);

            KinectSensor newSensor = (KinectSensor)e.NewValue;

            newSensor.DepthStream.Enable();
            newSensor.SkeletonStream.Enable();

            try
            { 
                newSensor.Start(); 
            }
            catch (System.IO.IOException)
            { 
                kinectSensorChooser1.AppConflictOccurred(); 
            }

            txtDebug.Text = "Sandflow";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(kinectSensorChooser1.Kinect);
        }

        void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.AudioSource.Stop();
            }
        }

        private void PortList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (port != null)
                port.Close();
            try
            {
                port = new SerialPort(PortList.SelectedItem.ToString(), 9600, Parity.None, 8, StopBits.One);                
                port.Open();
            }
            catch 
            {
                MessageBox.Show("No Arduino Connected.....\n\n");                
            }
        }

    }
}
