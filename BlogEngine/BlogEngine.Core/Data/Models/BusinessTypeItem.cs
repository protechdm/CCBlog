using System;

namespace BlogEngine.Core.Data.Models
{
    /// <summary>
    /// Category item
    /// </summary>
    public class BusinessTypeItem
    {
        /// <summary>
        /// If checked in the UI
        /// </summary>
        public bool IsChecked { get; set; }
        /// <summary>
        /// Unique Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// BusinessType
        /// </summary>
        public String BusinessType { get; set; }
        
        /// <summary>
        /// Counter
        /// </summary>
        public int Count { get; set; }
    }
}
