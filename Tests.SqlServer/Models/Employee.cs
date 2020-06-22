using AO.Models;
using AO.Models.Enums;
using AO.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;

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
    public class Employee : ITrigger
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

        public async Task RowDeletedAsync(IDbConnection connection, IDbTransaction txn = null)
        {
            Value = OtherEnum.Other;
            await Task.CompletedTask;
        }

        public async Task RowSavedAsync(IDbConnection connection, SaveAction saveAction, IDbTransaction txn = null)
        {
            await Task.CompletedTask;
        }
    }
}
