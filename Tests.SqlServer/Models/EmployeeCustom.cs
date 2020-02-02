using AO.DbSchema.Attributes;
using Dapper.CX.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;

namespace Tests.Models
{
    [Identity(nameof(Id))]
    [Table("Employee")]
    public class EmployeeCustom : ICustomGet, IGetRelated<EmployeeCustom>
    {
        [Key]
        public string FirstName { get; set; }
        [Key]
        public string LastName { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? TermDate { get; set; }
        public bool IsExempt { get; set; }
        public DateTime? Timestamp { get; set; }
        public int Id { get; set; }

        [NotMapped]
        public decimal Balance { get; set; }
        [NotMapped]
        public string Whatever { get; set; }

        public IEnumerable<string> Something { get; set; }
        public IEnumerable<DateTime> SomethingElse { get; set; }

        public string SelectFrom =>
            @"SELECT [emp].*, [se].[Balance], [se].[Whatever]
            FROM [dbo].[Employee] [emp]
            LEFT JOIN [dbo].[SomethingElse] [se] ON [emp].[Id]=[se].[EmployeeId]";

        public string WhereId => "[emp].[Id]=@id";

        public Func<IDbConnection, EmployeeCustom, Task> OnGetAsync => async (cn, model) =>
        {
            model.Something = new string[] { "this", "that", "other" };
            model.SomethingElse = new DateTime[] { DateTime.Today };
            await Task.CompletedTask;
        };
    }    
}
