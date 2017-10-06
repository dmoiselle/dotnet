﻿using Newtonsoft.Json;
using System;


namespace Bridge.Web.Utility
{
    public class GeneralJsonEnumConverter : JsonConverter
    {
        public override bool CanConvert( Type objectType ) {
            return objectType.IsEnum;
        }

        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer ) {
            writer.WriteValue( value.ToString() );
        }

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer ) {
            return Enum.Parse( objectType, reader.Value.ToString() );
        }
    }
}