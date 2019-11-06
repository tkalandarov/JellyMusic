using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace JellyMusic.Core
{
    public class JsonService<T>
    {
        private readonly string _folderPath;

        public JsonService(string folderName)
        {
            _folderPath = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            Directory.CreateDirectory(_folderPath); // does nothing if folder already exists
        }
        public void SerializeToFile(string fileName, T objectToSerialize)
        {
            string filePath = Path.Combine(_folderPath, fileName);
            // serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, objectToSerialize);
            }
        }
        public T DeserializeFromFile(string fileName)
        {
            string filePath = Path.Combine(_folderPath, fileName);
            // serialize JSON directly to a file
            using (StreamReader reader = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                return (T)serializer.Deserialize(reader, typeof(T));
            }
        }
    }
    public static class JsonLite
    {
        public static void SerializeToFile(string filePath, object objectToSerialize)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, objectToSerialize);
            }
        }
        public static object DeserializeFromFile(string filePath, Type typeOfFile)
        {
            // serialize JSON directly to a file
            using (StreamReader reader = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize(reader, typeOfFile);
            }
        }
    }
}
