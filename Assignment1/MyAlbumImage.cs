using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Assignment1
{
    //This class is used to store image information like image title, content url, size and thumbnail url
    public class MyAlbumImage
    {
        public string title { get; set; }        
        public string content { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string size { get; set; }
        public string thumbnail { get; set; }
    }
}
