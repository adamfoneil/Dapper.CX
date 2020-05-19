using AO.Models;
using AO.Models.Interfaces;
using System.Data;
using System.Threading.Tasks;
using Tests.Models;

namespace Tests.SqlServer.Models
{
    public class EmployeeValid : Employee, IValidate
    {
        public ValidateResult Validate()
        {
            if (TermDate < HireDate)
            {
                return new ValidateResult()
                {
                    Message = "TermDate cannot be before HireDate",
                    IsValid = false
                };
            }

            return new ValidateResult() { IsValid = true };
        }

        public Task<ValidateResult> ValidateAsync(IDbConnection connection, IDbTransaction txn = null)
        {
            return Task.FromResult(new ValidateResult() { IsValid = true });
        }
    }
}
