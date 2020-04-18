namespace MSharp.Framework
{
    /// <summary>
    /// Created from a persisted Document to prevent unnecessary file loading when it's not actually changed.
    /// So that if an entity is being updated, while its original file is not changed,
    /// we don't do an unnecessary file operation.
    /// </summary>
    class ClonedDocument : Document
    {
        Document Original;

        public ClonedDocument(Document original) : base(original.FileName) => Original = original;

        bool BelongsToOriginal => Owner == Original.Owner && OwnerProperty == Original.OwnerProperty;

        public override byte[] FileData => Original.FileData;

        public override void SaveOnDisk()
        {
            if (BelongsToOriginal) return;

            NewFileData = FileData;
            base.SaveOnDisk();
        }
    }
}