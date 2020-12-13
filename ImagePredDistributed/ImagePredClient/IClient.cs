using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using ImagePredContracts;

namespace ImagePredClient {
    public interface IClient
    {
        ClassifiedImage SelectedImage {get; set;}
        ObservableCollection<ClassifiedImage> DbImages {get; set;}
        event PropertyChangedEventHandler PropertyChanged;
        Task GetImages();
        Task GetStats();  
        Task PutImage(string path);
        Task DeleteImages();
        
    }
}