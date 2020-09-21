using AO.Models;
using AO.Models.Attributes;
using AO.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Models
{
    [TrackChanges]
    public partial class Item : IValidate
    {
        public ValidateResult Validate()
        {
            var rules = new Dictionary<Func<Item, bool>, string>()
            {
                { (model) => model.UnitCost < 0, "Unit cost may not be less than zero." },
                { (model) => model.SalePrice < 0, "Sale price may not be less than zero." }
            };

            var errors = rules.Where(kp => kp.Key.Invoke(this)).Select(kp => kp.Value);
            return (errors.Any()) ?
                new ValidateResult() { IsValid = false, Message = string.Join("\r\n", errors) } :
                new ValidateResult() { IsValid = true };
        }

        public async Task<ValidateResult> ValidateAsync(IDbConnection connection, IDbTransaction txn = null)
        {
            return await Task.FromResult(new ValidateResult() { IsValid = true });
        }
    }
}
