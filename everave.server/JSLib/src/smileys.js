import {
    Plugin,
    DropdownView,
    ButtonView,
    IconEmoji,
    createDropdown
} from 'ckeditor5';

export default class SmileyPlugin extends Plugin {
    init() {
        const editor = this.editor;

        const smileySet = editor.config.get('smilyes.smileySet') || [];

        editor.ui.componentFactory.add('addSmiley', locale => {
            const dropdown = createDropdown(locale);

            dropdown.buttonView.set({
                label: 'Smileys',
                tooltip: true,
                icon: IconEmoji
            });

            const items = smileySet.map(smiley => {
                const button = new ButtonView(locale);

                button.set({
                    label: smiley,
                    withText: true
                });

                button.on('execute', () => {
                    console.log(`Selected smiley: ${smiley}`);
                    alert(`You selected: ${smiley}`);
                });

                return button;
            });

            dropdown.panelView.children.addMany(items);

            return dropdown;
        });
    }
}
