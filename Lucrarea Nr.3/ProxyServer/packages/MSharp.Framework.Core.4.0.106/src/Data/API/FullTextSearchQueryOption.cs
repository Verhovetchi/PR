using System.Collections.Generic;

namespace MSharp.Framework.Data.QueryOptions
{
    public class FullTextSearchQueryOption : QueryOption
    {
        /// <summary>
        /// Creates a new FullTextIndexQueryOption instance.
        /// </summary>
        internal FullTextSearchQueryOption() { }

        /// <summary>
        /// Gets or sets the Keywords of this FullTextIndexQueryOption.
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// Gets or sets the Properties of this FullTextIndexQueryOption.
        /// </summary>
        public IEnumerable<string> Properties { get; set; }

        internal override void Configure(DatabaseQuery query)
            => throw new System.NotImplementedException("Full Text Search is not implemented in the new model yet.");
    }
}
