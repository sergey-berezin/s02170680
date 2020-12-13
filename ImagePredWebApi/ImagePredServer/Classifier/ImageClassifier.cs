using System;
using ImagePredContracts;
using MNISTModelLib;

namespace ImagePredServer.Classifier
{ 
    public class ImageClassifier: IImageClassifier
    {
        MNISTModel model;
        public ImageClassifier()
        {
            model=new MNISTModel(@"..\\..\\MNISTModelLib\\mnist-8.onnx");
        }
        
        public ClassifiedImage Classify(NewImage newImage)
        {
            MNISTModelResult result=model.PredImage(Convert.FromBase64String(newImage.ImageBase64));
            return new ClassifiedImage()
            {
                Name=newImage.Name, Class=result.Class, Confidence=result.Confidence,
                RetrieveCount=0, ImageBase64=newImage.ImageBase64
            };

        } 
    }
}