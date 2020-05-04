using System;
using System.Collections.Generic;
using System.Text;
using Byt3.Serialization;
using Byt3.Serialization.Serializers.Base;

namespace MasterServer.Common
{
    public class SerializerSingleton
    {
        public static readonly Byt3Serializer Serializer = Byt3Serializer.GetDefaultSerializer(new TypeSafeBaseSerializer());
    }
}
