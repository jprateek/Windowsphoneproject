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
    //This class is used to store album information: title, published time, href to album and cover thumbnail
    public class Myalbum
    {   
        public string title { get; set; }        
        public string published { get; set; }        
        public string href { get; set; }        
        public string thumbnail { get; set; }
    }

    
}
