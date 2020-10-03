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


namespace ModelLib
{
    public class Model
    {
        const int imageWidth=28;
        const int imageHeight=28;
        public string ErrorMsg {get; set;}
        public string ModelPath {get; set;}
        public Model(string modelPath=@"..\ModelLib\mnist-8.onnx")
        {
            ErrorMsg=null;
            this.ModelPath=modelPath;
        }
        public string PredImage(string imagePath)
        {
            Image<Rgb24> image;
            string strOutput="";
            try
            {
                //crop
                image = Image.Load<Rgb24>(imagePath, out IImageFormat format);
                Stream imageStream = new MemoryStream();
                image.Mutate(x =>
                    {
                        x.Resize(new ResizeOptions
                            {
                                Size = new Size(imageWidth, imageHeight),
                                Mode = ResizeMode.Crop
                            }
                        );
                    }
                );
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
                    NamedOnnxValue.CreateFromTensor("Input3", input)
                };
                //run model
                var session = new InferenceSession(ModelPath);
                var results = session.Run(inputs);
                //postprocess output
                IEnumerable<float> output = results.First().AsEnumerable<float>();
                IEnumerable<float> softmax = output.Select(x => (float)Math.Exp(x) / 
                    output.Sum(x => (float)Math.Exp(x)));
                int i=0;
                foreach(var value in softmax)
                {
                    strOutput+=i.ToString()+": "+value+"; ";
                    i++;
                }
            }
            catch(Exception ex)
            {
                ErrorMsg=ex.Message;    
            }
            return strOutput;
        }
        public void PredImages(string dirPath, Stream outstream, CancellationToken token)
        {
            var files=Directory.EnumerateFiles(dirPath).ToList<string>();
            ParallelOptions options = new ParallelOptions();
            options.CancellationToken = token;
            StreamWriter writer=new StreamWriter(outstream);
            try {
                Parallel.For(0, files.Count(), options, (i) =>
                {
                    // if (options.CancellationToken.IsCancellationRequested)
                    // {
                    //     return;
                    // }    
                    string output=this.PredImage(files[i]);
                    lock (writer) {
                        writer.WriteLine(i+"."+files[i]+" "+output+"\n");
                        writer.Flush();
                    }
                });
            }
            catch(Exception ex)
            {
                ErrorMsg=ex.Message;
            }
        }
    }
}
