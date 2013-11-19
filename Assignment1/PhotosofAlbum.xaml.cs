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
using System.Collections;
using Microsoft.Phone.Tasks;

namespace Assignment1
{
    public partial class PhotosofAlbum : PhoneApplicationPage
    {
        App app = App.Current as App;
        CameraCaptureTask cameraCaptureTask;
        PhotoChooserTask photoChooserTask;

        public PhotosofAlbum()
        {
            InitializeComponent();
            
        }

        private void listBoxPhotos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If real selection is happened, go to a ImagesPage	
            if (listBoxPhotos.SelectedIndex == -1) 
                return;

            this.NavigationService.Navigate(new Uri("/Photos.xaml?SelectedIndex=" + listBoxPhotos.SelectedIndex + "&SelectedAlbum=" + PhotosofAlbmTitle.Text, UriKind.Relative));
         }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e); 	
            
            // While navigating back from Pictures page
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)	
            {
                listBoxPhotos.ItemsSource = app.albumImages;
                PhotosofAlbmTitle.Text = app.albums[app.selectedAlbumIndex].title;
                listBoxPhotos.SelectedIndex = -1;		
                return;	}
            
            // While navigating from AlbumsPage, start loading images	
            IDictionary<string, string> parameters = this.NavigationContext.QueryString;
            if (parameters.ContainsKey("SelectedIndex"))
            {
                int selectedIndex = Int32.Parse(parameters["SelectedIndex"]);
                PhotosofAlbmTitle.Text = app.albums[selectedIndex].title;
                GetImages(selectedIndex);
            }
            
           
        }

        // Get the images for the selected album from google.
        private void GetImages(int selectedIndex)
        {
            // Show loading... animation
           // ShowProgress = true;
            WebClient webClient = new WebClient();
            string auth = "GoogleLogin auth=" + App.auth;
            webClient.Headers[HttpRequestHeader.Authorization] = auth;
            Uri uri = new Uri(app.albums[selectedIndex].href, UriKind.Absolute);
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadAlbumImages);
            webClient.DownloadStringAsync(uri);
        }


        public void DownloadAlbumImages(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Result == null || e.Error != null)
                {
                    MessageBox.Show(AppResources.errMsgPicasaServer1);
                   // ShowProgress = false;
                    return;
                }
                else
                {
                    // Deserialize JSON string to dynamic object
                    IDictionary<string, object> json = (IDictionary<string, object>)SimpleJson.DeserializeObject(e.Result);
                    // Feed object
                    IDictionary<string, object> feed = (IDictionary<string, object>)json["feed"];
                    // Number of photos object
                    IDictionary<string, object> numberOfPhotos = (IDictionary<string, object>)feed["gphoto$numphotos"];
                    // Entries List
                    var entries = (IList)feed["entry"];
                    // clear previous images from albumImages
                    app.albumImages.Clear();
                    // Find image details from entries
                    for (int i = 0; i < entries.Count; i++)
                    {
                        // Create a new albumImage
                        MyAlbumImage albumImage = new MyAlbumImage();
                        // Image entry object
                        IDictionary<string, object> entry = (IDictionary<string, object>)entries[i];
                        // Image title object
                        IDictionary<string, object> title = (IDictionary<string, object>)entry["title"];
                        // Get album title
                        albumImage.title = (string)title["$t"];
                        // Album content object
                        IDictionary<string, object> content = (IDictionary<string, object>)entry["content"];
                        // Get image src url
                        albumImage.content = (string)content["src"];
                        // Image width object
                        IDictionary<string, object> width = (IDictionary<string, object>)entry["gphoto$width"];
                        // Get image width
                        albumImage.width = (string)width["$t"];
                        // Image height object
                        IDictionary<string, object> height = (IDictionary<string, object>)entry["gphoto$height"];
                        // Get image height
                        albumImage.height = (string)height["$t"];
                        // Image size object
                        IDictionary<string, object> size = (IDictionary<string, object>)entry["gphoto$size"];
                        // Get image size 
                        albumImage.size = (string)size["$t"];
                        // Image media group List
                        IDictionary<string, object> mediaGroup = (IDictionary<string, object>)entry["media$group"];
                        IList mediaThumbnailList = (IList)mediaGroup["media$thumbnail"];
                        // First thumbnail object
                        IDictionary<string, object> mediathumbnail = (IDictionary<string, object>)mediaThumbnailList[0];
                        // Get thumnail url
                        albumImage.thumbnail = (string)mediathumbnail["url"];
                        // Add albumImage to albumImages Collection
                        app.albumImages.Add(albumImage);
                    }
                    // Hide loading... animation
                   // ShowProgress = false;
                    // Add albumImages to AlbumImagesListBox
                    listBoxPhotos.ItemsSource = app.albumImages;
                }
            }
            catch (WebException)
            {
                MessageBox.Show(AppResources.errMsgPicasaServer1);
               // ShowProgress = false;
            }
            catch (KeyNotFoundException)
            {
                MessageBox.Show(AppResources.errMsgJSONError1);
               // ShowProgress = false;
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            photoChooser();
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            photoChooser();
        }

        //Initiating the Photochooser task and showing the picture picker.
        void photoChooser()
        {
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.ShowCamera = true;
            photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
            photoChooserTask.Show();
        }
       
        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                MessageBox.Show(e.ChosenPhoto.Length.ToString());

                //Code to display the photo on the page in an image control named myImage.
                //System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                //bmp.SetSource(e.ChosenPhoto);
                //myImage.Source = bmp;
            }
        }

    }
}