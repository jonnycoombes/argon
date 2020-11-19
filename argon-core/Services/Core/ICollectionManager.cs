using System.Threading.Tasks;

namespace JCS.Argon.Services.Core
{
    public interface ICollectionManager
    {
        /// <summary>
        /// Return a count of all currently known collections (asynchronously)
        /// </summary>
        /// <returns></returns>
        public Task<int> CollectionCountAsync();
    }
}