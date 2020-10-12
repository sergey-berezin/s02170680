using System;
using System.Threading;
using MNISTModelLib;

namespace ImagePredConsole
{
    
    class Program
    {
        static CancellationTokenSource source=new CancellationTokenSource();
        static void CancelEventHandler(object sender, ConsoleCancelEventArgs args)
        {
            source.Cancel();
        }
        static void ResultEventHandler(object sender, ResultEventArgs args) 
        {
            Console.WriteLine(args.Result.ToString());
        }
        static void Main(string[] args)
        {
            MNISTModel model=new MNISTModel();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelEventHandler);
            model.ResultIsReady+=ResultEventHandler;
            try
            {
                model.PredImages(@"..\MNISTModelLib\Samples", source.Token);    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
