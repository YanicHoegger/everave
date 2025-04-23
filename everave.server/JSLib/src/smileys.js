import {
    Plugin,
    IconEmoji,
    View,
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

            dropdown.render();

            smileySet.forEach(smileyPath => {
                const smileyView = new View(locale);

                smileyView.setTemplate({
                    tag: 'img',
                    attributes: {
                        src: smileyPath,
                        alt: 'Smiley',
                        style: 'width: 32px; height: 32px; cursor: pointer; margin: 4px;',
                        tabindex: '-1'
                    }
                });

                smileyView.on('render', () => {
                    smileyView.element.addEventListener('click', () => {
                        editor.model.change(writer => {
                            const imgHtml = `<img src="${smileyPath}" alt="Smiley" style="width: 32px; height: 32px;">`;
                            const viewFragment = editor.data.processor.toView(imgHtml);
                            const modelFragment = editor.data.toModel(viewFragment);

                            editor.model.insertContent(modelFragment, editor.model.document.selection);
                        });
                    });
                });

                smileyView.focus = () => {
                    smileyView.element.focus();
                };

                dropdown.panelView.children.add(smileyView);
            });

            return dropdown;
        });
    }
}