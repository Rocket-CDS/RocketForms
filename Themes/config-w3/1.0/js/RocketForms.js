/**
 * Make div elements with isbutton class accessible like button elements
 * This function adds proper keyboard navigation, ARIA attributes, and semantic behavior
 */
function makeIsButtonsAccessible() {
    // Find all div elements with isbutton class
    const divButtons = document.querySelectorAll('div.isbutton');

    console.log('Found ' + divButtons.length + ' div.isbutton elements to make accessible');

    divButtons.forEach(function (divButton, index) {
        // Skip if already processed
        if (divButton.hasAttribute('data-accessible-button')) {
            return;
        }

        console.log('Processing div.isbutton:', divButton);

        // Mark as processed
        divButton.setAttribute('data-accessible-button', 'true');

        // Add ARIA role
        divButton.setAttribute('role', 'button');

        // Make focusable
        if (!divButton.hasAttribute('tabindex')) {
            divButton.setAttribute('tabindex', '0');
        }

        // Add ARIA label if text content exists but no aria-label
        if (!divButton.hasAttribute('aria-label') && divButton.textContent.trim()) {
            divButton.setAttribute('aria-label', divButton.textContent.trim());
        }

        // Add keyboard event listeners
        divButton.addEventListener('keydown', function (event) {
            // Activate on Enter or Space key
            if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();

                // Add visual feedback
                divButton.classList.add('isbutton-pressed');
                setTimeout(function () {
                    divButton.classList.remove('isbutton-pressed');
                }, 150);

                // Trigger click event if onclick attribute exists
                if (divButton.hasAttribute('onclick')) {
                    try {
                        eval(divButton.getAttribute('onclick'));
                    } catch (e) {
                        console.warn('Error executing onclick for isbutton:', e);
                    }
                }

                // Dispatch click event for other event listeners
                const clickEvent = new Event('click', {
                    bubbles: true,
                    cancelable: true
                });
                divButton.dispatchEvent(clickEvent);
            }
        });

        // Add focus and blur events for better visual feedback
        divButton.addEventListener('focus', function () {
            divButton.classList.add('isbutton-focused');
        });

        divButton.addEventListener('blur', function () {
            divButton.classList.remove('isbutton-focused');
        });

        // Handle disabled state
        if (divButton.hasAttribute('disabled') || divButton.classList.contains('disabled')) {
            divButton.setAttribute('aria-disabled', 'true');
            divButton.setAttribute('tabindex', '-1');
        }

        // Add screen reader support for loading states
        if (divButton.classList.contains('loading') || divButton.textContent.includes('Loading')) {
            divButton.setAttribute('aria-busy', 'true');
        }
    });
}

// CSS styles for accessibility feedback
function addIsButtonAccessibilityStyles() {
    const existingStyle = document.getElementById('isbutton-accessibility-styles');
    if (existingStyle) {
        return; // Styles already added
    }

    const style = document.createElement('style');
    style.id = 'isbutton-accessibility-styles';
    style.textContent = `
        /* Focus styles for accessibility */
        div.isbutton:focus,
        div.isbutton.isbutton-focused {
            outline: 2px solid #2196F3;
            outline-offset: 2px;
            box-shadow: 0 0 0 3px rgba(33, 150, 243, 0.2);
        }
        
        /* Pressed state for visual feedback */
        div.isbutton.isbutton-pressed {
            transform: translateY(1px);
            box-shadow: inset 0 2px 4px rgba(0,0,0,0.2);
        }
        
        /* Disabled state styling */
        div.isbutton[aria-disabled="true"] {
            opacity: 0.6;
            cursor: not-allowed;
            pointer-events: none;
        }
        
        /* Loading state styling */
        div.isbutton[aria-busy="true"] {
            cursor: wait;
        }
        
        /* High contrast mode support */
        @media (prefers-contrast: high) {
            div.isbutton:focus,
            div.isbutton.isbutton-focused {
                outline: 3px solid;
                outline-offset: 2px;
            }
        }
        
        /* Reduced motion support */
        @media (prefers-reduced-motion: reduce) {
            div.isbutton.isbutton-pressed {
                transform: none;
                transition: none;
            }
        }
    `;
    document.head.appendChild(style);
}

// Initialize accessibility features
function initIsButtonAccessibility() {
    console.log('Initializing IsButton Accessibility');

    // Add CSS styles
    addIsButtonAccessibilityStyles();

    // Make existing buttons accessible
    makeIsButtonsAccessible();

    // Watch for dynamically added buttons
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.type === 'childList') {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType === 1) { // Element node
                        // Check if the added node is an isbutton div
                        if (node.matches && node.matches('div.isbutton')) {
                            console.log('New div.isbutton detected via MutationObserver');
                            makeIsButtonsAccessible();
                        }
                        // Check if any descendants are isbutton divs
                        if (node.querySelectorAll) {
                            const newButtons = node.querySelectorAll('div.isbutton');
                            if (newButtons.length > 0) {
                                console.log('New div.isbutton descendants detected:', newButtons.length);
                                makeIsButtonsAccessible();
                            }
                        }
                    }
                });
            }
        });
    });

    // Start observing
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });

    // Also check periodically for dynamically added content
    setTimeout(function () {
        console.log('Running delayed accessibility check for dynamic content');
        makeIsButtonsAccessible();
    }, 1500);

    // Check again after a longer delay to catch any other dynamic content
    setTimeout(function () {
        console.log('Running final accessibility check');
        makeIsButtonsAccessible();
    }, 3000);
}

// Auto-initialize when DOM is ready
(function () {
    function initWhenReady() {
        if (typeof jQuery !== 'undefined' && typeof $ !== 'undefined') {
            $(document).ready(function () {
                console.log('Initializing isbutton accessibility with jQuery');
                initIsButtonAccessibility();
            });
        } else if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function () {
                console.log('Initializing isbutton accessibility on DOMContentLoaded');
                initIsButtonAccessibility();
            });
        } else {
            console.log('Initializing isbutton accessibility immediately');
            initIsButtonAccessibility();
        }
    }

    // Initialize immediately if possible, otherwise wait
    initWhenReady();
})();