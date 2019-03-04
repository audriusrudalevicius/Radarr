using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Profiles
{
    public class ProfileLanguageItem : IEmbeddedDocument
    {
        public int Id { get; set; }
        public bool Allowed { get; set; }
        public Language Language { get; set; }
    }
}
