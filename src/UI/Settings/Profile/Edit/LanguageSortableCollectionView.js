var BackboneSortableCollectionView = require('backbone.collectionview');
var EditProfileItemView = require('./EditProfileLanguageItemView');

module.exports = BackboneSortableCollectionView.extend({
    className : 'preferred-languages',
    modelView : EditProfileItemView,

    attributes : {
        'validation-name' : 'items'
    },

    selectableModelsFilter : function( model ) {
        return model.get( "id" ) > -1;
    },

    events : {
        'click li, td'    : '_listItem_onMousedown',
        'dblclick li, td' : '_listItem_onDoubleClick',
        'keydown'         : '_onKeydown'
    }
});
