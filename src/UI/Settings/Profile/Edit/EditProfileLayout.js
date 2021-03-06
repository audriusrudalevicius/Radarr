var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var Backbone = require('backbone');
var QualitySortableCollectionView = require('./QualitySortableCollectionView');
var SortablePreferredLanguagesView = require('./LanguageSortableCollectionView');
var EditProfileView = require('./EditProfileView');
var DeleteView = require('../DeleteProfileView');
var FullMovieCollection = require('../../../Movies/FullMovieCollection');
var NetImportCollection = require('../../NetImport/NetImportCollection');
var Config = require('../../../Config');
var AsEditModalView = require('../../../Mixins/AsEditModalView');
var LanguageCollection = require('../Language/LanguageCollection');

var view = Marionette.Layout.extend({
    template : 'Settings/Profile/Edit/EditProfileLayoutTemplate',

    regions : {
        fields    : '#x-fields',
        qualities : '#x-qualities',
        preferredLanguages : '#x-preferred-languages',
        formats   : '#x-formats'
    },

    events : {
        'click .x-language-add' : '_addLanguage'
    },

    ui : {
        deleteButton : '.x-delete'
    },

    _deleteView : DeleteView,

    initialize : function(options) {
        this.profileCollection = options.profileCollection;
        this.itemsCollection = new Backbone.Collection(_.toArray(this.model.get('items')).reverse());
        this.netImportCollection = new NetImportCollection();
        this.netImportCollection.fetch();
        this.formatItemsCollection = new Backbone.Collection(_.toArray(this.model.get('formatItems')).reverse());
        this.preferredLanguagesCollection = new Backbone.Collection(_.toArray(this.model.get('preferredLanguages')).reverse());
        this.listenTo(FullMovieCollection, 'all', this._updateDisableStatus);
        this.listenTo(this.netImportCollection, 'all', this._updateDisableStatus);
    },

    templateHelpers : function() {
        return {
            languages : LanguageCollection.toJSON()
        };
    },

    onRender : function() {
        this._updateDisableStatus();
    },

    onShow : function() {
        this.fieldsView = new EditProfileView({ model : this.model });
        this._showFieldsView();
        var advancedShown = Config.getValueBoolean(Config.Keys.AdvancedSettings, false);

        this.sortableListView = new QualitySortableCollectionView({
            selectable     : true,
            selectMultiple : true,
            clickToSelect  : true,
            clickToToggle  : true,
            sortable       : advancedShown,

            sortableOptions : {
                handle : '.x-drag-handle'
            },

            visibleModelsFilter : function(model) {
                var quality = model.get('quality');
                if (quality) {
                    return quality.id !== 0 || advancedShown;
                }

                return true;
            },

            collection : this.itemsCollection,
            model      : this.model
        });

        this.sortableListView.setSelectedModels(this.itemsCollection.filter(function(item) {
            return item.get('allowed') === true;
        }));
        this.qualities.show(this.sortableListView);

        this.sortablePreferredLanguagesView = new SortablePreferredLanguagesView({
            selectable     : true,
            selectMultiple : true,
            clickToSelect  : true,
            clickToToggle  : true,
            sortable: advancedShown,

            sortableOptions : {
                handle : '.x-drag-handle'
            },

            collection : this.preferredLanguagesCollection,
            model      : this.model
        });
        this.sortablePreferredLanguagesView.setSelectedModels(this.preferredLanguagesCollection.filter(function(item) {
            return item.get('allowed') === true;
        }));

        this.preferredLanguages.show(this.sortablePreferredLanguagesView);
        this.sortableFormatListView = new QualitySortableCollectionView({
            selectable     : true,
            selectMultiple : true,
            clickToSelect  : true,
            clickToToggle  : true,
            sortable       : advancedShown,

            sortableOptions : {
                handle : '.x-drag-handle'
            },

            visibleModelsFilter : function(model) {
                var quality = model.get('format');
                if (quality) {
                    return quality.id !== 0 || advancedShown;
                }

                return true;
            },

            collection : this.formatItemsCollection,
            model      : this.model
        });
        this.sortableFormatListView.setSelectedModels(this.formatItemsCollection.filter(function(item) {
            return item.get('allowed') === true;
        }));
        this.formats.show(this.sortableFormatListView);

        this.listenTo(this.sortablePreferredLanguagesView, 'selectionChanged', this._selectionChanged);
        this.listenTo(this.sortablePreferredLanguagesView, 'sortStop', this._updateModel);

        this.listenTo(this.sortableListView, 'selectionChanged', this._selectionChanged);
        this.listenTo(this.sortableListView, 'sortStop', this._updateModel);

        this.listenTo(this.sortableFormatListView, 'selectionChanged', this._selectionChanged);
        this.listenTo(this.sortableFormatListView, 'sortStop', this._updateModel);
    },

    _onBeforeSave : function() {
        var cutoff = this.fieldsView.getCutoff();
        this.model.set('cutoff', cutoff);
    },

    _onAfterSave : function() {
        this.profileCollection.add(this.model, { merge : true });
        vent.trigger(vent.Commands.CloseModalCommand);
    },

    _selectionChanged : function(newSelectedModels, oldSelectedModels) {
        var addedModels = _.difference(newSelectedModels, oldSelectedModels);
        var removeModels = _.difference(oldSelectedModels, newSelectedModels);

        _.each(removeModels, function(item) {
            item.set('allowed', false);
        });
        _.each(addedModels, function(item) {
            item.set('allowed', true);
        });
        this._updateModel();
    },

    _updateModel : function() {
        this.model.set('items', this.itemsCollection.toJSON().reverse());
        this.model.set('formatItems', this.formatItemsCollection.toJSON().reverse());
        this.model.set('preferredLanguages', this.preferredLanguagesCollection.toJSON().reverse());

        this._showFieldsView();
    },

    _showFieldsView : function() {
        this.fields.show(this.fieldsView);
    },

    _updateDisableStatus : function() {
        if (this._isQualityInUse() || this._isQualityInUsebyList()) {
            this.ui.deleteButton.attr('disabled', 'disabled');
            this.ui.deleteButton.addClass('disabled');
            this.ui.deleteButton.attr('title', 'Can\'t delete a profile that is attached to a movie or list.');
        } else {
            this.ui.deleteButton.removeClass('disabled');
        }
    },

    _isQualityInUse : function() {
        return FullMovieCollection.where({ 'profileId' : this.model.id }).length !== 0;
    },

    _isQualityInUsebyList : function() {
        return this.netImportCollection.where({ 'profileId' : this.model.id }).length !== 0;
    },

    _addLanguage : function (e) {
        e.preventDefault();

        var languageSelector = this.$('#x-language-select').first();
        var item = languageSelector.find(':selected').data();
        this.preferredLanguagesCollection.add(_.extend(item, {allowed: true}));
        this.sortablePreferredLanguagesView.setSelectedModels(this.preferredLanguagesCollection.filter(function(item) {
            return item.get('allowed') === true;
        }));
    }
});
module.exports = AsEditModalView.call(view);
