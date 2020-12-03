using System;
using System.Collections.Generic;
using System.Linq;
using ImagePredContracts;

namespace ImagePredServer.Database 
{
    public class ImageDb: IImageDb
    {
        public ClassifiedImage[] GetImages()
        {
            List<ClassifiedImage> output=new List<ClassifiedImage>();
            using (ImageDbContext dbContext=new ImageDbContext())
            {
                var classifiedDbImages=dbContext.ClassifiedImages;
                foreach (var dbImage in classifiedDbImages )
                {
                    dbContext.Entry(dbImage).Reference(dbImage 
                        => dbImage.Image).Load();
                    output.Add(FromDbImage(dbImage));
                }
            }
            return output.ToArray();
        }
        public int[] GetStats()
        {
            int[] stats=new int[10];
            using (ImageDbContext dbContext=new ImageDbContext())
            {
                IEnumerable<int> imgClasses=Enumerable.Range(0, 10);
                foreach (var imgClass in imgClasses)
                {
                    stats[imgClass]=dbContext.ClassifiedImages.
                                    Where(img => img.Class==imgClass).ToList().Count();
                }
            }
            return stats;
        }
        public ClassifiedImage FindImage(NewImage newImage)
        {
            using(ImageDbContext dbContext=new ImageDbContext())
            {
                var dbImages=dbContext.ClassifiedImages.
                    Where(img => img.Name.Equals(newImage.Name)).
                    ToList();
                if (dbImages.Any())
                {
                    ClassifiedDbImage dbImage=dbImages.First();
                    dbContext.Entry(dbImage).Reference(img => img.Image).Load();
                    if (Enumerable.SequenceEqual(dbImage.Image.Bytes,
                        Convert.FromBase64String(newImage.ImageBase64))) 
                    {
                        dbImage.RetrieveCount+=1;
                        dbContext.SaveChanges();
                        return FromDbImage(dbImage);
                    }
                }
            }
            return null;
        }
        public ClassifiedImage PutImage(ClassifiedImage classifiedImage)
        {
            ClassifiedDbImage dbImage = new ClassifiedDbImage()
                {   
                    Name=classifiedImage.Name, 
                    Class=classifiedImage.Class, 
                    Confidence=classifiedImage.Confidence,
                    RetrieveCount=classifiedImage.RetrieveCount,
                };
            using (ImageDbContext dbContext=new ImageDbContext())
            {
                Blob blob=new Blob(){Bytes=Convert.FromBase64String(classifiedImage.ImageBase64)};
                dbContext.Add(blob);
                dbImage.Image=blob;
                dbContext.Add(dbImage);
                dbContext.SaveChanges();
            }
            classifiedImage.Id=dbImage.Id;
            return classifiedImage;
        }
        public void DeleteImages()
        {
            using (ImageDbContext dbContext=new ImageDbContext())
            {
                dbContext.Database.EnsureDeleted();   
                dbContext.Database.EnsureCreated();
            }
        }
        private ClassifiedImage FromDbImage(ClassifiedDbImage dbImage)
        {
            return new ClassifiedImage(){Id=dbImage.Id, 
                                         Name=dbImage.Name, 
                                         Class=dbImage.Class, 
                                         Confidence=dbImage.Confidence,
                                         RetrieveCount=dbImage.RetrieveCount,
                                         ImageBase64=
                                            Convert.ToBase64String(dbImage.Image.Bytes)};
        }

    }
} 