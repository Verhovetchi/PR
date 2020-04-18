using System;
using System.Collections.Generic;
using System.Text;

namespace MSharp.Framework
{
    public interface IContextParameterValueProvider
    {
        string Param(string key);
    }
}
