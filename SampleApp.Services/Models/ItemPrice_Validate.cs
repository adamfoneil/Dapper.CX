using AO.Models;
using AO.Models.Interfaces;
using System.Data;
using System.Threading.Tasks;

namespace SampleApp.Models
{
    public partial class ItemPrice : IValidate
    {
        public ValidateResult Validate()
        {
            if (Price < 0) return new ValidateResult("SalePrice may not be less than zero.");

            return new ValidateResult();
        }

        public async Task<ValidateResult> ValidateAsync(IDbConnection connection, IDbTransaction txn = null)
        {
            return await Task.FromResult(new ValidateResult() { IsValid = true });
        }
    }
}
