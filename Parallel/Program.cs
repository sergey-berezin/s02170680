using System;
using System.IO;
using ModelLib;
using System.Threading;

namespace Parallel
{
    class Program
    {
        static void Main(string[] args)
        {
            Model model=new Model();
            CancellationTokenSource source=new CancellationTokenSource();
            // source.Cancel();
            // model.PredImages(@"..\ModelLib\Samples", Console.OpenStandardOutput(), source.Token); 
            FileStream fileStream=File.OpenWrite("output.txt");
            model.PredImages(@"..\ModelLib\Samples", fileStream, source.Token);
            Console.Write(model.ErrorMsg);
        }
    }
}
