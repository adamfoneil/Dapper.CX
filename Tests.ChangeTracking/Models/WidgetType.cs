using AO.DbSchema.Attributes;
using AO.DbSchema.Attributes.Interfaces;
using Dapper;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;

namespace Tests.ChangeTracking.Models
{
    [Identity(nameof(Id))]
    public class WidgetType
    {
        public int Id { get; set; }

        [MaxLength(50)]
        [Key]
        public string Name { get; set; }
    }

    public class Widget : ITextLookup
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [References(typeof(WidgetType))]
        public int TypeId { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<string> GetLookupProperties() => new string[] { nameof(TypeId) };

        public async Task<string> GetTextFromKeyAsync(IDbConnection connection, IDbTransaction transaction, string propertyName, object keyValue)
        {
            switch (propertyName)
            {
                case nameof(TypeId):
                    return await connection.QuerySingleAsync<string>("SELECT [Name] FROM [dbo].[WidgetType] WHERE [Id]=@keyValue", new { keyValue }, transaction);

                default:
                    return null;
            }            
        }
    }
}
