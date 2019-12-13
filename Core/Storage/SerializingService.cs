using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace JellyMusic.Core
{
    public class SerializingService<T>
    {
        // Fields
        private readonly string _folderPath;
        private readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        // Constructor
        public SerializingService(string folderPath)
        {
            Directory.CreateDirectory(folderPath); // does nothing if folder already exists
            _folderPath = folderPath;
        }

        // Methods
        public void Serialize(string objectName, T objectToSerialize)
        {
            string filePath = Path.Combine(_folderPath, objectName);

            using (FileStream memoryStream = new FileStream(filePath, FileMode.Create))
            {
                binaryFormatter.Serialize(memoryStream, objectToSerialize);
            }
        }
        public T Deserialize(string objectName)
        {
            string filePath = Path.Combine(_folderPath, objectName);

            using (FileStream memoryStream = new FileStream(filePath, FileMode.Open))
            {
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}
