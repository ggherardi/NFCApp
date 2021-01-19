using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CSharp.NFC
{
    public static class Utility
    {
        public static string GetEnumDescription(Enum value)
        {
            string description = string.Empty;
            string valueAsString = value.ToString();
            if (!string.IsNullOrEmpty(valueAsString))
            {
                FieldInfo fieldInfo = value.GetType().GetField(valueAsString);
                DescriptionAttribute attribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null)
                {
                    description = attribute.Description;
                }
            }
            return description;
        }
    }
}
