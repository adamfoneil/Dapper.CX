using AO.DbSchema.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tests.Models
{
    public enum Status
    {
        Active,
        Inactive
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
        public int Id { get; set; }

        public IEnumerable<string> Something { get; set; }
        public IEnumerable<DateTime> SomethingElse { get; set; }
    }
}
