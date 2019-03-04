using System.Collections.Generic;
using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class LanguageSpecificationFixture : CoreTest
    {
        private RemoteMovie _remoteMovie;

        [SetUp]
        public void Setup()
        {
            _remoteMovie = new RemoteMovie
            {
                ParsedMovieInfo = new ParsedMovieInfo
                {
                    Languages = new List<Language> {Language.English}
                },
                Movie = new Movie
                {
                    Profile = new LazyLoaded<Profile>(new Profile
                    {
                        PreferredLanguages = new List<ProfileLanguageItem>
                        {
                            new ProfileLanguageItem
                            {
                                Language = Language.English,
                                Allowed = true
                            }
                        }
                    })
                }
            };
        }

        private void WithEnglishRelease()
        {
            _remoteMovie.ParsedMovieInfo.Languages = new List<Language> {Language.English};
        }

        private void WithGermanRelease()
        {
            _remoteMovie.ParsedMovieInfo.Languages = new List<Language> {Language.German};
        }

        [Test]
        public void should_return_true_if_language_is_english()
        {
            WithEnglishRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_is_german()
        {
            WithGermanRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_allowed_language_any()
        {
            _remoteMovie.Movie.Profile = new LazyLoaded<Profile>(new Profile
            {
                PreferredLanguages = new List<ProfileLanguageItem>
                {
                    new ProfileLanguageItem
                    {
                        Language = Language.Any,
                        Allowed = true
                    }
                }
            });

            WithGermanRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();

            WithEnglishRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }
    }
}
