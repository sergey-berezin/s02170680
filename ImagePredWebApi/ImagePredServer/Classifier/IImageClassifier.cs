using ImagePredContracts;

namespace ImagePredServer.Classifier
{ 
    public interface IImageClassifier
    {
        ClassifiedImage Classify(NewImage newImage);
    }
}