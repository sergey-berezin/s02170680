using MNISTModelLib;

namespace ImagePredUIDb.ViewModels
{
    public class MNISTModelResultDb: MNISTModelResult 
    {
        public int RetrieveCount {get; set;}
         public MNISTModelResultDb(string imagePath): base(imagePath)
        {
            RetrieveCount=0;
        }
        public MNISTModelResultDb(MNISTModelResult result, int retrieveCount=0): base(result)
        {
            RetrieveCount=retrieveCount;
        }
    }
}