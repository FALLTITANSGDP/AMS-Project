using AMS.Models;

namespace AMS.Services
{
    public interface IDbOperations
    {
        Task<T> SaveData<T>(T data, string dbDocument) where T : BaseModel;

        Task<bool> DeleteData(string id, string dbDocument);

        Task<T> UpdateData<T>(string id, T data, string dbDocument) where T : BaseModel;

        Task<List<T>> GetAllData<T>(string dbDocument) where T : BaseModel;
    }
}
