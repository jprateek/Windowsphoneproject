using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;

namespace Assignment1
{
    public partial class Photos : PhoneApplicationPage
    {
        private App app = App.Current as App;

        //Image herf in case of direct call from live tile
        private string imageHref = string.Empty;

        // Selected album title
        private string selectedAlbumTitle;

        // BitmapImage for loading Images from Google
        private BitmapImage bitmapImage;

        // GestureListener from ToolKit
        private GestureListener gestureListener;

        double initialScale = 1d;

        public Photos()
        {
            InitializeComponent();

            // Initialize GestureListener
            gestureListener = GestureService.GetGestureListener(ContentPanel);
            // Handle Dragging (to show next or previous image from Album)
            gestureListener.DragCompleted += new EventHandler<DragCompletedGestureEventArgs>(gestureListener_DragCompleted);
            // app.selectedImageIndex = 0;
        }

        // Navigated to Photos page
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Find selected image index from parameters
            IDictionary<string, string> parameters = this.NavigationContext.QueryString;

            // Find selected album name
            if (parameters.ContainsKey("SelectedAlbum"))
            {
                selectedAlbumTitle = parameters["SelectedAlbum"];
            }
            else
            {
                selectedAlbumTitle = "No album name";
            }

            if (parameters.ContainsKey("SelectedIndex"))
            {
                app.selectedImageIndex = Int32.Parse(parameters["SelectedIndex"]);
                LoadImage();
            }
            else
            {
                app.selectedImageIndex = 0;
            }

            // Find selected album name
            if (parameters.ContainsKey("SelectedImage"))
            {
                imageHref = parameters["SelectedImage"];
                LoadImage(imageHref);
                //Disable gesture handler
                //gestureListener.DragCompleted -= new EventHandler<DragCompletedGestureEventArgs>(gestureListener_DragCompleted);
            }

            // Load image from Google
            
        }

        private void LoadImage()
        {
            // Load a new image
            bitmapImage = new BitmapImage(new Uri(app.albumImages[app.selectedImageIndex].content, UriKind.RelativeOrAbsolute));
            // Handle loading (hide Loading... animation)
            bitmapImage.DownloadProgress += new EventHandler<DownloadProgressEventArgs>(bitmapImage_DownloadProgress);
            // Loaded Image is image source in XAML
            imgPhotos.Source = bitmapImage;
        }

        private void LoadImage(string href)
        {
            // Show loading... animation
            // ShowProgress = true;
            // Load a new image
            bitmapImage = new BitmapImage(new Uri(href, UriKind.RelativeOrAbsolute));
            // Handle loading (hide Loading... animation)
            bitmapImage.DownloadProgress += new EventHandler<DownloadProgressEventArgs>(bitmapImage_DownloadProgress);
            // Loaded Image is image source in XAML
            imgPhotos.Source = bitmapImage;
        }

        // Image is loaded from Google
        void bitmapImage_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            // Hide loading... animation
            //ShowProgress = false;
            // Disable LoadingListener for this image
            bitmapImage.DownloadProgress -= new EventHandler<DownloadProgressEventArgs>(bitmapImage_DownloadProgress);
           
        }

        // Gesture - Drag is complete
        void gestureListener_DragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            // Left or Right
            if (e.HorizontalChange > 0)
            {
                // previous image (or last if first is shown)
                app.selectedImageIndex--;
                if (app.selectedImageIndex < 0)
                {
                    app.selectedImageIndex = app.albumImages.Count - 1;
                }
            }
            else
            {
                // next image (or first if last is shown)
                app.selectedImageIndex++;
                if (app.selectedImageIndex > (app.albumImages.Count - 1))
                {
                    app.selectedImageIndex = 0;
                }
            }
            // Load image from Google
            LoadImage();
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/capture.xaml", UriKind.Relative));
        }

        private void OnPinchStarted(object s, PinchStartedGestureEventArgs e)
        {
            initialScale = ((CompositeTransform)imgPhotos.RenderTransform).ScaleX;
        }

        private void OnPinchDelta(object s, PinchGestureEventArgs e)
        {
            var finger1 = e.GetPosition(imgPhotos, 0);
            var finger2 = e.GetPosition(imgPhotos, 1);

            var center = new Point(
                (finger2.X + finger1.X) / 2 / imgPhotos.ActualWidth,
                (finger2.Y + finger1.Y) / 2 / imgPhotos.ActualHeight);

            imgPhotos.RenderTransformOrigin = center;

            var transform = (CompositeTransform)imgPhotos.RenderTransform;
            transform.ScaleX = initialScale * e.DistanceRatio;
            transform.ScaleY = transform.ScaleX;
        }


    }
}