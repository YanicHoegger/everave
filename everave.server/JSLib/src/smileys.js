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

            const smileyWrapper = new View(locale);

            // Define the template with children directly
            smileyWrapper.setTemplate({
                tag: 'div',
                attributes: {
                    class: 'smiley-dropdown-panel'
                },
                children: smileySet.map(smileyPath => {
                    const smileyView = new View(locale);

                    smileyView.setTemplate({
                        tag: 'img',
                        attributes: {
                            src: smileyPath,
                            alt: 'Smiley',
                            style: 'cursor: pointer; margin: 4px;',
                            tabindex: '-1'
                        }
                    });

                    smileyView.on('render', () => {
                        smileyView.element.addEventListener('click', () => {
                            editor.model.change(writer => {
                                const imgHtml = `<img src="${smileyPath}">`;
                                const viewFragment = editor.data.processor.toView(imgHtml);
                                const modelFragment = editor.data.toModel(viewFragment);

                                editor.model.insertContent(modelFragment, editor.model.document.selection);
                            });
                        });
                    });

                    smileyView.focus = () => {
                        smileyView.element.focus();
                    };

                    return smileyView;
                })
            });

            dropdown.panelView.children.add(smileyWrapper);

            return dropdown;
        });
    }
}