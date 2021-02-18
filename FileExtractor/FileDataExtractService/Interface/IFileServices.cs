namespace FileDataExtractService.Interface
{
    using System.Threading.Tasks;

    public interface IFileServices
    {
        Task<string> GetBlobFileInfo();
    }
}
