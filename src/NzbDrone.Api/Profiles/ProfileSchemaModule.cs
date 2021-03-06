﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Profiles
{
    public class ProfileSchemaModule : NzbDroneRestModule<ProfileResource>
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly ICustomFormatService _formatService;

        public ProfileSchemaModule(IQualityDefinitionService qualityDefinitionService, ICustomFormatService formatService)
            : base("/profile/schema")
        {
            _qualityDefinitionService = qualityDefinitionService;
            _formatService = formatService;

            GetResourceAll = GetAll;
        }

        private List<ProfileResource> GetAll()
        {
            var items = _qualityDefinitionService.All()
                .OrderBy(v => v.Weight)
                .Select(v => new ProfileQualityItem { Quality = v.Quality, Allowed = false })
                .ToList();

            var formatItems = _formatService.All().Select(v => new ProfileFormatItem
            {
                Format = v, Allowed = true
            }).ToList();

            formatItems.Insert(0, new ProfileFormatItem
            {
                Format = CustomFormat.None,
                Allowed = true
            });

            var profile = new Profile();
            profile.Cutoff = Quality.Unknown;
            profile.Items = items;
            profile.FormatCutoff = CustomFormat.None;
            profile.FormatItems = formatItems;
            profile.PreferredLanguages = Enum.GetValues(typeof(Language)).Cast<Language>()
                .Select(language => new ProfileLanguageItem
                {
                    Language = language,
                    Allowed = language == Language.Any
                })
                .ToList();

            return new List<ProfileResource> { profile.ToResource() };
        }
    }
}
