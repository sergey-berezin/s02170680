using System;
using System.Threading;

using System.Threading.Tasks;

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

        static async Task Main(string[] args)
        {
            MNISTModel model=new MNISTModel();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelEventHandler);
            model.ResultIsReady+=ResultEventHandler;
            try
            {
                // Console.Write("Choose directories with images: ");
                string dirPath=@"..\MNISTModelLib\Samples\Decoded"; //Console.ReadLine();
                await model.PredImages(dirPath, source.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
