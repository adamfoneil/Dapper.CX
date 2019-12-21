namespace Dapper.CX.Classes
{
    /// <summary>
    /// Use this to inject a literal expression into a SqlCmdDictionary value
    /// </summary>
    public class SqlExpression
    {
        public SqlExpression(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }
}
