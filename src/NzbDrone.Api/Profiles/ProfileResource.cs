﻿using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Qualities;
using NzbDrone.Api.REST;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Profiles
{
    public class ProfileResource : RestResource
    {
        public string Name { get; set; }
        public Quality Cutoff { get; set; }
        public string PreferredTags { get; set; }
        public List<ProfileQualityItemResource> Items { get; set; }
        public CustomFormatResource FormatCutoff { get; set; }
        public List<ProfileFormatItemResource> FormatItems { get; set; }
        public List<ProfileLanguageItemResource> PreferredLanguages { get; set; }
    }

    public class ProfileQualityItemResource : RestResource
    {
        public Quality Quality { get; set; }
        public bool Allowed { get; set; }
    }

    public class ProfileLanguageItemResource : RestResource
    {
        public bool Allowed { get; set; }
        public string Name { get; set; }
    }

    public class ProfileFormatItemResource : RestResource
    {
        public CustomFormatResource Format { get; set; }
        public bool Allowed { get; set; }
    }

    public static class ProfileResourceMapper
    {
        public static ProfileResource ToResource(this Profile model)
        {
            if (model == null) return null;

            return new ProfileResource
            {
                Id = model.Id,

                Name = model.Name,
                Cutoff = model.Cutoff,
                PreferredTags = model.PreferredTags != null ? string.Join(",", model.PreferredTags) : "",
                Items = model.Items.ConvertAll(ToResource),
                FormatCutoff = model.FormatCutoff.ToResource(),
                FormatItems = model.FormatItems.ConvertAll(ToResource),
                PreferredLanguages = model.PreferredLanguages.ConvertAll(ToResource)
            };
        }

        public static ProfileLanguageItemResource ToResource(this ProfileLanguageItem model)
        {
            if (model == null) return null;

            return new ProfileLanguageItemResource
            {
                Id = (int) model.Language,
                Allowed = model.Allowed,
                Name = model.Language.ToString()
            };
        }

        public static ProfileQualityItemResource ToResource(this ProfileQualityItem model)
        {
            if (model == null) return null;

            return new ProfileQualityItemResource
            {
                Quality = model.Quality,
                Allowed = model.Allowed
            };
        }

        public static ProfileFormatItemResource ToResource(this ProfileFormatItem model)
        {
            return new ProfileFormatItemResource
            {
                Format = model.Format.ToResource(),
                Allowed = model.Allowed
            };
        }

        public static Profile ToModel(this ProfileResource resource)
        {
            if (resource == null) return null;

            return new Profile
            {
                Id = resource.Id,

                Name = resource.Name,
                Cutoff = (Quality)resource.Cutoff.Id,
                PreferredTags = resource.PreferredTags.Split(',').ToList(),
                Items = resource.Items.ConvertAll(ToModel),
                FormatCutoff = resource.FormatCutoff.ToModel(),
                FormatItems = resource.FormatItems.ConvertAll(ToModel),
                PreferredLanguages = resource.PreferredLanguages.FindAll(itemResource => itemResource.Allowed).ToList().ConvertAll(ToModel)
            };
        }

        public static ProfileLanguageItem ToModel(this ProfileLanguageItemResource itemResource)
        {
            if (itemResource == null) return null;

            return new ProfileLanguageItem
            {

                Allowed = itemResource.Allowed,
                Language = (Language) itemResource.Id
            };
        }
        public static ProfileQualityItem ToModel(this ProfileQualityItemResource resource)
        {
            if (resource == null) return null;

            return new ProfileQualityItem
            {
                Quality = (Quality)resource.Quality.Id,
                Allowed = resource.Allowed
            };
        }

        public static ProfileFormatItem ToModel(this ProfileFormatItemResource resource)
        {
            return new ProfileFormatItem
            {
                Format = resource.Format.ToModel(),
                Allowed = resource.Allowed
            };
        }

        public static List<ProfileResource> ToResource(this IEnumerable<Profile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
