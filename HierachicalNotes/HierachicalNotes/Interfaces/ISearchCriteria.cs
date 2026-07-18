using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Xps.Serialization;

namespace HierachicalNotes.Interfaces
{
    public interface ISearchCriteria
    {
        /// <summary>
        /// Check whether textSource has matches with this criterion
        /// </summary>
        /// <param name="textSource">the source text</param>
        /// <returns>true:has matches, false:no matches</returns>
        bool HasMatches(string textSource);

        /// <summary>
        /// Determines whether the specified text source has matches.
        /// </summary>
        /// <param name="textSource">The text source.</param>
        /// <param name="itemDateTime">The item date time.</param>
        /// <returns>
        ///   <c>true</c> if the specified text source has matches; otherwise, <c>false</c>.
        /// </returns>
        bool HasMatches(string textSource, DateTime? itemDateTime);
    }
}
