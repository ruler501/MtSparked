using System.ComponentModel;

namespace MtSparked.Interop.Utils {
    public static class MyEnumExtensions {

        public static string ToDescriptionString<T>(this T val) where T : System.Enum {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
               .GetType()
               .GetField(val.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : System.String.Empty;
        }

    }
}