using System;
using System.Collections.Generic;
using System.Text;

namespace MSharp.Framework
{
    public interface ISessionMemoryAccessor
    {
        SessionMemory GetInstance(Func<SessionMemory> factory);
    }
}
