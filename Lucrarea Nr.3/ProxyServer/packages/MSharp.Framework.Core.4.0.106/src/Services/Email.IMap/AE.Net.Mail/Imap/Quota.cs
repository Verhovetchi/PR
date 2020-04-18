namespace AE.Net.Mail.Imap
{
    public class Quota
    {
        string Ressource, Usage;
        int used, max;
        public Quota(string ressourceName, string usage, int used, int max)
        {
            Ressource = ressourceName;
            Usage = usage;
            this.used = used;
            this.max = max;
        }
        public virtual int Used => used;
        public virtual int Max => max;
    }
}