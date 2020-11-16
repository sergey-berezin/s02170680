using MNISTModelLib;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Avalonia.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.ComponentModel;

namespace ImagePredUIDb.ViewModels {  
    class MNISTModelVM: INotifyPropertyChanged
    {
        public ObservableCollection<MNISTModelResultDb> ImageResults 
        {
            get; set;
        }
        public ObservableCollection<ObservableCollection<MNISTModelResultDb>> ImageClasses
        {
            get; set;
        } 
        public ObservableCollection<string> ClassesInfo {get; set;}
        public CancellationTokenSource Source {get; set;}
        public event PropertyChangedEventHandler PropertyChanged;
        int numOfImages;
        int processed;
        string progress;
        public string Progress 
        {
            get {return progress;}
            set 
            {
                progress=value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }
        public async Task PredImages(string dirPath)
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
            ParallelOptions options = new ParallelOptions();
            options.CancellationToken = Source.Token;
            processed=0;
            numOfImages=imagePaths.Count();
            ProdProgressInfo();
            dbContext=new ImageDbContext();
            await Task.Run(()=>
            {
                Parallel.ForEach(imagePaths, options, (imagePath)=>
                {
                    List<string> imagesToClassify=new List<string>();
                    if (Source.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    MNISTModelResultDb dbImage=null;
                    dbImage=FindImageInDb(imagePath);
                    if (dbImage!=null) 
                    {
                        Dispatcher.UIThread.InvokeAsync(() => 
                        {
                            lock(ImageResults) 
                            {
                                ImageResults.Add(dbImage);
                                ImageClasses[dbImage.ImageClass].Add(dbImage);
                                    ClassesInfo[dbImage.ImageClass]=ClassInfoProcess(dbImage.ImageClass, 
                                        ImageClasses[dbImage.ImageClass].Count);
                                processed++;
                                ProdProgressInfo();
                            }
                        });
                    }
                    else {
                        Dispatcher.UIThread.InvokeAsync(()=> 
                        {
                            lock(ImageResults) 
                            {
                                ImageResults.Add(new MNISTModelResultDb(imagePath));
                            }
                        }); 
                        imagesToClassify.Add(imagePath);  
                        model.PredImages(imagesToClassify, Source.Token).Wait();
                    }
                });
            });    
        }

        public async Task DeleteDatabase()
        {
            await Task.Run(()=> 
            {
                lock (dbContext)
                {
                    try {
                        dbContext.Database.EnsureDeleted();   
                        dbContext.Database.EnsureCreated();
                    }
                    catch(Exception){}           
                }
            });
        }

        MNISTModel model;
        ImageDbContext dbContext;

        public MNISTModelVM()
        {
            model=new MNISTModel();
            model.ResultIsReady+=ResultEventHandler;
            ImageResults=new ObservableCollection<MNISTModelResultDb>();
            ImageClasses=new ObservableCollection<ObservableCollection<MNISTModelResultDb>>();
            ClassesInfo=new ObservableCollection<string>();
            for (int i=0; i<MNISTModel.NumOfClasses; i++) 
            {
                ImageClasses.Add(new ObservableCollection<MNISTModelResultDb>());
                ClassesInfo.Add(ClassInfoProcess(i, 0));
            }
            Source=new CancellationTokenSource();
            processed=0;
            numOfImages=0;
            progress=null;
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
                        ImageResults[index]=new MNISTModelResultDb(result);
                        ImageClasses[result.ImageClass].Add(new MNISTModelResultDb(result));
                        ClassesInfo[result.ImageClass]=ClassInfoProcess(result.ImageClass, 
                            ImageClasses[result.ImageClass].Count);
                        processed++;
                        ProdProgressInfo();
                    }
                });  
                Task.Run(()=> 
                {
                    lock (dbContext) 
                    {
                        Bitmap resImage=new Bitmap(result.ImagePath);
                        Blob resBlob=new Blob {Bytes=ImageToByteArray(resImage)};
                        dbContext.ClassifiedImages.Add(new ClassifiedImage {Path=result.ImagePath, 
                            Class=result.ImageClass, Confidence=result.Confidence, 
                            RetrieveCount=0, Image=resBlob});
                        dbContext.Blobs.Add(resBlob);
                        dbContext.SaveChanges();
                    }
                });
            }   
        }
        public byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, image.RawFormat);
                return stream.ToArray();
            }
        }
        Image ByteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream stream = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(stream);
            }
        }
        MNISTModelResultDb FindImageInDb(string imagePath)
        {
            byte[] blob;
            try {
                blob=ImageToByteArray(new Bitmap(imagePath));
            }
            catch(Exception) {return null;}
            lock (dbContext)
            {
                ClassifiedImage classifiedImage;
                var classifiedImages=dbContext.ClassifiedImages.
                    Where(img => 
                        img.Path.Equals(imagePath)).
                    Select(img => img).ToList();
                if (classifiedImages.Any()) 
                {
                    classifiedImage=classifiedImages.First();
                    dbContext.Entry(classifiedImage).Reference(img => img.Image).Load();
                    if (Enumerable.SequenceEqual(blob, classifiedImage.Image.Bytes)) 
                    {
                        classifiedImage.RetrieveCount+=1;
                        dbContext.SaveChanges();
                        return new MNISTModelResultDb(new MNISTModelResult(classifiedImage.Path, 
                            classifiedImage.Class, classifiedImage.Confidence), 
                            classifiedImage.RetrieveCount);
                    }
                }
            }
            return null;
        }
        string ClassInfoProcess(int imageClass, int number)
        {
            return "|["+imageClass+"]| = "+number;
        }
        
        void ProdProgressInfo() 
        {
            // string[] symbols = new string[] {"|", "/", "--", @"\"};
            if (processed<numOfImages) {
                Progress=$"{processed}/{numOfImages} processed";
            }
            else 
            {
                Progress=null;
            }    
        }
    }
}