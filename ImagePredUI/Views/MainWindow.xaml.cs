using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ImagePredUI.ViewModels;


namespace ImagePredUI.Views
{   
    public class MainWindow : Window
    {
        MNISTModelVM modelVM;
        public MainWindow()
        {
            InitializeComponent();
            modelVM=new MNISTModelVM();
            
            this.DataContext=modelVM;
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        async void ChooseDirClick(object sender, RoutedEventArgs args)
        {
            var dialog=new OpenFolderDialog();
            dialog.Directory=Directory.GetCurrentDirectory();
            string result=await dialog.ShowAsync(this);
            TextBlock textBlock = this.FindControl<TextBlock>("TextBlockDir");
            if  (result!=null && result!="") 
            {
                textBlock.Text=result;
                try {await modelVM.PredImages(result);}
                catch(Exception)
                {textBlock.Text="Choose directory containing only images";}
                
            }
            else {textBlock.Text="Choose directory containing only images";}
        }
        void StopClick(object sender, RoutedEventArgs args)
        {
            modelVM.Source.Cancel();
        }
        void ShowAllClick(object sender, RoutedEventArgs args)
        {
            ListBox listBoxImages = this.FindControl<ListBox>("ListBoxImages");
            listBoxImages.Items = modelVM.ImageResults;
        }
        void ClassSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
           
            if (args.AddedItems.Count>0) 
            {
                ListBox listBoxImages = this.FindControl<ListBox>("ListBoxImages");
                ListBox listBoxClasses = this.FindControl<ListBox>("ListBoxClasses");
                listBoxImages.Items = modelVM.ImageClasses[listBoxClasses.SelectedIndex];
            }
        }
    }
}