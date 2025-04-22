import {
	ClassicEditor,
	Autoformat,
	Bold,
	Italic,
	Underline,
	BlockQuote,
	Essentials,
	Heading,
	Image,
	ImageCaption,
	ImageResize,
	ImageStyle,
	ImageToolbar,
	ImageUpload,
	PictureEditing,
	Indent,
	IndentBlock,
	Link,
	List,
	MediaEmbed,
	Mention,
	Paragraph,
	PasteFromOffice,
	Table,
	TableColumnResize,
	TableToolbar,
	TextTransformation,
	SimpleUploadAdapter
} from 'ckeditor5';

import SmileyPlugin from './smileys.js';

export function initializeCKEditor(editorId, dotNetHelper) {
    ClassicEditor
        .create(document.querySelector(`#${editorId}`), {
            licenseKey: 'GPL',
			plugins: [
				Autoformat,
				BlockQuote,
				Bold,
				Essentials,
				Heading,
				Image,
				ImageCaption,
				ImageResize,
				ImageStyle,
				ImageToolbar,
				ImageUpload,
				Indent,
				IndentBlock,
				Italic,
				Link,
				List,
				MediaEmbed,
				Mention,
				Paragraph,
				PasteFromOffice,
				PictureEditing,
				SimpleUploadAdapter,
				Table,
				TableColumnResize,
				TableToolbar,
				TextTransformation,
				Underline,
				SmileyPlugin
			],
			toolbar: [
				'undo',
				'redo',
				'|',
				'heading',
				'|',
				'bold',
				'italic',
				'underline',
				'|',
				'link',
				'uploadImage',
				'ckbox',
				'insertTable',
				'blockQuote',
				'mediaEmbed',
				'|',
				'bulletedList',
				'numberedList',
				'|',
				'outdent',
				'indent',
				'|',
				'addSmiley'
			],
			heading: {
				options: [
					{
						model: 'paragraph',
						title: 'Paragraph',
						class: 'ck-heading_paragraph'
					},
					{
						model: 'heading1',
						view: 'h1',
						title: 'Heading 1',
						class: 'ck-heading_heading1'
					},
					{
						model: 'heading2',
						view: 'h2',
						title: 'Heading 2',
						class: 'ck-heading_heading2'
					},
					{
						model: 'heading3',
						view: 'h3',
						title: 'Heading 3',
						class: 'ck-heading_heading3'
					},
					{
						model: 'heading4',
						view: 'h4',
						title: 'Heading 4',
						class: 'ck-heading_heading4'
					}
				]
			},
			image: {
				resizeOptions: [
					{
						name: 'resizeImage:original',
						label: 'Default image width',
						value: null
					},
					{
						name: 'resizeImage:50',
						label: '25% page width',
						value: '25'
					},
					{
						name: 'resizeImage:50',
						label: '50% page width',
						value: '50'
					},
					{
						name: 'resizeImage:75',
						label: '75% page width',
						value: '75'
					}
				],
				toolbar: [
					'imageTextAlternative',
					'toggleImageCaption',
					'|',
					'imageStyle:inline',
					'imageStyle:wrapText',
					'imageStyle:breakText',
					'|',
					'resizeImage'
				]
			},
			link: {
				addTargetToExternalLinks: true,
				defaultProtocol: 'https://'
			},
			table: {
				contentToolbar: ['tableColumn', 'tableRow', 'mergeTableCells']
			},
            simpleUpload: {
				uploadUrl: '/image/upload',
			},
			smilyes: {
				smileySet: ['😊', '😂', '😍', '😎', '😢'] // Example configuration
			}
        })
        .then(editor => {
            editor.model.document.on('change:data', () => {
                dotNetHelper.invokeMethodAsync('OnEditorDataChanged', editor.getData());
            });
        })
        .catch(error => {
            console.error(error);
	     });
}