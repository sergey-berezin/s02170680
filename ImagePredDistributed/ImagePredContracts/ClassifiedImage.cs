namespace ImagePredContracts
{
    public class ClassifiedImage
    {
        public int Id {get; set;}
        public string Name {get; set;}
        public int Class {get; set;}
        public float Confidence {get; set;}
        public int RetrieveCount {get; set;}
        public string ImageBase64 {get; set;}
    }
}