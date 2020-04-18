using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MSharp.Framework
{
    public interface IHttpContextItemsAccessor
    {
        bool ItemsAvaiable { get; }

        IDictionary Items { get; }
    }
}
