using System.Threading.Tasks;

namespace ILICheck.Web
{
    public interface IValidator
    {
        Task ValidateAsync(string jobId, bool deleteTransferFile, string uploadFilePath);
    }
}
