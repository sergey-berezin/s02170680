from skimage import io
import numpy as np
import os

mnist_images_path=r't10k-images-idx3-ubyte\t10k-images.idx3-ubyte' 
    #input('Choose file with mnist image set (idx3-ubyte format): ')
mnist_labels_path=r't10k-labels-idx1-ubyte\t10k-labels.idx1-ubyte' 
    #input('Choose file with mnist labels (idx1-ubyte format): ')
images=open(mnist_images_path, 'rb')
labels=open(mnist_labels_path, 'rb')

labels_magic_num=int.from_bytes(labels.read(4), byteorder='big')
label_num=int.from_bytes(labels.read(4), byteorder='big')
images_magic_num=int.from_bytes(images.read(4), byteorder='big')
images_num=int.from_bytes(images.read(4), byteorder='big')
rows_num=int.from_bytes(images.read(4), byteorder='big')
columns_num=int.from_bytes(images.read(4), byteorder='big')

print('number of labels: '+str(label_num))
print('number of images: '+str(images_num))
print('image shape: ('+str(rows_num)+', '+str(columns_num)+')\n')
decoded_num=int(input('Choose number of images to decoded: '))
decoded_dir='Decoded' #input('Choose directory to store decodedd images: ')
try:
    os.mkdir(decoded_dir)
except OSError:
    print ("Creation of the directory %s failed" % decoded_dir)
    exit
old_files=os.scandir(decoded_dir)
for file in old_files:
    os.remove(file)    

classes=np.zeros(10)
for k in range(decoded_num):
    byte=labels.read(1)
    if not byte:
        break
    label=int.from_bytes(byte, byteorder='big')
    classes[label]+=1
    image=np.ndarray((rows_num, columns_num), np.uint8)
    for i in range(rows_num):
        for j in range(columns_num):
            image[i, j]=int.from_bytes(images.read(1), byteorder='big') 
    io.imsave(os.path.join(decoded_dir, str(label)+'('+str(int(classes[label]))+').jpg'), image)

images.close()
labels.close()