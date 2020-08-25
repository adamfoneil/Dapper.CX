using AO.Models;
using AO.Models.Interfaces;
using System.Data;
using System.Threading.Tasks;

namespace SampleApp.Models
{
    public partial class Workspace : IValidate
    {
        public ValidateResult Validate()
        {
            return (Name.Equals("zooropa")) ?
                new ValidateResult() { IsValid = false, Message = "Can't use the name zooropa." } :
                new ValidateResult() { IsValid = true };
        }

        public Task<ValidateResult> ValidateAsync(IDbConnection connection, IDbTransaction txn = null)
        {
            return Task.FromResult(new ValidateResult() { IsValid = true });
        }
    }
}
