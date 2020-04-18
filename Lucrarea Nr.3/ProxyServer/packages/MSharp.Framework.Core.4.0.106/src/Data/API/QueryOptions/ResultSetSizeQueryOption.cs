namespace MSharp.Framework.Data
{
    public class ResultSetSizeQueryOption : QueryOption
    {
        /// <summary>
        /// Creates a new ResultSetSizeQueryOption instance.
        /// </summary>
        internal ResultSetSizeQueryOption()
        {
        }

        /// <summary>
        /// Gets or sets the Number of this ResultSetSizeQueryOption.
        /// </summary>
        public int? Number { get; set; }

        internal override void Configure(DatabaseQuery query) => query.Top(Number.Value);
    }
}