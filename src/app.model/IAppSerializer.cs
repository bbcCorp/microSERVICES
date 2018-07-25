
using System;

namespace app.model
{
    public interface IAppSerializer<T>
    {  
        string ToJson();
        T FromJson(string serializedJSON);
    }
  
}