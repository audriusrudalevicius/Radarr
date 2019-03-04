using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(151)]
    public class Add_preferred_languages_to_profile : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Profiles").AddColumn("PreferredLanguages").AsString().WithDefaultValue("");
            Execute.WithConnection(ConvertExistingLanguage);
        }

        private void ConvertExistingLanguage(IDbConnection arg1, IDbTransaction arg2)
        {

        }
    }
}
