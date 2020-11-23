using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Threading;
using System.Threading.Tasks;


namespace MNISTModelLib
{
    public class MNISTModelResult 
    {
        public string ImagePath {get; set;}
        public int ImageClass {get; set;}
        public float Confidence {get; set;}
        public MNISTModelResult(string imagePath=null, int imageClass=-1, float confidence=-1)
        {
            ImagePath=imagePath;
            ImageClass=imageClass;
            Confidence=confidence;
        }

        public MNISTModelResult(MNISTModelResult result)
        {
            ImagePath=result.ImagePath;
            ImageClass=result.ImageClass;
            Confidence=result.Confidence;
        }
        public override string ToString()
        {
            return ImagePath+": "+"class = "+ImageClass.ToString()+
                   ", confidence = "+Confidence.ToString();
        }
    }


    public delegate void  ResultEventHandler(object sender, ResultEventArgs args);
    public class ResultEventArgs: EventArgs
    {
        public MNISTModelResult Result {get;}
        public ResultEventArgs(MNISTModelResult result) 
        {
            Result=new MNISTModelResult(result.ImagePath, result.ImageClass, result.Confidence);
        }
    }


    public class MNISTModel
    {

        const int numOfClasses = 10;

        const int imageWidth=28;
        const int imageHeight=28;
        string ModelPath {get;}
        string InputName {get;}


        public static int NumOfClasses => numOfClasses;


        public event ResultEventHandler ResultIsReady;
        public MNISTModel(string modelPath=@"..\MNISTModelLib\mnist-8.onnx", 
                          string inputName="Input3")
        {
            ModelPath=modelPath;
            InputName=inputName;
        }
        public MNISTModelResult PredImage(string imagePath)
        {
            Image<Rgb24> image;
            MNISTModelResult result=new MNISTModelResult();
        
            //crop
            image = Image.Load<Rgb24>(imagePath, out IImageFormat format);
            Stream imageStream = new MemoryStream();
            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(imageWidth, imageHeight),
                    Mode = ResizeMode.Crop
                });
            });
            image.Save(imageStream, format);
            //make a tensor
            Tensor<float> input = new DenseTensor<float>(new[] { 1, 1, image.Width, image.Height});
            for (int y = 0; y < image.Height; y++)
            {
                Span<Rgb24> pixelSpan = image.GetPixelRowSpan(y);
                for (int x = 0; x < image.Width; x++)
                {
                    input[0, 0, y, x] = (0.299f*pixelSpan[x].R/255f
                                        +0.587f*pixelSpan[x].G/255f
                                        +0.114f*pixelSpan[x].B/255f);
                }
            }
            //make an input
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(InputName, input)
            };
            //run model
            var session = new InferenceSession(ModelPath);
            var results = session.Run(inputs);
            //postprocess output
            IEnumerable<float> output = results.First().AsEnumerable<float>();
            IEnumerable<float> softmax = output.Select(x => (float)Math.Exp(x) / 
                output.Sum(x => (float)Math.Exp(x)));
            //select max
            Dictionary<int, float> dictOutput=new Dictionary<int, float>();
            int key=0;
            foreach(var value in softmax)
            {
                dictOutput.Add(key, value);
                key++;
            }
            var maxConfElem=dictOutput.FirstOrDefault(elem => elem.Value==dictOutput.Values.Max());
            result.ImagePath=imagePath;
            result.ImageClass=maxConfElem.Key;
            result.Confidence=maxConfElem.Value;
            return result;
        }

        /*Method changed*/
        public async Task PredImages<T>(T directory, CancellationToken token)
        {
            IEnumerable<string> files;
            if (directory is string dirPath) {
                files= await Task.Run<IEnumerable<string>>(()=> 
                    {return Directory.EnumerateFiles(dirPath);});
            }
            else if (directory is IEnumerable<string> dirFiles){
                files = dirFiles;
            }
            else {
                throw new ArgumentException("Only types string and IEnumerable<string> are supported");
            }
            ParallelOptions options = new ParallelOptions();
            options.CancellationToken = token;
            Parallel.ForEach(files, options, (file) =>
            {
                MNISTModelResult result=new MNISTModelResult(file);
                result=this.PredImage(file);
                ResultIsReady?.Invoke(this, new ResultEventArgs(result));
            
            });
        }

    }
}
