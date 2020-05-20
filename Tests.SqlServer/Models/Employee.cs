using AO.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests.Models
{
    public enum Status
    {
        Active,
        Inactive
    }

    public enum OtherEnum
    {
        This,
        That,
        Other
    }

    [Identity(nameof(Id))]
    public class Employee
    {
        [Key]
        public string FirstName { get; set; }
        [Key]
        public string LastName { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? TermDate { get; set; }
        public bool IsExempt { get; set; }
        public DateTime? Timestamp { get; set; }
        public Status Status { get; set; } = Status.Active;
        [Column(Order = 10)]
        public OtherEnum? Value { get; set; }
        public int Id { get; set; }

        public IEnumerable<string> Something { get; set; }
        public IEnumerable<DateTime> SomethingElse { get; set; }
    }
}
