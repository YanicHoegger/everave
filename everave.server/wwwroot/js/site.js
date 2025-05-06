function scrollToElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: "smooth", block: "center" });
    }
}

window.registerOutsideClickHandler = (dotNetHelper, popupSelector, buttonSelector) => {
    document.addEventListener('click', (event) => {
        const popup = document.querySelector(popupSelector);
        const button = document.querySelector(buttonSelector);

        // Check if the click is outside the popup and button
        if (popup && button && !popup.contains(event.target) && !button.contains(event.target)) {
            dotNetHelper.invokeMethodAsync('ClosePopup');
        } else if (popup && popup.contains(event.target)) {
            // Check if the click is on a button or link inside the popup
            const clickedElement = event.target;
            if (clickedElement.tagName === 'BUTTON' || clickedElement.tagName === 'A' || clickedElement.closest('button') || clickedElement.closest('a')) {
                dotNetHelper.invokeMethodAsync('ClosePopup');
            }
        }
    });
};