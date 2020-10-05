using System;
using System.Threading;
using ModelLib;

namespace Parallel
{
    
    class Program
    {
        static CancellationTokenSource source=new CancellationTokenSource();
        static void CancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            source.Cancel();
        }
        static void Main(string[] args)
        {
            Model model=new Model();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);
            model.PredImages(@"..\ModelLib\Samples", Console.OpenStandardOutput(), source.Token); 
            // FileStream fileStream=File.OpenWrite("output.txt");
            // model.PredImages(@"..\ModelLib\Samples", fileStream, source.Token);
            Console.Write(model.ErrorMsg);
        }
    }
}
