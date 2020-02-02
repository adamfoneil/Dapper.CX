using AO.DbSchema.Attributes.Interfaces;
using AO.DbSchema.Attributes.Models;
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

        public Task<ValidateResult> ValidateAsync(IDbConnection connection)
        {
            return Task.FromResult(new ValidateResult() { IsValid = true });
        }
    }
}
