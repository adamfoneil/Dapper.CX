using Dapper.CX.Attributes;
using System;
using System.Collections.Generic;

namespace Tests.Models
{
    [Identity(nameof(Id))]
    public class Employee
    {
        [PrimaryKey]
        public string FirstName { get; set; }
        [PrimaryKey]
        public string LastName { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? TermDate { get; set; }
        public bool IsExempt { get; set; }
        public DateTime? Timestamp { get; set; }
        public int Id { get; set; }

        public IEnumerable<string> Something { get; set; }
        public IEnumerable<DateTime> SomethingElse { get; set; }
    }
}
