using MNISTModelLib;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Avalonia.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ImagePredUI {  
    class MNISTModelVM 
    {
        public ObservableCollection<MNISTModelResult> ImageResults 
        {
            get; set;
        }
        public ObservableCollection<ObservableCollection<MNISTModelResult>> ImageClasses
        {
            get; set;
        } 
        public ObservableCollection<string> ClassesInfo {get; set;}
        public  async Task PredImages(string dirPath)
        {
            var images=await Task.Run<IEnumerable<string>>(()=> 
                {return Directory.EnumerateFiles(dirPath);});
            ImageResults.Clear();
            foreach(var imageClass in ImageClasses)
            {
                imageClass.Clear();
                ClassesInfo[ImageClasses.IndexOf(imageClass)]=
                    ClassInfoProcess(ImageClasses.IndexOf(imageClass), 0);
            }
            foreach (var image in images)
            {
                ImageResults.Add(new MNISTModelResult(image));
            }
            await model.PredImages(dirPath, source.Token);
        }
        MNISTModel model;
        CancellationTokenSource source;
        public MNISTModelVM()
        {
            model=new MNISTModel();
            model.ResultIsReady+=ResultEventHandler;
            ImageResults=new ObservableCollection<MNISTModelResult>();
            ImageClasses=new ObservableCollection<ObservableCollection<MNISTModelResult>>();
            ClassesInfo=new ObservableCollection<string>();
            for (int i=0; i<MNISTModel.NumOfClasses; i++) 
            {
                ImageClasses.Add(new ObservableCollection<MNISTModelResult>());
                ClassesInfo.Add(ClassInfoProcess(i, 0));
            }
            source=new CancellationTokenSource();
        }
        public void CancelEventHandler(object sender, ConsoleCancelEventArgs args)
        {
            source.Cancel();
        }
        void ResultEventHandler(object sender, ResultEventArgs args) 
        {
            var result=args.Result;
            int index=0;
            foreach (var image in ImageResults) 
            {
                if (image.ImagePath==result.ImagePath)
                {
                    index=ImageResults.IndexOf(image);
                    break;
                }
            } 
            Dispatcher.UIThread.InvokeAsync(()=>{ImageResults[index]=new MNISTModelResult(result);
                ImageClasses[result.ImageClass].Add(new MNISTModelResult(result));
                ClassesInfo[result.ImageClass]=ClassInfoProcess(result.ImageClass, 
                    ImageClasses[result.ImageClass].Count);});
        }
        string ClassInfoProcess(int imageClass, int number)
        {
            return "|["+imageClass+"]| = "+number;
        }
    }
}