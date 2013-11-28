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
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Phone.Shell;

// Refered the API from https://developers.google.com/picasa-web/docs/2.0/developers_guide_protocol

namespace Assignment1
{
    public partial class PhotosofAlbum : PhoneApplicationPage
    {
        App app = App.Current as App;
       PhotoChooserTask photoChooserTask;
       int AlbumIndex = -1;
      
       int photoCount = 0;
        public delegate void UploadPhotoCallback(bool success, string message); 


        public PhotosofAlbum()
        {
            InitializeComponent();
            Loaded += PhotosofAlbum_Loaded;            
        }

        void PhotosofAlbum_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            CreateApplicationBar();
        }

        // Creating localized app bar
        private void CreateApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            //Creating the appbar menu Icon
            var appBarButtonUpload = new ApplicationBarIconButton(new Uri("/Images/appbar.upload.rest.png", UriKind.Relative)) 
                                         { Text = AppResources.upload };
            appBarButtonUpload.Click += Upload_Click;
            ApplicationBar.Buttons.Add(appBarButtonUpload);

            //Creating the appbar menu
            var appBarMenuUpload = new ApplicationBarMenuItem(AppResources.upload);
            var appBarMenuLogout = new ApplicationBarMenuItem(AppResources.logout);

            appBarMenuUpload.Click += Upload_Click;
            appBarMenuLogout.Click += logout_Click;
            
            ApplicationBar.MenuItems.Add(appBarMenuUpload);
            ApplicationBar.MenuItems.Add(appBarMenuLogout);
        }

        //setting the progress Indicator
        private static void SetProgessIndicator(bool isVisible)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
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

            if (parameters.ContainsKey("href"))
            {
                string href = (string)parameters["href"];
                
                GetImages(href);
            }

            
        }

        // Get the images for the selected album from google.
        private void GetImages(int selectedIndex)
        {
            AlbumIndex = selectedIndex;

            //Show progress Indicator
            SetProgessIndicator(true);
            
            //for debuging purpose
            // MessageBox.Show(App.auth);
            
            WebClient webClient = new WebClient();

            string auth = "GoogleLogin auth=" + App.auth;
            webClient.Headers[HttpRequestHeader.Authorization] = auth;
            Uri uri = new Uri(app.albums[selectedIndex].href, UriKind.Absolute);
            //for debuging purpose
           // MessageBox.Show(app.albums[selectedIndex].href);
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadAlbumImages);
            webClient.DownloadStringAsync(uri);
        }

        private void GetImages(string href)
        {
            WebClient webClient = new WebClient();
            string auth = "GoogleLogin auth=" + App.auth;
            webClient.Headers[HttpRequestHeader.Authorization] = auth;
            Uri uri = new Uri(href, UriKind.Absolute);
            //for debuging purpose
           // MessageBox.Show(app.albums[selectedIndex].href);
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadAlbumImages);
            webClient.DownloadStringAsync(uri);
        }

        // Code refered from http://developer.nokia.com/Community/Wiki/Picasa_Image_Gallery_with_JSON_in_Window_Phone
        // Refered the API from https://developers.google.com/picasa-web/docs/2.0/developers_guide_protocol
        public void DownloadAlbumImages(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Result == null || e.Error != null)
                {
                    MessageBox.Show(AppResources.errMsgPicasaServer1);
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
                        // Image id object
                        IDictionary<string, object> id = (IDictionary<string, object>)entry["id"];
                        // Get album title
                        albumImage.id = (string)id["$t"];
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
                    
                  
                    // Add albumImages to AlbumImagesListBox
                    listBoxPhotos.ItemsSource = app.albumImages;
                    
                    //Hide Progress Indicator
                    SetProgessIndicator(false);
                   
                    photoCount = entries.Count;
                    UpdateTileIfExist();
                    
                }
           }
            catch (WebException)
            {
                MessageBox.Show(AppResources.errMsgPicasaServer1);
                SetProgessIndicator(false);
            }
            catch (KeyNotFoundException)
            {
                MessageBox.Show(AppResources.errMsgJSONError1);
                SetProgessIndicator(false);
            }
        }


        //Call photochooser from appbar
        private void Upload_Click(object sender, EventArgs e)
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
                //MessageBox.Show(e.ChosenPhoto.Length.ToString());
                UploadNewPhoto(e.ChosenPhoto);
            }
        }

        private void UploadNewPhoto(Stream stream)
        {
            const int BLOCK_SIZE = 4096;
            string AuthToken = App.auth;

            SetProgessIndicator(true);
            SystemTray.ProgressIndicator.Text = AppResources.showupload;

            if (AlbumIndex != -1)
            {
                Uri uri = new Uri(app.albums[AlbumIndex].href, UriKind.Absolute);
                WebClient wc = new WebClient();

                wc.Headers[HttpRequestHeader.Authorization] = "GoogleLogin auth=" + AuthToken;
                wc.Headers[HttpRequestHeader.ContentLength] = stream.Length.ToString();
                wc.Headers[HttpRequestHeader.ContentType] = "image/jpeg";
                wc.AllowReadStreamBuffering = true;
                wc.AllowWriteStreamBuffering = true;


                wc.OpenWriteCompleted += (s, args) =>
                {
                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        using (BinaryWriter bw = new BinaryWriter(args.Result))
                        {
                            long bCount = 0;
                            long fileSize = stream.Length;
                            byte[] bytes = new byte[BLOCK_SIZE];
                            do
                            {
                                bytes = br.ReadBytes(BLOCK_SIZE);
                                bCount += bytes.Length;
                                bw.Write(bytes);
                            }
                            while (bCount < fileSize);
                        }
                    }
                };
                // Uploading is complete             
                wc.WriteStreamClosed += (s, args) =>
                {
                   // MessageBox.Show(AppResources.photouploaded);
                    SetProgessIndicator(false);
                    GetImages(AlbumIndex);
                  
                };


                wc.OpenWriteAsync(uri, "POST");

            }
        }
        
        private void Tile_Click(object sender, RoutedEventArgs e)
        {
            int index = -1;
            MyAlbumImage item = null; 
            if (sender is MenuItem) 
            { 
                ListBoxItem selectedListBoxItem = listBoxPhotos.ItemContainerGenerator.ContainerFromItem( 
                    (sender as MenuItem).DataContext) as ListBoxItem; 
                if (selectedListBoxItem != null) 
                { 
                    index = listBoxPhotos.ItemContainerGenerator.IndexFromContainer(selectedListBoxItem); 
                    if (index >= 0 && index < listBoxPhotos.Items.Count()) 
                    {
                        item = listBoxPhotos.Items[index] as MyAlbumImage;
                    } 
                } 
            }
            if (item != null)
            {
                string navigationUri = "/Photos.xaml?SelectedImage=" + item.content;
                
                ShellTile Tile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(navigationUri));
                if (Tile == null)
                {
                    // create a new secondary tile
                    StandardTileData data = new StandardTileData();
                    // tile foreground data
                    data.Title = AppResources.appName; 
                    
                  
                    data.BackgroundImage = new Uri(item.thumbnail, UriKind.Absolute);
                    data.BackBackgroundImage = new Uri(item.thumbnail, UriKind.Absolute);
                    data.BackTitle = AppResources.appName;
                    // create a new tile for this Second Page
                    ShellTile.Create(new Uri(navigationUri, UriKind.Relative), data);
                }
            }
        }
        private void delete_Click(object sender, RoutedEventArgs e)
        {
      
            int index = -1;
            MyAlbumImage item = null; 

            //Start progress Indicator
            SetProgessIndicator(true);
            SystemTray.ProgressIndicator.Text = AppResources.showdelete;

            if (sender is MenuItem) 
            { 
                ListBoxItem selectedListBoxItem = listBoxPhotos.ItemContainerGenerator.ContainerFromItem( 
                    (sender as MenuItem).DataContext) as ListBoxItem; 
                if (selectedListBoxItem != null) 
                { 
                    index = listBoxPhotos.ItemContainerGenerator.IndexFromContainer(selectedListBoxItem); 
                    if (index >= 0 && index < listBoxPhotos.Items.Count()) 
                    {
                        item = listBoxPhotos.Items[index] as MyAlbumImage;
                    } 
                } 
            } 
            if (item != null)       
            { 
                string id = item.id;
                int sindex = id.IndexOf("?alt");
                string sURL = id.Substring(0, sindex);
                sURL = sURL.Replace("entry", "media");
              
                string AuthToken = App.auth;
              
                Uri uri = new Uri(sURL, UriKind.Absolute);
                try
                {
                    var wc = new WebClient();
                    wc.Headers[HttpRequestHeader.Authorization] = "GoogleLogin auth=" + AuthToken;
                    wc.Headers[HttpRequestHeader.ContentLength] = "0";
                    wc.Headers[HttpRequestHeader.IfMatch] = "*";
                    wc.AllowReadStreamBuffering = false;
                    wc.AllowWriteStreamBuffering = false;
                    wc.UploadStringCompleted += new UploadStringCompletedEventHandler(wc_UploadStringCompleted);
                    wc.UploadStringAsync(uri,"DELETE", string.Empty);
                        
                }
                catch (Exception e2)
                {
                    MessageBox.Show(e2.Message);
                }
            }
        }

        void wc_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Result))
            {
              // MessageBox.Show(AppResources.deletephoto);

                //Stop progress Indicator
                SetProgessIndicator(false);

                GetImages(AlbumIndex);
               
            }
            else
            {
                MessageBox.Show(AppResources.deleteError);
            }
        }
        
        // Logout from the app
        private void logout_Click(object sender, EventArgs e)
        {
            App.auth = "";
            NavigationService.Navigate(new Uri("/MainPage.xaml?logout=1", UriKind.Relative));
        }

   
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
             //NavigationService.RemoveBackEntry();
            //base.OnNavigatedTo(e);
        }

        private void ApplicationBarMenuItem_Click_2(object sender, EventArgs e)
        {
            // check if secondary tile is already made and pinned
            if (AlbumIndex != -1)
            {
                string albumID = app.albums[AlbumIndex].title;
            }
            string navigationUri = "/PhotosofAlbum.xaml?SelectedIndex=" + app.albums[AlbumIndex].href;

            ShellTile Tile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(navigationUri));
            if (Tile == null) 
            {
                Random r = new Random();
                // create a new secondary tile
                StandardTileData data = new StandardTileData();
                // tile foreground data
                data.Title = app.albums[AlbumIndex].title;
                
                data.BackTitle = AppResources.appName;
                
                data.BackgroundImage = new Uri(app.albums[AlbumIndex].thumbnail.Replace("JPG", "jpg"), UriKind.Absolute);
                data.Count = photoCount;
                int index = r.Next(photoCount);
                data.BackBackgroundImage = new Uri(app.albumImages[index].thumbnail.Replace("JPG", "jpg"), UriKind.Absolute);
                
                // create a new tile for this Second Page
                ShellTile.Create(new Uri(navigationUri, UriKind.Relative), data);
            }
        }

        private void UpdateTileIfExist()
        {
            // check if secondary tile is already made and pinned
            if (AlbumIndex != -1)
            {
                string albumID = app.albums[AlbumIndex].title;
            }
            string navigationUri = "/PhotosofAlbum.xaml?SelectedIndex=" + app.albums[AlbumIndex].href;

            
            // Find the Tile we want to update.
            ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(navigationUri));

            // If the Tile was found, then update the background image.
            if (TileToFind != null)
            {
                Random r = new Random();
                int index = r.Next(photoCount);
                    
                StandardTileData NewTileData = new StandardTileData
                {
                    //BackContent = photoCount + " photos in this album",
                    Count = photoCount,
                    BackBackgroundImage = new Uri(app.albumImages[index].thumbnail.Replace("JPG", "jpg"), UriKind.Absolute)
                };

                TileToFind.Update(NewTileData);
            }
        }
    }
}