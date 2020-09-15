using AO.Models.Interfaces;
using System;

namespace Dapper.CX.Classes
{
    public class SystemUser : IUserBase
    {
        public SystemUser(string userName)
        {
            Name = userName;
        }

        public string Name { get; }

        public DateTime LocalTime => DateTime.UtcNow;
    }
}
