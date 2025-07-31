window.commonJsInterop = {
    registerClickOutsideListener: function (elementId, dotNetInstance) {
        try { 
            const el = document.getElementById(elementId);
            if (!el) {
                console.error(`Element with ID ${elementId} not found.`);
                return false;
            }

            const handleClickOutside = (e) => {
                if (!document.getElementById(elementId).contains(e.target)) {
                    dotNetInstance.invokeMethodAsync('HandleClickOutside')
                        .catch(err => console.error('Error invoking HandleClickOutside:', err));
                }
            };

            window.addEventListener('click', (e) => handleClickOutside(e));
        } catch(err) {
            console.error('registerViewportSizeEventListener failed.', err);
        }
    }
}