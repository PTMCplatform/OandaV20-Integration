// Copyright PFSOFT LLC. © 2003-2017. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OandaV20ExternalVendor.TradeLibrary
{
    public static class EnumJsonSerialiserExtension
    {      
        public static Enum DeserializeFromJson(this Enum enumObj, string stringValue)
        {
            Type enumType = enumObj.GetType();
     
            var found = enumType.GetMembers()
            .Select(x => new
            {
                Member = x,
                Attribute = x.GetCustomAttributes(typeof(EnumMemberAttribute), false).OfType<EnumMemberAttribute>().FirstOrDefault()
            })
            .FirstOrDefault(x => x.Attribute?.Value == stringValue);

            if (found != null)
                enumObj = (Enum)Enum.Parse(enumType, found.Member.Name);

            return enumObj;
        }

        public static string SerializeToJson(this Enum enumObj)
        {
            Type enumType = enumObj.GetType();
            
            var memInfo = enumType.GetMember(enumObj.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(EnumMemberAttribute),
                false);
            return ((EnumMemberAttribute)attributes[0]).Value;
        }
    }
}
