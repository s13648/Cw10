using System.Threading.Tasks;
using Cw10.Dto;

namespace Cw10.Services
{
    public interface IStudyDbService
    {
        Task<Study> GetByName(string modelStudies);
    }
}
