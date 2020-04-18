using System.Collections.ObjectModel;

namespace AE.Net.Mail.Imap
{
    public class Namespaces
    {
        Collection<Namespace> usernamespace = new Collection<Namespace>();
        Collection<Namespace> sharednamespace = new Collection<Namespace>();

        public virtual Collection<Namespace> ServerNamespace { get; } = new Collection<Namespace>();

        public virtual Collection<Namespace> UserNamespace => usernamespace;
        public virtual Collection<Namespace> SharedNamespace => sharednamespace;
    }

    public class Namespace
    {
        public Namespace(string prefix, string delimiter)
        {
            Prefix = prefix;
            Delimiter = delimiter;
        }
        public Namespace() { }
        public virtual string Prefix { get; internal set; }
        public virtual string Delimiter { get; internal set; }
    }
}