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
using Microsoft.Phone.Shell;


// Refered the API from https://developers.google.com/picasa-web/docs/2.0/developers_guide_protocol

namespace Assignment1
{
    
    public partial class AlbumsPage : PhoneApplicationPage
    {
        private App objApp = App.Current as App;
        
        public AlbumsPage()
        {
            InitializeComponent();
            Loaded += AlbumsPage_Loaded;
        }

        void AlbumsPage_Loaded(object sender, RoutedEventArgs e)
        {
            CreateApplicationBar();
        }

        // Creating localized app bar
        private void CreateApplicationBar()
        {
            ApplicationBar = new ApplicationBar();


            var appBarMenuAbout = new ApplicationBarMenuItem(AppResources.aboutapp);
            var appBarMenuLogout = new ApplicationBarMenuItem(AppResources.logout);
            appBarMenuAbout.Click += about_Click;
            appBarMenuLogout.Click += logout_Click;
            ApplicationBar.MenuItems.Add(appBarMenuAbout);
            ApplicationBar.MenuItems.Add(appBarMenuLogout);
        }


        private void listBoxAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxAlbums.SelectedIndex == -1) return;
            objApp.selectedAlbumIndex = listBoxAlbums.SelectedIndex;
            this.NavigationService.Navigate(new Uri("/PhotosofAlbum.xaml?SelectedIndex=" + listBoxAlbums.SelectedIndex, UriKind.Relative));
            
        }

        private void GetAlbums()
        {
            string dataFeed = String.Format("http://picasaweb.google.com/data/feed/api/user/{0}?alt=json", App.username);
            WebClient webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = "GoogleLogin auth=" + App.auth;
            Uri uri = new Uri(dataFeed, UriKind.Absolute);
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadAlbums);
            webClient.DownloadStringAsync(uri);
        }
        

        // Get on the Albums Page and removing the back entry for logon page.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            NavigationService.RemoveBackEntry();
            base.OnNavigatedTo(e);

            // Sending the request to populate the albums.
            GetAlbums();
        }

        // Code refered from http://developer.nokia.com/Community/Wiki/Picasa_Image_Gallery_with_JSON_in_Window_Phone
        public void DownloadAlbums(object sender, DownloadStringCompletedEventArgs e)
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
                     //Deserialize JSON string to dynamic object
			        IDictionary<string, object> json = (IDictionary<string, object>)SimpleJson.DeserializeObject(e.Result);
			        // Feed object
			        IDictionary<string, object> feed = (IDictionary<string, object>)json["feed"];
			        // Author List
			        IList author = (IList)feed["author"];
			        // First author (and only)
			        IDictionary<string, object> firstAuthor = (IDictionary<string, object>)author[0];
			        // Album entries
			        IList entries = (IList)feed["entry"];
			        // Delete previous albums from albums list
                    objApp.albums.Clear(); 
                    //MessageBox.Show("data is ready");

                    // Find album details
                    for (int i = 0; i < entries.Count; i++)
                    {
                        // Create a new Album
                        Myalbum album = new Myalbum();
                        // Album entry object
                        IDictionary<string, object> entry = (IDictionary<string, object>)entries[i];
                        // Published object
                        IDictionary<string, object> published = (IDictionary<string, object>)entry["published"];
                        // Get published date
                        album.published = (string)published["$t"];
                        // Title object
                        IDictionary<string, object> title = (IDictionary<string, object>)entry["title"];
                        // Album title
                        album.title = (string)title["$t"];
                        // Link List
                        IList link = (IList)entry["link"];
                        // First link is album data link object
                        IDictionary<string, object> href = (IDictionary<string, object>)link[0];
                        // Get album data addres
                        album.href = (string)href["href"];
                        // Media group object
                        IDictionary<string, object> mediagroup = (IDictionary<string, object>)entry["media$group"];
                        // Media thumbnail object list
                        IList mediathumbnailList = (IList)mediagroup["media$thumbnail"];
                        // First thumbnail object (smallest)
                        var mediathumbnail = (IDictionary<string, object>)mediathumbnailList[0];
                        // Get thumbnail url
                        album.thumbnail = (string)mediathumbnail["url"];
                        // Add album to albums
                        objApp.albums.Add(album);
                        
                    }

                    listBoxAlbums.ItemsSource = objApp.albums;
                    UpdateLiveTileWithCountAndImage();
                }
            }
            catch (WebException)
            {
                MessageBox.Show(AppResources.errMsgPicasaServer1);
            }
            catch (KeyNotFoundException)
            {
                MessageBox.Show(AppResources.errMsgJSONError1);
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            //this.NavigationService.Navigate(new Uri("/capture.xaml", UriKind.Relative));
        }

        // Logout of the app
        private void logout_Click(object sender, EventArgs e)
        {
            App.auth = "";
            NavigationService.Navigate(new Uri("/MainPage.xaml?logout=1", UriKind.Relative));

        }


        //Calling the about page
        private void about_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/about.xaml", UriKind.Relative));
        }

        private void UpdateLiveTileWithCountAndImage()
        {
            // get application tile
            ShellTile tile = ShellTile.ActiveTiles.First();
            if (null != tile)
            {
                // create a new data for tile		
                StandardTileData data = new StandardTileData();
                // tile foreground data		
                data.Count = objApp.albums.Count;
                Random r = new Random();
                int index = r.Next(objApp.albums.Count);
                data.BackgroundImage = new Uri(objApp.albums[index].thumbnail, UriKind.Absolute);
                index = r.Next(objApp.albums.Count);
                data.BackBackgroundImage = new Uri(objApp.albums[index].thumbnail, UriKind.Absolute);
                // update tile		
                tile.Update(data);
            }
        }


    }
}