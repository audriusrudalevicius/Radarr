var _ = require('underscore');
var Handlebars = require('handlebars');

Handlebars.registerHelper('languageLabel', function() {
    var languages = this.preferredLanguages.reverse();
    var result = '';
    for (var index in languages) {
        if (languages.hasOwnProperty(index) && index < 4)
            result = result + '<span class="label label-primary">' + languages[index].name + '</span> ';
    }

    return new Handlebars.SafeString(result);
});
