using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class LanguageSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public LanguageSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            List<Language> wantedLanguages = subject.Movie.Profile.Value.PreferredLanguages
                .Where(item => item.Allowed)
                .Select(item => item.Language)
                .ToList();

            if (wantedLanguages.Contains(Language.Any))
            {
                _logger.Debug("Profile allows any language, accepting release.");
                return Decision.Accept();
            }

            _logger.Debug("Checking if report meets language requirements. {0}", subject.ParsedMovieInfo.Languages.ToExtendedString());

            if (!subject.ParsedMovieInfo.Languages.Intersect(wantedLanguages).Any())
            {
                _logger.Debug("Report Language: {0} rejected because it is not wanted, wanted {1}", subject.ParsedMovieInfo.Languages.ToExtendedString(), wantedLanguages.ToExtendedString());
                return Decision.Reject("{0} is wanted, but found {1}", wantedLanguages.ToExtendedString(), subject.ParsedMovieInfo.Languages.ToExtendedString());
            }

            return Decision.Accept();
        }
    }
}
