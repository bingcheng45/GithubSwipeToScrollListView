//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Wpf.Controls;
    using Microsoft.Samples.Kinect.ControlsBasics.DataModel;
    using System.Windows.Input;
    using PixelMirror;
    using System.Collections.Generic;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow
    {

        //Kinect code start

        /// <summary> Active Kinect sensor </summary>
        private KinectSensor kinectSensor = null;

        /// <summary> Array for the bodies (Kinect will track up to 6 people simultaneously) </summary>
        private Body[] bodies = null;

        /// <summary> Reader for body frames </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary> Current status text to display </summary>
        private string statusText = null;

        /// <summary> KinectBodyView object which handles drawing the Kinect bodies to a View box in the UI </summary>
        private KinectBodyView kinectBodyView = null;

        /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6) </summary>
        private List<GestureDetector> gestureDetectorList = null;

        internal static MainWindow main;





        //Kinect code end



        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class. 
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            KinectRegion.SetKinectRegion(this, kinectRegion);

            App app = ((App)Application.Current);
            app.KinectRegion = kinectRegion;

            // Use the default sensor
            this.kinectRegion.KinectSensor = KinectSensor.GetDefault();
            //kinect code start

            // only one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();


            // open the sensor
            this.kinectSensor.Open();

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // set the BodyFramedArrived event notifier
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;

            // initialize the BodyViewer object for displaying tracked bodies in the UI
            this.kinectBodyView = new KinectBodyView(this.kinectSensor);

            // initialize the gesture detection objects for our gestures
            this.gestureDetectorList = new List<GestureDetector>();

            // set our data context objects for display in UI
            this.DataContext = this;
            this.kinectBodyViewbox.DataContext = this.kinectBodyView;

            // create a gesture detector for each body (6 bodies => 6 detectors) and create content controls to display results in the UI
            int col0Row = 0;
            int col1Row = 0;
            int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
            for (int i = 0; i < maxBodies; ++i)
            {
                GestureResultView result = new GestureResultView(i, false, false, 0.0f);
                GestureDetector detector = new GestureDetector(this.kinectSensor, result);
                this.gestureDetectorList.Add(detector);

                // split gesture results across the first two columns of the content grid
                ContentControl contentControl = new ContentControl();
                contentControl.Content = this.gestureDetectorList[i].GestureResultView;

                if (i % 2 == 0)
                {
                    // Gesture results for bodies: 0, 2, 4
                    Grid.SetColumn(contentControl, 0);
                    Grid.SetRow(contentControl, col0Row);
                    ++col0Row;
                }
                else
                {
                    // Gesture results for bodies: 1, 3, 5
                    Grid.SetColumn(contentControl, 1);
                    Grid.SetRow(contentControl, col1Row);
                    ++col1Row;
                }

                //this.contentGrid.Children.Add(contentControl);
            }



            //kinect code end
        }

        /// <summary>
        /// Handle a button click from the wrap panel.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void ButtonClick(object sender, RoutedEventArgs e)
        {            
            var button = (Button)e.OriginalSource;
            SampleDataItem sampleDataItem = button.DataContext as SampleDataItem;

            
        }
        //kinect code start

        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        // creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                // visualize the new body data
                this.kinectBodyView.UpdateBodyFrame(this.bodies);

                // we may have lost/acquired bodies, so update the corresponding gesture detectors
                if (this.bodies != null)
                {
                    // loop through all bodies to see if any of the gesture detectors need to be updated
                    int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
                    for (int i = 0; i < maxBodies; ++i)
                    {
                        Body body = this.bodies[i];
                        ulong trackingId = body.TrackingId;

                        // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                        if (trackingId != this.gestureDetectorList[i].TrackingId)
                        {
                            this.gestureDetectorList[i].TrackingId = trackingId;

                            // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                            // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                            this.gestureDetectorList[i].IsPaused = trackingId == 0;
                        }
                    }
                }
            }
        }

        //kinect code end
        /// <summary>
        /// Handle the back button click.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void GoBack(object sender, RoutedEventArgs e)
        {
        }
        DelayedActionCommandButton scrollbutton = new DelayedActionCommandButton();

        private void btnmiddle_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.scrollbutton.DelayedActionCommandButton_TouchDown(sender,e);
        }

        private void btnmiddle_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.scrollbutton.DelayedActionCommandButton_TouchUp(sender, e);
            
        }

        private void RepeatButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(printer5);
        }

        public void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(printer5);
        }
    }
}
