using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Ombro
{
    public static class IsolatedStorageOperations
    {
        public static async Task Save<T>(this T obj, string file)
        {
            await Task.Run(() =>
            {
                using(IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = storage.CreateFile(file))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        serializer.Serialize(stream, obj);
                    }
                }
            });
        }

        public static async Task<T> Load<T>(string file)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                T obj = default(T);

                if (storage.FileExists(file))
                {
                    using (IsolatedStorageFileStream stream = storage.OpenFile(file, FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        obj = (T)serializer.Deserialize(stream);
                    }
                }
                else
                {
                    obj = Activator.CreateInstance<T>();
                    await obj.Save(file);
                }

                return obj;
            }
        }
    }
}
