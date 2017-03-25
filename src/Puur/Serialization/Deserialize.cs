namespace Puur.Serialization
{
    using System;

    public delegate object Deserialize(string data, Type declaringType);
}