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


namespace Assignment1
{
    public partial class MainPage : PhoneApplicationPage
    {

        String strProvider;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
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
            Uri uri = new Uri(string.Format("https://www.google.com/accounts/ClientLogin?Email={0}&Passwd={1}&service={2}&accountType={3}", txtEmail.Text, passwordBox1.Password, service, accountType)); 
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(AuthDownloaded); 
            webClient.DownloadStringAsync(uri);
        }



        private void btnLogon_Click(object sender, RoutedEventArgs e)
        {
            App.username = txtEmail.Text;
            
            //Check if the txtEmail or pwdbox1 is empty
            if (String.IsNullOrWhiteSpace(txtEmail.Text) || String.IsNullOrWhiteSpace(passwordBox1.Password))
            {
                
                MessageBox.Show(AppResources.msgInvalidUser);

            }
            else
            {
                
            
                if (IsValidEmail(txtEmail.Text) == true)
                {
                  

                    if (radioFlickr.IsChecked == true)
                        strProvider = "Flickr";
                    else
                        strProvider = "Picasa";
                    
                    // Authenticating the user details with google server 
                    GetAuth();
            //        MessageBox.Show(AppResources.msgLoginSuccess +  "  " + strProvider);
                   
                }
                else
                    MessageBox.Show(AppResources.msgInvalidemail);
                
            }
            
            
           
            
        }

        private void appBar_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/about.xaml", UriKind.Relative));
        }

       
        

       

        
    }
}