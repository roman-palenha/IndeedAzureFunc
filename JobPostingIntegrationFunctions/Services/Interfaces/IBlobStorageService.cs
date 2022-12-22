using JobPostingIntegrationFunctions.Models;
using System.Collections.Generic;

namespace JobPostingIntegrationFunctions.Services.Interfaces
{
    public interface IBlobStorageService
    {
        void InsertRecordToTable(string id, string hash);

        BlobRecord GetRecordFromTable(string id);

        IEnumerable<BlobRecord> GetRecordsFromTable();

        void DeleteRecordFromTable(string id);
    }
}
