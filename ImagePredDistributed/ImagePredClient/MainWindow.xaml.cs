using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;


namespace ImagePredClient
{
    public class MainWindow : Window
    {
        private IClient client;
        public MainWindow()
        {
            InitializeComponent();
            client=new Client();
            this.DataContext=client;
            Image image = this.FindControl<Image>("SelectedImage");
            image.Source=new Avalonia.Media.Imaging.Bitmap("Assets/avalonia-logo.ico");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        async void ChooseImageClick(object sender, RoutedEventArgs args)
        {
            DisplayErrorMessage("");
            var dialog=new OpenFileDialog();
            dialog.Directory=Directory.GetCurrentDirectory();
            string[] result =await dialog.ShowAsync(this);
            TextBlock textBlockImage = this.FindControl<TextBlock>("TextBlockImage");
            if  (result!=null && result.Length!=0) 
            {
                textBlockImage.Text=result[0];
                try {await client.PutImage(result[0]);}
                catch(Exception exc)
                {DisplayErrorMessage(exc.Message);}
                
            }
            else {textBlockImage.Text="Choose image to classify";}
        }
        async void GetImagesClick(object sender, RoutedEventArgs args)
        {
            DisplayErrorMessage("");
            try 
            {
                await client.GetImages();
                await client.GetStats();
            }
            catch(Exception exc)
            {
                DisplayErrorMessage(exc.Message);
            }
        }
        async void DeleteClick(object sender, RoutedEventArgs args)
        {
            DisplayErrorMessage("");
            try {await client.DeleteImages();}
            catch(Exception exc)
            {
                DisplayErrorMessage(exc.Message);
            }
        }
        private void DisplayErrorMessage(string message)
        {
            TextBox textBoxError = this.FindControl<TextBox>("TextBoxError");
            textBoxError.Text=message;
        }

    }
}