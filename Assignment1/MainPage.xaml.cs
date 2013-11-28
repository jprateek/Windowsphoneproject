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
using System.Text.RegularExpressions;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;


namespace Assignment1
{
    public partial class MainPage : PhoneApplicationPage
    {

        private const string emailKey = "email";
        private const string pwdKey = "pwd";
        private const string isSaveKey = "Savekey";

        private IsolatedStorageSettings appSettings;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;

            appSettings = IsolatedStorageSettings.ApplicationSettings;
        }


        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            CreateApplicationBar();
        }

        // Creating the localized app bar
        private void CreateApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            
            var appBarMenuAbout = new ApplicationBarMenuItem(AppResources.aboutapp);
            appBarMenuAbout.Click += appBar_Click;
            ApplicationBar.MenuItems.Add(appBarMenuAbout);
        }



        //Checking if user has entered a valid emailid.
        private bool IsValidEmail(String txtEmail)
        {
            return (Regex.IsMatch(txtEmail,
              @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
              @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$"));
 
        }
        private void AuthDownloaded(object sender, DownloadStringCompletedEventArgs e)
        {	
            try	
                {		
                    if (e.Result != null && e.Error == null)		
                        {			
                            App.auth = "";
                            int index = e.Result.IndexOf("Auth=");
                            
                            if (index != -1)
                            {
                                App.auth = e.Result.Substring(index + 5);
                            }
                        
                            if (App.auth != "")
                            {
                               // MessageBox.Show("Auth Successful");
                                this.NavigationService.Navigate(new Uri("/AlbumsPage.xaml", UriKind.Relative));
                                return;
                            }   
                        }
                    MessageBox.Show(AppResources.errMsgAuthFail);
                }	
            catch (WebException)
            {
                MessageBox.Show(AppResources.errMsgAuthFail);	
            }
        }

        private void GetAuth()
        {
            string service = "lh2"; // Picasa	
            string accountType = "GOOGLE"; WebClient webClient = new WebClient();
            Uri uri = new Uri(string.Format("https://www.google.com/accounts/ClientLogin?Email={0}&Passwd={1}&service={2}&accountType={3}", App.username, App.password, service, accountType)); 
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(AuthDownloaded); 
            webClient.DownloadStringAsync(uri);
        }



        private void btnLogon_Click(object sender, RoutedEventArgs e)
        {
            // Getting the username, pwd and does user wants to save his credentials
            App.username = txtEmail.Text;
            App.password = passwordBox1.Password;
            App.isSaveDetails = (bool)chkSave.IsChecked;
            
            //Check if the txtEmail or pwdbox1 is empty
            if (String.IsNullOrWhiteSpace(txtEmail.Text) || String.IsNullOrWhiteSpace(passwordBox1.Password))
            {
                
                MessageBox.Show(AppResources.msgInvalidUser);

            }
            else
            {
                
            
                if (IsValidEmail(txtEmail.Text) == true)
                {
                  
                                                         
                    // Authenticating the user details with google server 
                    GetAuth();
                               
                }
                else
                    MessageBox.Show(AppResources.msgInvalidemail);
                
            }
            
            
           
            
        }

        private void appBar_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/about.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            
            if (App.isSaveDetails == true)
            {
                // Saving logon details to the isolated storage.
                if (appSettings.Contains(emailKey))
                    appSettings[emailKey] = txtEmail.Text;
                else
                    appSettings.Add(emailKey, App.username);

                if (appSettings.Contains(pwdKey))
                    appSettings[pwdKey] = passwordBox1.Password;
                else
                    appSettings.Add(pwdKey, App.password);

                if (appSettings.Contains(isSaveKey))
                    appSettings[isSaveKey] = chkSave.IsChecked;
                else
                    appSettings.Add(isSaveKey, "true");
            }
           
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //NavigationService.RemoveBackEntry();
            base.OnNavigatedTo(e);
            string isLogout;
            if (NavigationContext.QueryString.TryGetValue("logout", out isLogout))
            {
                while (NavigationService.CanGoBack)
                    NavigationService.RemoveBackEntry();
                NavigationContext.QueryString.Clear();
            }

            if (appSettings.Contains(emailKey))
            {
                txtEmail.Text = (string)appSettings[emailKey];
                passwordBox1.Password = (string)appSettings[pwdKey];
              //  if( (string)appSettings[isSaveKey] == "true" )
                    chkSave.IsChecked = true;
            }

            UpdateTile();
            
        }

		// modify Application Tile data
		private void UpdateTile()
		{	
			// get application tile
			ShellTile tile = ShellTile.ActiveTiles.First();	
			if (null != tile)	
			{		
                // create a new data for tile		
                StandardTileData data = new StandardTileData();		
                // tile foreground data		
                data.Title = AppResources.appName;		
                data.BackgroundImage = new Uri("/Images/reel7.jpg", UriKind.Relative);		
                data.Count = 0;		
                // to make tile flip add data to background also		
                data.BackTitle = AppResources.appName;
                data.BackContent = AppResources.TileNotification;		
                // update tile		
                tile.Update(data);	
            }
        }

		
    }
}