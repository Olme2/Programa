// En una carpeta como /Helpers o /Extensions
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
namespace TempDataExtension;
public static class TempDataExtensions
{
    /// <summary>
    /// Guarda un objeto complejo en TempData serializándolo a JSON.
    /// </summary>
    public static void Set<T>(this ITempDataDictionary tempData, string key, T value) where T : class
    {
        tempData[key] = JsonConvert.SerializeObject(value);
    }

    /// <summary>
    /// Recupera un objeto complejo de TempData deserializándolo desde JSON.
    /// </summary>
    public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class
    {
        tempData.TryGetValue(key, out object o);
        return o == null ? null : JsonConvert.DeserializeObject<T>((string)o);
    }
}