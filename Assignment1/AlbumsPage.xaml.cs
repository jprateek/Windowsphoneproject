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

namespace Assignment1
{
    
    public partial class AlbumsPage : PhoneApplicationPage
    {
        private App objApp = App.Current as App;
        //string albumName = "Album2";
        public AlbumsPage()
        {
            InitializeComponent();
        }

        private void listBoxAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxAlbums.SelectedIndex == -1) return;
            objApp.selectedAlbumIndex = listBoxAlbums.SelectedIndex;
            this.NavigationService.Navigate(new Uri("/PhotosofAlbum.xaml?SelectedIndex=" + listBoxAlbums.SelectedIndex, UriKind.Relative));
            //NavigationService.Navigate(new Uri("/PhotosofAlbum.xaml?AlbumName=" + albumName, UriKind.Relative));

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
        

        // Get on the Albums Page and removeing the back entry for logon page.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            NavigationService.RemoveBackEntry();
            base.OnNavigatedTo(e);

            // Sending the request to populate the albums.
            GetAlbums();
        }

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
            this.NavigationService.Navigate(new Uri("/capture.xaml", UriKind.Relative));
        }


    }
}