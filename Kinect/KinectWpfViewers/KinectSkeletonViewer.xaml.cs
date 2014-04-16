//------------------------------------------------------------------------------
// <copyright file="KinectSkeletonViewer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Microsoft.Kinect;
    using System.Diagnostics; 

    public enum ImageType
    {
        Color,
        Depth,
    }

    internal enum TrackingMode
    {
        DefaultSystemTracking,
        Closest1Player,
        Closest2Player,
        Sticky1Player,
        Sticky2Player,
        MostActive1Player,
        MostActive2Player
    }


    /// <summary>
    /// Interaction logic for KinectSkeletonViewer.xaml
    /// </summary>
    public partial class KinectSkeletonViewer : ImageViewer, INotifyPropertyChanged
    {
        private const float ActivityFalloff = 0.98f;
        private readonly List<ActivityWatcher> recentActivity = new List<ActivityWatcher>();
        private readonly List<int> activeList = new List<int>();
        private List<KinectSkeleton> skeletonCanvases;
        private List<Dictionary<JointType, JointMapping>> jointMappings = new List<Dictionary<JointType, JointMapping>>();
        private Skeleton[] skeletonData;

        public double currentX;
        public double currentY;
        public double currentZ;
        public int currentPlayer;

        private double X_LOW = 0;
        private double X_HIGH = 600;
        private double Y_LOW = 0;
        private double Y_HIGH = 600; 

        private double MAX_X = 450;
        private double MAX_Y = 315;

        private double FALSE_X = 19;
        private double tempX = 0;
        private double tempY = 0; 

        public KinectSkeletonViewer()
        {
            InitializeComponent();
            this.ShowJoints = true;
            this.ShowBones = true;
            this.ShowCenter = true;

            currentX = 0;
            currentY = 0; 
        }


        public bool ShowBones { get; set; }

        public bool ShowJoints { get; set; }

        public bool ShowCenter { get; set; }

        public ImageType ImageType { get; set; }

        internal TrackingMode TrackingMode { get; set; }

        public void HideAllSkeletons()
        {
            if (this.skeletonCanvases != null)
            {
                foreach (KinectSkeleton skeletonCanvas in this.skeletonCanvases)
                {
                    skeletonCanvas.Reset();
                }
            }
        }

        protected override void OnKinectChanged(KinectSensor oldKinectSensor, KinectSensor newKinectSensor)
        {
            if (oldKinectSensor != null)
            {
                oldKinectSensor.AllFramesReady -= this.KinectAllFramesReady;
                this.HideAllSkeletons();
            }

            if (newKinectSensor != null && newKinectSensor.Status == KinectStatus.Connected)
            {
                newKinectSensor.AllFramesReady += this.KinectAllFramesReady;
            }
        }

        private void KinectAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if ((this.Kinect == null) || !((KinectSensor)sender).SkeletonStream.IsEnabled)
            {
                return;
            }

            bool haveSkeletonData = false;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if (this.skeletonCanvases == null)
                    {
                        this.CreateListOfSkeletonCanvases();
                    }

                    if ((this.skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);

                    haveSkeletonData = true;
                }
            }

            if (haveSkeletonData)
            {
                using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
                {
                    if (depthImageFrame != null)
                    {
                        int trackedSkeletons = 0; 
                        currentZ = this.skeletonData.Length; 
                        bool first = true;

                        //skeleton = ChooseTrackedSkeletons(this.skeletonData); 
                        tempX = FALSE_X;
                        bool ignore = false;
                        int players = 0; 

                        foreach (Skeleton skeleton in this.skeletonData)
                        {
                            Dictionary<JointType, JointMapping> jointMapping = this.jointMappings[trackedSkeletons];
                            jointMapping.Clear();

                            KinectSkeleton skeletonCanvas = this.skeletonCanvases[trackedSkeletons++];
                            skeletonCanvas.ShowBones = false; 
                            skeletonCanvas.ShowJoints = true; 
                            skeletonCanvas.ShowCenter = true;

                            if (this.TryValid(depthImageFrame, skeleton.Position))
                            {
                                ++players;
                            }

                            if (!ignore)
                            {
                                foreach (Joint joint in skeleton.Joints)
                                {
                                    //Point mappedPoint = this.GetPosition2DLocation(depthImageFrame, joint.Position);
                                    Point mappedPoint = this.GetMappedPoint(depthImageFrame, skeleton.Position, true);

                                    first = false;

                                    jointMapping[joint.JointType] = new JointMapping
                                    {
                                        Joint = joint,
                                        MappedPoint = mappedPoint
                                    };
                                }

                                Point centerPoint = this.GetMappedPoint(depthImageFrame, skeleton.Position, first);
                                double scale = this.RenderSize.Width / 640;
                                skeletonCanvas.RefreshSkeleton(skeleton, jointMapping, centerPoint, scale);
                            }

                            if (tempX != FALSE_X)
                            {
                                ignore = true; 
                            }
                        }
                        currentPlayer = players; 
                        if (ImageType == ImageType.Depth)
                        {
                            this.ChooseTrackedSkeletons(this.skeletonData);
                        }
                    }
                }
            }
        }

        private Point GetPosition2DLocation(DepthImageFrame depthFrame, SkeletonPoint skeletonPoint)
        {
            DepthImagePoint depthPoint = depthFrame.MapFromSkeletonPoint(skeletonPoint);

            switch (ImageType)
            {
                case ImageType.Color:
                    ColorImagePoint colorPoint = depthFrame.MapToColorImagePoint(depthPoint.X, depthPoint.Y, this.Kinect.ColorStream.Format);

                    // map back to skeleton.Width & skeleton.Height
                    return new Point(
                        (int)(this.RenderSize.Width * colorPoint.X / this.Kinect.ColorStream.FrameWidth),
                        (int)(this.RenderSize.Height * colorPoint.Y / this.Kinect.ColorStream.FrameHeight));


                case ImageType.Depth:
                    return new Point(
                        (int)(this.RenderSize.Width * depthPoint.X / depthFrame.Width),
                        (int)(this.RenderSize.Height * depthPoint.Y / depthFrame.Height));
                default:
                    throw new ArgumentOutOfRangeException("ImageType was a not expected value: " + ImageType.ToString());
            }
        }

        private bool TryValid(DepthImageFrame depthFrame, SkeletonPoint skeletonPoint)
        {
            DepthImagePoint depthPoint = depthFrame.MapFromSkeletonPoint(skeletonPoint);

            switch (ImageType)
            {
                case ImageType.Color:
                    ColorImagePoint colorPoint = depthFrame.MapToColorImagePoint(depthPoint.X, depthPoint.Y, this.Kinect.ColorStream.Format);

                    if (tempX == FALSE_X) return false; else return true; 

                case ImageType.Depth: return false;
                default: return false; 
            }
        }
        

        private Point GetMappedPoint(DepthImageFrame depthFrame, SkeletonPoint skeletonPoint, bool first)
        {
            DepthImagePoint depthPoint = depthFrame.MapFromSkeletonPoint(skeletonPoint);

            switch (ImageType)
            {
                case ImageType.Color:
                    ColorImagePoint colorPoint = depthFrame.MapToColorImagePoint(depthPoint.X, depthPoint.Y, this.Kinect.ColorStream.Format);

                    // map back to skeleton.Width & skeleton.Height
                    if (colorPoint.X != FALSE_X)
                    {
                        currentX = colorPoint.X;
                        currentY = colorPoint.Y;
                    }
                    tempX = colorPoint.X; 
                    //Console.WriteLine(colorPoint.X + "," + colorPoint.Y); 
                    
                    return new Point(
                        (int)(this.RenderSize.Width * colorPoint.X / this.Kinect.ColorStream.FrameWidth),
                        (int)(this.RenderSize.Height * colorPoint.Y / this.Kinect.ColorStream.FrameHeight));
                     
                case ImageType.Depth:
                    return new Point(
                        (int)(this.RenderSize.Width * depthPoint.X / depthFrame.Width),
                        (int)(this.RenderSize.Height * depthPoint.Y / depthFrame.Height));
                default:
                    throw new ArgumentOutOfRangeException("ImageType was a not expected value: " + ImageType.ToString());
            }
        }

        private void CreateListOfSkeletonCanvases()
        {
            this.skeletonCanvases = new List<KinectSkeleton>
                {
                    this.skeletonCanvas1,
                    this.skeletonCanvas2,
                    this.skeletonCanvas3,
                    this.skeletonCanvas4,
                    this.skeletonCanvas5,
                    this.skeletonCanvas6
                };

            this.skeletonCanvases.ForEach(s => this.jointMappings.Add(new Dictionary<JointType, JointMapping>()));
        }

        // NOTE: The ChooseTrackedSkeletons part of the KinectSkeletonViewer would be useful
        // separate from the SkeletonViewer.
        private void ChooseTrackedSkeletons(IEnumerable<Skeleton> skeletonDataValue)
        {
            switch (TrackingMode)
            {
                case TrackingMode.Closest1Player:
                    this.ChooseClosestSkeletons(skeletonDataValue, 1);
                    break;
                case TrackingMode.Closest2Player:
                    this.ChooseClosestSkeletons(skeletonDataValue, 2);
                    break;
                case TrackingMode.Sticky1Player:
                    this.ChooseOldestSkeletons(skeletonDataValue, 1);
                    break;
                case TrackingMode.Sticky2Player:
                    this.ChooseOldestSkeletons(skeletonDataValue, 2);
                    break;
                case TrackingMode.MostActive1Player:
                    this.ChooseMostActiveSkeletons(skeletonDataValue, 1);
                    break;
                case TrackingMode.MostActive2Player:
                    this.ChooseMostActiveSkeletons(skeletonDataValue, 2);
                    break;
            }
        }

        private void ChooseClosestSkeletons(IEnumerable<Skeleton> skeletonDataValue, int count)
        {
            SortedList<float, int> depthSorted = new SortedList<float, int>();

            foreach (Skeleton s in skeletonDataValue)
            {
                if (s.TrackingState != SkeletonTrackingState.NotTracked)
                {
                    float valueZ = s.Position.Z;
                    while (depthSorted.ContainsKey(valueZ))
                    {
                        valueZ += 0.0001f;
                    }

                    depthSorted.Add(valueZ, s.TrackingId);
                }
            }

            this.ChooseSkeletonsFromList(depthSorted.Values, count);
        }

        private void ChooseOldestSkeletons(IEnumerable<Skeleton> skeletonDataValue, int count)
        {
            List<int> newList = new List<int>();
            
            foreach (Skeleton s in skeletonDataValue)
            {
                if (s.TrackingState != SkeletonTrackingState.NotTracked)
                {
                    newList.Add(s.TrackingId);
                }
            }

            // Remove all elements from the active list that are not currently present
            this.activeList.RemoveAll(k => !newList.Contains(k));

            // Add all elements that aren't already in the activeList
            this.activeList.AddRange(newList.FindAll(k => !this.activeList.Contains(k)));

            this.ChooseSkeletonsFromList(this.activeList, count);
        }

        private void ChooseMostActiveSkeletons(IEnumerable<Skeleton> skeletonDataValue, int count)
        {
            foreach (ActivityWatcher watcher in this.recentActivity)
            {
                watcher.NewPass();
            }

            foreach (Skeleton s in skeletonDataValue)
            {
                if (s.TrackingState != SkeletonTrackingState.NotTracked)
                {
                    ActivityWatcher watcher = this.recentActivity.Find(w => w.TrackingId == s.TrackingId);
                    if (watcher != null)
                    {
                        watcher.Update(s);
                    }
                    else
                    {
                        this.recentActivity.Add(new ActivityWatcher(s));
                    }
                }
            }

            // Remove any skeletons that are gone
            this.recentActivity.RemoveAll(aw => !aw.Updated);

            this.recentActivity.Sort();
            this.ChooseSkeletonsFromList(this.recentActivity.ConvertAll(f => f.TrackingId), count);
        }

        private void ChooseSkeletonsFromList(IList<int> list, int max)
        {
            if (this.Kinect.SkeletonStream.IsEnabled)
            {
                int argCount = Math.Min(list.Count, max);

                if (argCount == 0)
                {
                    this.Kinect.SkeletonStream.ChooseSkeletons();
                }

                if (argCount == 1)
                {
                    this.Kinect.SkeletonStream.ChooseSkeletons(list[0]);
                }

                if (argCount >= 2)
                {
                    this.Kinect.SkeletonStream.ChooseSkeletons(list[0], list[1]);
                }
            }
        }

        private class ActivityWatcher : IComparable<ActivityWatcher>
        {
            private float activityLevel;
            private SkeletonPoint previousPosition;
            private SkeletonPoint previousDelta;

            internal ActivityWatcher(Skeleton s)
            {
                this.activityLevel = 0.0f;
                this.TrackingId = s.TrackingId;
                this.Updated = true;
                this.previousPosition = s.Position;
                this.previousDelta = new SkeletonPoint();
            }

            internal int TrackingId { get; private set; }

            internal bool Updated { get; private set; }

            public int CompareTo(ActivityWatcher other)
            {
                // Use the existing CompareTo on float, but reverse the arguments,
                // since we wish to have larger activityLevels sort ahead of smaller values.
                return other.activityLevel.CompareTo(this.activityLevel);
            }

            internal void NewPass()
            {
                this.Updated = false;
            }

            internal void Update(Skeleton s)
            {
                SkeletonPoint newPosition = s.Position;
                SkeletonPoint newDelta = new SkeletonPoint
                    {
                        X = newPosition.X - this.previousPosition.X,
                        Y = newPosition.Y - this.previousPosition.Y,
                        Z = newPosition.Z - this.previousPosition.Z
                    };

                SkeletonPoint deltaV = new SkeletonPoint
                    {
                        X = newDelta.X - this.previousDelta.X,
                        Y = newDelta.Y - this.previousDelta.Y,
                        Z = newDelta.Z - this.previousDelta.Z
                    };

                this.previousPosition = newPosition;
                this.previousDelta = newDelta;

                float deltaVLengthSquared = (deltaV.X * deltaV.X) + (deltaV.Y * deltaV.Y) + (deltaV.Z * deltaV.Z);
                float deltaVLength = (float)Math.Sqrt(deltaVLengthSquared);

                this.activityLevel = this.activityLevel * ActivityFalloff;
                this.activityLevel += deltaVLength;

                this.Updated = true;
            }
        }
    }
}
