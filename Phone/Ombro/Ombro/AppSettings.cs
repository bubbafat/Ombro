using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombro
{
    public static class AppSettings
    {
        public static void AddOrUpdate<T>(string setting, T value)
        {
            var app = IsolatedStorageSettings.ApplicationSettings;

            string str = value.ToString();

            if (!app.Contains(setting))
            {
                app.Add(setting, str);
            }
            else
            {
                app[setting] = str;
            }

            app.Save();
        }

        public static T GetValue<T>(string setting, T valueIfMissing)
        {
            var app = IsolatedStorageSettings.ApplicationSettings;
            string found;

            if(!app.TryGetValue<string>(setting, out found))
            {
                return valueIfMissing;
            }

            try
            {
                return (T)Convert.ChangeType(found, typeof(T));
            }
            catch(Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    throw;
                }

                return valueIfMissing;
            }
        }
    }
}
