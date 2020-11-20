using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Schema;

namespace JCS.Argon.Services.Core
{
    public interface ICollectionManager
    {
        /// <summary>
        /// Return a count of all currently known collections 
        /// </summary>
        /// <returns></returns>
        public Task<int> CountCollectionsAsync();

        /// <summary>
        /// Return a total count of all currently known documents
        /// </summary>
        /// <returns></returns>
        public Task<int> CountDocumentsAsync();

        /// <summary>
        /// Retrieves a list of all current collections
        /// </summary>
        /// <returns></returns>
        public Task<List<Collection>> ListCollections();

        public Task<Collection> CreateCollection(CreateCollectionCommand cmd);
    }
}