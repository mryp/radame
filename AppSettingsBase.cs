using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Radame
{
    /// <summary>
    /// アプリケーション設定データ管理
    /// 参考：http://8thway.blogspot.jp/2015/04/winrt-settings.html
    /// </summary>
    public abstract class AppSettingsBase : INotifyPropertyChanged
    {
        protected enum ContainerType
        {
            Local, // Default
            Roaming,
        }

        protected static T GetValue<T>(ContainerType container = default(ContainerType), [CallerMemberName] string propertyName = null)
        {
            return GetValue(default(T), container, propertyName);
        }

        protected static T GetValue<T>(T defaultValue, ContainerType container = default(ContainerType), [CallerMemberName] string propertyName = null)
        {
            try
            {
                var values = GetSettingsValues(container);

                if (values.ContainsKey(propertyName))
                {
                    if (typeof(T).GetTypeInfo().GetCustomAttribute(typeof(DataContractAttribute)) != null)
                    {
                        using (var ms = new MemoryStream())
                        using (var sw = new StreamWriter(ms))
                        {
                            sw.Write(values[propertyName]);
                            sw.Flush();
                            ms.Seek(0, SeekOrigin.Begin);

                            var serializer = new DataContractJsonSerializer(typeof(T));
                            return (T)serializer.ReadObject(ms);
                        }
                    }

                    return (T)values[propertyName];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to get property value: {0}\r\n{1}", propertyName, ex);
            }

            return defaultValue;
        }

        protected static void SetValue<T>(T propertyValue, ContainerType container = default(ContainerType), [CallerMemberName] string propertyName = null)
        {
            try
            {
                var values = GetSettingsValues(container);

                if (typeof(T).GetTypeInfo().IsEnum)
                {
                    var underlyingValue = Convert.ChangeType(propertyValue, Enum.GetUnderlyingType(typeof(T)));

                    if (values.ContainsKey(propertyName))
                        values[propertyName] = underlyingValue;
                    else
                        values.Add(propertyName, underlyingValue);

                    return;
                }

                if (typeof(T).GetTypeInfo().GetCustomAttribute(typeof(DataContractAttribute)) != null)
                {
                    using (var ms = new MemoryStream())
                    using (var sr = new StreamReader(ms))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(T));
                        serializer.WriteObject(ms, propertyValue);

                        ms.Seek(0, SeekOrigin.Begin);
                        var serializedValue = sr.ReadToEnd();

                        if (values.ContainsKey(propertyName))
                            values[propertyName] = serializedValue;
                        else
                            values.Add(propertyName, serializedValue);
                    }
                    return;
                }

                if (values.ContainsKey(propertyName))
                    values[propertyName] = propertyValue;
                else
                    values.Add(propertyName, propertyValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to set property value: {0}\r\n{1}", propertyName, ex);
            }
        }

        private static IPropertySet GetSettingsValues(ContainerType container)
        {
            return (container == ContainerType.Local)
                ? ApplicationData.Current.LocalSettings.Values
                : ApplicationData.Current.RoamingSettings.Values;
        }


        #region INotifyPropertyChanged member

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
