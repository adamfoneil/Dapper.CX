namespace Dapper.CX.Interfaces
{
    /// <summary>
    /// implement this on model classes to inject your own SELECT...FROM
    /// when performing a GetAsync or GetWhereAsync
    /// </summary>
    public interface ICustomGet
    {
        /// <summary>
        /// specifies the SELECT...FROM portion of a custom get query
        /// </summary>
        string SelectFrom { get; }

        /// <summary>
        /// syntax of the WHERE identity portion of custom get query,
        /// intended for when you need to prefix the identity column with a table alias.
        /// You should still use @id as the parameter name.
        /// </summary>
        string WhereId { get; }
    }
}
