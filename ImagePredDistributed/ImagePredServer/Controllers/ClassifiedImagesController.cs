using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImagePredContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ImagePredServer.Database;
using ImagePredServer.Classifier;

namespace ImagePredServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClassifiedImagesController : ControllerBase
    {
        private IImageClassifier classifier;
        private IImageDb db;
        public ClassifiedImagesController(IImageClassifier classifier, IImageDb db)
        {
            this.classifier=classifier;
            this.db=db;
        }
        [HttpGet]
        public ClassifiedImage[] GetImages()
        {
            return db.GetImages();
        }
        [HttpGet("stats")] 
        public int[] GetStats(string  page)
        {
            return db.GetStats();
        }
        [HttpPut]
        public ClassifiedImage PutImage(NewImage newImage)
        {
            ClassifiedImage classifiedImage=db.FindImage(newImage);
            if (classifiedImage!=null)
            {
                return classifiedImage;
            }
            classifiedImage= classifier.Classify(newImage);
            db.PutImage(classifiedImage);
            return classifiedImage;
        }
        [HttpDelete]
        public void DeleteImages()
        {
            db.DeleteImages();
        }
    }
}
