using System.ComponentModel;

namespace MSharp.Framework.Data
{
    public class AuditSaveEventArgs : CancelEventArgs
    {
        public IEntity Entity { get; set; }
        public IApplicationEvent ApplicationEvent { get; set; }
        public SaveMode SaveMode { get; set; }
    }

    public class AuditDeleteEventArgs : CancelEventArgs
    {
        public IEntity Entity { get; set; }
        public IApplicationEvent ApplicationEvent { get; set; }
    }
}