using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Wabbajack.GTK
{
    class MainWindow : Window
    {
        [UI] private FlowBox galleryFlow = null;

        private int _counter;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetObject("MainWindow").Handle)
        {
            builder.Autoconnect(this);
            var image = new Image();
            var image2 = new Image();
            var image3 = new Image();
            var image4 = new Image();
            var image5 = new Image();
            galleryFlow.Insert(image, 0);
            galleryFlow.Insert(image2, 1);
            galleryFlow.Insert(image3, 2);
            galleryFlow.Insert(image4, 3);
            galleryFlow.Insert(image5, 4);

            DeleteEvent += Window_DeleteEvent;
            // _button1.Clicked += Button1_Clicked;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        // private void Button1_Clicked(object sender, EventArgs a)
        // {
        //     _counter++;
        //     _label1.Text = "Hello World! This button has been clicked " + _counter + " time(s).";
        // }
    }
}
