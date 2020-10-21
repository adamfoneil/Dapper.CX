using Dapper.CX.Abstract;
using System;
using System.Data;
using System.Text.Json;

namespace Tests.Base
{
    public class SessionDbDictionary : DbDictionary<string>
    {
        public SessionDbDictionary(Func<IDbConnection> getConnection) : base(getConnection, "session.User")
        {
        }

        protected override TValue Deserialize<TValue>(string value) => JsonSerializer.Deserialize<TValue>(value);

        protected override string Serialize<TValue>(TValue value) => JsonSerializer.Serialize(value);
    }
}
