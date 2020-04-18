namespace MSharp.Framework.Data
{
    public class SortQueryOption : QueryOption
    {
        /// <summary>
        /// Creates a new SortQueryOption instance.
        /// </summary>
        internal SortQueryOption() { }


        /// <summary>
        /// Gets or sets the Property of this SortQueryOption.
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the Descending of this SortQueryOption.
        /// </summary>
        public bool Descending { get; set; }

        internal override void Configure(DatabaseQuery query) => query.OrderBy(Property, Descending);
    }
}