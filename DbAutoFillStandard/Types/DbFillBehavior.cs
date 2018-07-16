namespace DbAutoFillStandard.Types
{
    /// <summary>
    /// Defines the filling behavior (from DB to object or vice-versa, or both, or none).
    /// </summary>
    public enum DbFillBehavior
    {
        /// <summary>
        /// Fills the object from the database result only.
        /// </summary>
        FromDB,

        /// <summary>
        /// Uses the object value(s) to send to the database as parameters.
        /// </summary>
        ToDB,

        /// <summary>
        /// Combines FromDB and ToDB. 
        /// </summary>
        Both,

        /// <summary>
        /// Have no behavior. Doesn't communicate with DB.
        /// </summary>
        None
    }
}