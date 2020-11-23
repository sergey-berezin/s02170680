using Microsoft.EntityFrameworkCore;

namespace ImagePredUIDb.ViewModels { 

    class ClassifiedImage 
    {
        public int Id {get; set;}
        public string Path {get; set;}
        public int Class {get; set;}
        public float Confidence{get; set;}
        public int RetrieveCount {get; set;}
        public Blob Image {get; set;}
    }

    class Blob 
    {
        public int Id {get; set;}
        public byte[] Bytes {get; set;}
    }
    
    class ImageDbContext: DbContext 
    {
        public DbSet<ClassifiedImage> ClassifiedImages { get; set; }
        public  DbSet<Blob> Blobs {get; set;}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite("Data Source=images.db");
    }
}