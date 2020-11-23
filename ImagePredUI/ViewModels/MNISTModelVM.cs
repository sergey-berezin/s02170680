using MNISTModelLib;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Avalonia.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ImagePredUI.ViewModels {  
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
            var imagePaths=await Task.Run<IEnumerable<string>>(()=> 
                {return Directory.EnumerateFiles(dirPath);});
            ImageResults.Clear();
            Source = new CancellationTokenSource();
            foreach(var imageClass in ImageClasses)
            {
                imageClass.Clear();
                ClassesInfo[ImageClasses.IndexOf(imageClass)]=
                    ClassInfoProcess(ImageClasses.IndexOf(imageClass), 0);
            }
            foreach (var imagePath in imagePaths)
            {
                ImageResults.Add(new MNISTModelResult(imagePath));
            }
            await Task.Run(()=>model.PredImages(dirPath, Source.Token));
        }
        public CancellationTokenSource Source {get; set;}
        MNISTModel model;
        
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
            Source=new CancellationTokenSource();
        }
        void ResultEventHandler(object sender, ResultEventArgs args) 
        { 
            var result=args.Result;
            int index=0;
            lock(ImageResults) {
                foreach (var image in ImageResults) 
                {
                    if (image.ImagePath==result.ImagePath)
                    {
                        index=ImageResults.IndexOf(image);
                        break;
                    }
                }
                Dispatcher.UIThread.InvokeAsync(()=> {
                    lock(ImageResults) {
                        ImageResults[index]=new MNISTModelResult(result);
                        ImageClasses[result.ImageClass].Add(new MNISTModelResult(result));
                        ClassesInfo[result.ImageClass]=ClassInfoProcess(result.ImageClass, 
                            ImageClasses[result.ImageClass].Count);}});
                    
            }   
        }
        string ClassInfoProcess(int imageClass, int number)
        {
            return "|["+imageClass+"]| = "+number;
        }
    }
}