using ImagePredContracts;
using System.IO;
using System.Collections.ObjectModel;
using System.Drawing;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.ComponentModel;
using Avalonia.Threading;

namespace ImagePredClient
{
    class Client: INotifyPropertyChanged, IClient
    {
        private ClassifiedImage selectedImage;
        public ClassifiedImage SelectedImage {
            get {return selectedImage;} 
            set 
            {
                selectedImage=value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedImage)));
            }
        }
        private ObservableCollection<ClassifiedImage> dbImages;
        public ObservableCollection<ClassifiedImage> DbImages
        {
            get {return dbImages;}
            set 
            {
                dbImages=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DbImages)));
            }
        }   
        private ObservableCollection<string> stats;
        public ObservableCollection<string> Stats 
        {   
            get {return stats;}
            set
            {
                stats=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Stats)));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public Client()
        {
         
            SelectedImage=null;
            DbImages=new ObservableCollection<ClassifiedImage>();
            Stats=new ObservableCollection<string>();
        }
        public async Task GetImages()
        {
            await Task.Run(()=>
            {
                HttpClient client=new HttpClient();
                string result = client.GetStringAsync(Urls.ClassifiedImages).Result;
                var classifiedImages=JsonConvert.DeserializeObject<ClassifiedImage[]>(result);
                Dispatcher.UIThread.InvokeAsync(() => 
                {
                    DbImages=new ObservableCollection<ClassifiedImage>(classifiedImages);
                });
            });
        }
        public async Task GetStats()
        {
            await Task.Run(()=>
            {
                HttpClient client=new HttpClient();
                string result = client.GetStringAsync(Urls.Stats).Result;
                var stats=JsonConvert.DeserializeObject<int[]>(result);
                Dispatcher.UIThread.InvokeAsync(() => 
                {
                    Stats=new ObservableCollection<string>();
                    for (int i=0; i<stats.Length; i++)
                    {
                        Stats.Add($"|[{i}]|={stats[i]}");
                    }
                });
            });
        }
        public async Task PutImage(string path)
        {
            string imageBase64=ImageToBase64(new Bitmap(path));
            SelectedImage=new ClassifiedImage()
            {
                Id=-1, Name=Path.GetFileName(path), 
                Class=-1, Confidence=-1, RetrieveCount=-1, 
                ImageBase64=imageBase64
            };
            await Task.Run(()=>
            {
                HttpClient httpClient=new HttpClient();
                NewImage newImage=new NewImage(){Name=Path.GetFileName(path), 
                    ImageBase64=imageBase64};
                string json=JsonConvert.SerializeObject(newImage);
                var content=new StringContent(json);
                content.Headers.ContentType=new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var putResult=httpClient.PutAsync(Urls.ClassifiedImages, content).Result;
                json=putResult.Content.ReadAsStringAsync().Result;
                Dispatcher.UIThread.InvokeAsync(() => 
                {
                    SelectedImage=JsonConvert.DeserializeObject<ClassifiedImage>(json);
                });
            });
        }
        public async Task DeleteImages()
        {
            await Task.Run(()=>
            {
                HttpClient client=new HttpClient();
                var result=client.DeleteAsync(Urls.ClassifiedImages).Result;
            });
        }
        private string ImageToBase64(Image image)
        {
            MemoryStream stream=new MemoryStream();
            image.Save(stream, image.RawFormat);
            return  Convert.ToBase64String(stream.ToArray());
        }
    }
}