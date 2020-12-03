
using ImagePredContracts;

namespace ImagePredServer.Database 
{
    public interface IImageDb 
    {
        ClassifiedImage[] GetImages();
        int[] GetStats();
        ClassifiedImage FindImage(NewImage newImage);
        ClassifiedImage PutImage(ClassifiedImage classifiedImage);
        void DeleteImages();
    }
}